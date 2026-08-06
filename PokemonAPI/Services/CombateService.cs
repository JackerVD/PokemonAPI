using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using PokemonAPI.Data;
using PokemonAPI.DTOs;
using PokemonAPI.DTOs.Create;
using PokemonAPI.Logic;
using PokemonAPI.Models;

namespace PokemonAPI.Services
{
    public class CombateService
    {
        private readonly ConcurrentDictionary<Guid, CombateEstadoInterno> _combates = new();
        private readonly DanoService _danoService;
        private readonly Random _random = new();

        public CombateService(DanoService danoService)
        {
            _danoService = danoService;
        }

        /// <summary>
        /// Crea un combate en memoria con los MiPokemons del jugador y enemigos aleatorios.
        /// </summary>
        /// <param name="db">Contexto de base de datos</param>
        /// <param name="dto">Datos para crear el combate</param>
        /// <returns>El estado inicial del combate</returns>
        /// <exception cref="InvalidOperationException">Lanzada si la configuración es inválida</exception>
        public async Task<EstadoCombateDto> CrearCombateAsync(PokemonDbContext db, CreateCombateDto dto)
        {
            var misPokemons = await db.MisPokemons
                .Include(mp => mp.PokemonBase).ThenInclude(p => p.Tipos)
                .Include(mp => mp.Movimientos).ThenInclude(mm => mm.Movimiento)
                .Where(mp => dto.MiPokemonIds.Contains(mp.MiPokemonId))
                .ToListAsync();

            if (misPokemons.Count != dto.MiPokemonIds.Distinct().Count())
                throw new InvalidOperationException("Uno o mas MiPokemons no existen.");

            var enemigosBase = await db.Pokemons
                .Include(p => p.Tipos)
                .Include(p => p.Movimientos).ThenInclude(pm => pm.Movimiento)
                .Where(p => p.Movimientos.Any())
                .ToListAsync();

            if (enemigosBase.Count < dto.CantidadEnemigos)
                throw new InvalidOperationException("No hay suficientes Pokemons enemigos con movimientos cargados.");

            var seleccionEnemigos = enemigosBase
                .OrderBy(_ => _random.Next())
                .Take(dto.CantidadEnemigos)
                .ToList();

            var estado = new CombateEstadoInterno
            {
                CombateId = Guid.NewGuid(),
                Estado = EstadoCombate.EnCurso,
                Turno = 1,
                EquipoJugador = misPokemons.Select(MapJugador).ToList(),
                EquipoEnemigo = seleccionEnemigos.Select(MapEnemigo).ToList()
            };

            if (!_combates.TryAdd(estado.CombateId, estado))
                throw new InvalidOperationException("No se pudo crear el combate.");

            return MapEstado(estado);
        }

        /// <summary>
        /// Obtiene el estado actual de un combate.
        /// </summary>
        /// <param name="combateId">Identificador del combate</param>
        /// <returns>El estado del combate</returns>
        /// <exception cref="KeyNotFoundException">Lanzada si el combate no existe</exception>
        public EstadoCombateDto Obtener(Guid combateId)
        {
            if (!_combates.TryGetValue(combateId, out var estado))
                throw new KeyNotFoundException("Combate no encontrado.");

            lock (estado.SyncRoot)
            {
                return MapEstado(estado);
            }
        }

        /// <summary>
        /// Devuelve el historial completo de eventos de un combate.
        /// </summary>
        /// <param name="combateId">Identificador del combate</param>
        /// <returns>La lista de eventos del combate</returns>
        /// <exception cref="KeyNotFoundException">Lanzada si el combate no existe</exception>
        public IReadOnlyList<string> ObtenerHistorial(Guid combateId)
        {
            if (!_combates.TryGetValue(combateId, out var estado))
                throw new KeyNotFoundException("Combate no encontrado.");

            lock (estado.SyncRoot)
            {
                return estado.HistorialEventos.ToList();
            }
        }

        /// <summary>
        /// Ejecuta un turno del combate.
        /// </summary>
        /// <param name="combateId">Identificador del combate</param>
        /// <returns>El estado actualizado del combate</returns>
        /// <exception cref="KeyNotFoundException">Lanzada si el combate no existe</exception>
        /// <exception cref="InvalidOperationException">Lanzada si el combate ya está finalizado</exception>
        public Task<EstadoCombateDto> EjecutarTurnoAsync(Guid combateId)
        {
            if (!_combates.TryGetValue(combateId, out var estado))
                throw new KeyNotFoundException("Combate no encontrado.");

            lock (estado.SyncRoot)
            {
                if (estado.Estado == EstadoCombate.Finalizado)
                    throw new InvalidOperationException("El combate ya esta finalizado.");

                var eventosTurno = new List<string>();

                AjustarActivos(estado);

                var jugador = estado.EquipoJugador[estado.ActivoJugador];
                var enemigo = estado.EquipoEnemigo[estado.ActivoEnemigo];

                var movJugador = jugador.Movimientos[_random.Next(jugador.Movimientos.Count)];
                var movEnemigo = enemigo.Movimientos[_random.Next(enemigo.Movimientos.Count)];

                var primeroJugador = jugador.Velocidad > enemigo.Velocidad
                    || (jugador.Velocidad == enemigo.Velocidad && _random.Next(2) == 0);

                if (primeroJugador)
                {
                    Atacar(jugador, enemigo, movJugador, eventosTurno);
                    if (!enemigo.Debilitado) Atacar(enemigo, jugador, movEnemigo, eventosTurno);
                }
                else
                {
                    Atacar(enemigo, jugador, movEnemigo, eventosTurno);
                    if (!jugador.Debilitado) Atacar(jugador, enemigo, movJugador, eventosTurno);
                }

                ResolverFin(estado, eventosTurno);

                if (estado.Estado == EstadoCombate.EnCurso)
                {
                    estado.Turno++;
                    AjustarActivos(estado);
                }

                estado.UltimosEventos = eventosTurno;
                estado.HistorialEventos.AddRange(eventosTurno);

                return Task.FromResult(MapEstado(estado));
            }
        }

        /// <summary>
        /// Simula un combate completo hasta que uno de los equipos gane o se alcance el número máximo de turnos.
        /// </summary>
        /// <param name="combateId">Identificador del combate.</param>
        /// <param name="maxTurnos">Número máximo de turnos a simular.</param>
        /// <returns>Estado del combate tras la simulación.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Lanzada si maxTurnos es menor que 1.</exception>
        /// <exception cref="KeyNotFoundException">Lanzada si el combate no existe.</exception>
        public async Task<EstadoCombateDto> SimularCombateCompletoAsync(Guid combateId, int maxTurnos = 100)
        {
            if (maxTurnos < 1)
                throw new ArgumentOutOfRangeException(nameof(maxTurnos), "maxTurnos debe ser mayor o igual que 1.");

            if (!_combates.TryGetValue(combateId, out var estado))
                throw new KeyNotFoundException("Combate no encontrado.");

            int turnosEjecutados = 0;

            while (turnosEjecutados < maxTurnos)
            {
                // Releer estado por si el combate ya terminó en una iteración previa
                var snapshot = Obtener(combateId);
                if (snapshot.Estado == "Finalizado")
                    return snapshot;

                await EjecutarTurnoAsync(combateId);
                turnosEjecutados++;
            }

            // Si llega aquí, se detuvo por límite de turnos
            if (_combates.TryGetValue(combateId, out var estadoFinal))
            {
                lock (estadoFinal.SyncRoot)
                {
                    estadoFinal.HistorialEventos.Add($"Simulación detenida al alcanzar el límite de {maxTurnos} turnos.");
                }
            }

            return Obtener(combateId);
        }

        /// <summary>
        /// Elimina un combate del almacenamiento en memoria.
        /// </summary>
        /// <param name="combateId">Identificador del combate</param>
        /// <returns>True si se elimina; False si no existe</returns>
        public bool Eliminar(Guid combateId) => _combates.TryRemove(combateId, out _);

        /// <summary>
        /// Mapea un MiPokemon a un CombatienteInterno para el combate.
        /// </summary>
        /// <param name="mp">El MiPokemon a mapear</param>
        /// <returns>El combatiente interno correspondiente</returns>
        /// <exception cref="InvalidOperationException">Lanzada si el MiPokemon no tiene movimientos disponibles</exception>
        private CombatienteInterno MapJugador(MiPokemon mp)
        {
            var baseP = mp.PokemonBase;
            var movimientos = mp.Movimientos
                .OrderBy(m => m.Slot)
                .Select(m => new MovimientoInterno(
                    m.MovimientoId,
                    m.Movimiento.Nombre,
                    m.Movimiento.Tipo,
                    m.Movimiento.Poder,
                    m.Movimiento.Categoria))
                .ToList();

            if (movimientos.Count == 0)
            {
                movimientos = baseP.Movimientos
                    .Take(4)
                    .Select(m => new MovimientoInterno(
                        m.MovimientoId,
                        m.Movimiento.Nombre,
                        m.Movimiento.Tipo,
                        m.Movimiento.Poder,
                        m.Movimiento.Categoria))
                    .ToList();
            }

            if (movimientos.Count == 0)
                throw new InvalidOperationException($"MiPokemon {mp.MiPokemonId} no tiene movimientos.");

            return new CombatienteInterno
            {
                Nombre = mp.NombrePersonalizado ?? baseP.Nombre,
                SaludMaxima = baseP.SaludTotalBase,
                SaludActual = Math.Min(Math.Max(1, mp.SaludActual), baseP.SaludTotalBase),
                Ataque = baseP.AtaqueBase,
                Defensa = baseP.DefensaBase,
                AtaqueEspecial = baseP.AtaqueEspecialBase,
                DefensaEspecial = baseP.DefensaEspecialBase,
                Velocidad = baseP.VelocidadBase,
                Tipos = baseP.Tipos.Select(t => t.Tipo).Take(2).ToList(),
                Movimientos = movimientos
            };
        }

        /// <summary>
        /// Mapea un Pokemon enemigo a un CombatienteInterno para el combate.
        /// </summary>
        /// <param name="p">El Pokemon enemigo a mapear</param>
        /// <returns>El combatiente interno correspondiente</returns>
        /// <exception cref="InvalidOperationException">Lanzada si el Pokemon enemigo no tiene movimientos disponibles</exception>
        private CombatienteInterno MapEnemigo(Pokemon p)
        {
            var movimientos = p.Movimientos
                .Take(4)
                .Select(m => new MovimientoInterno(
                    m.MovimientoId,
                    m.Movimiento.Nombre,
                    m.Movimiento.Tipo,
                    m.Movimiento.Poder,
                    m.Movimiento.Categoria))
                .ToList();

            if (movimientos.Count == 0)
                throw new InvalidOperationException($"Pokemon enemigo {p.Nombre} no tiene movimientos.");

            return new CombatienteInterno
            {
                Nombre = p.Nombre,
                SaludMaxima = p.SaludTotalBase,
                SaludActual = p.SaludTotalBase,
                Ataque = p.AtaqueBase,
                Defensa = p.DefensaBase,
                AtaqueEspecial = p.AtaqueEspecialBase,
                DefensaEspecial = p.DefensaEspecialBase,
                Velocidad = p.VelocidadBase,
                Tipos = p.Tipos.Select(t => t.Tipo).Take(2).ToList(),
                Movimientos = movimientos
            };
        }

        /// <summary>
        /// Aplica un ataque de un combatiente a otro y registra el resultado en el log.
        /// </summary>
        /// <param name="atacante">El combatiente que ataca</param>
        /// <param name="defensor">El combatiente que defiende</param>
        /// <param name="mov">El movimiento utilizado</param>
        /// <param name="log">La lista donde se registran los resultados</param>
        private void Atacar(CombatienteInterno atacante, CombatienteInterno defensor, MovimientoInterno mov, List<string> log)
        {
            if (mov.Categoria == CategoriaMovimiento.Estado || mov.Poder <= 0)
            {
                log.Add($"{atacante.Nombre} usa {mov.Nombre}, pero no hace dano directo.");
                return;
            }

            var atacanteDto = ToPokemonDtoCombate(atacante);
            var defensorDto = ToPokemonDtoCombate(defensor);
            var movDto = ToMovimientoDtoCombate(mov);

            var dano = _danoService.CalcularDano(atacanteDto, defensorDto, movDto);
            var efectividad = TablaTipo.GetEfectividad(mov.Tipo, defensor.Tipos);

            if (efectividad == 0 || dano <= 0)
            {
                log.Add($"{atacante.Nombre} usa {mov.Nombre}, pero no afecta a {defensor.Nombre}.");
                return;
            }

            defensor.SaludActual = Math.Max(0, defensor.SaludActual - dano);

            var textoEf = efectividad switch
            {
                < 1 => "Poco eficaz",
                > 1 => "Super eficaz",
                _ => "Normal"
            };

            log.Add($"{atacante.Nombre} usa {mov.Nombre}: {dano} de dano ({textoEf}). {defensor.Nombre} queda en {defensor.SaludActual}/{defensor.SaludMaxima}.");
        }

        /// <summary>
        /// Ajusta los índices de los combatientes activos si alguno está debilitado.
        /// </summary>
        /// <param name="estado">El estado del combate</param>
        private static void AjustarActivos(CombateEstadoInterno estado)
        {
            if (estado.ActivoJugador < 0 || estado.EquipoJugador[estado.ActivoJugador].Debilitado)
                estado.ActivoJugador = SiguienteVivo(estado.EquipoJugador);

            if (estado.ActivoEnemigo < 0 || estado.EquipoEnemigo[estado.ActivoEnemigo].Debilitado)
                estado.ActivoEnemigo = SiguienteVivo(estado.EquipoEnemigo);
        }

        private static int SiguienteVivo(List<CombatienteInterno> equipo)
            => equipo.FindIndex(c => !c.Debilitado);

        /// <summary>
        /// Resuelve si el combate ha terminado y establece el ganador si es así.
        /// </summary>
        /// <param name="estado">El estado del combate</param>
        /// <param name="log">La lista donde se registran los resultados</param>
        private static void ResolverFin(CombateEstadoInterno estado, List<string> log)
        {
            var jugadorSinVivos = estado.EquipoJugador.All(c => c.Debilitado);
            var enemigoSinVivos = estado.EquipoEnemigo.All(c => c.Debilitado);

            if (!jugadorSinVivos && !enemigoSinVivos) return;

            estado.Estado = EstadoCombate.Finalizado;
            estado.Ganador = enemigoSinVivos ? "Jugador" : "Enemigo";
            log.Add($"Combate finalizado. Ganador: {estado.Ganador}.");
        }

        /// <summary>
        /// Mapea el estado interno del combate a un DTO para exponerlo al cliente.
        /// </summary>
        /// <param name="e">El estado interno del combate</param>
        /// <returns>El DTO del estado del combate</returns>
        private static EstadoCombateDto MapEstado(CombateEstadoInterno e)
        {
            var activoJ = e.ActivoJugador >= 0 ? e.EquipoJugador[e.ActivoJugador] : null;
            var activoE = e.ActivoEnemigo >= 0 ? e.EquipoEnemigo[e.ActivoEnemigo] : null;

            return new EstadoCombateDto
            {
                CombateId = e.CombateId,
                Estado = e.Estado.ToString(),
                Turno = e.Turno,
                Ganador = e.Ganador,
                ActivoJugador = activoJ is null ? new() : new CombatienteActivoDto
                {
                    Nombre = activoJ.Nombre,
                    SaludActual = activoJ.SaludActual,
                    SaludMaxima = activoJ.SaludMaxima,
                    Movimientos = activoJ.Movimientos.Select(m => m.Nombre).ToList()
                },
                ActivoEnemigo = activoE is null ? new() : new CombatienteActivoDto
                {
                    Nombre = activoE.Nombre,
                    SaludActual = activoE.SaludActual,
                    SaludMaxima = activoE.SaludMaxima,
                    Movimientos = activoE.Movimientos.Select(m => m.Nombre).ToList()
                },
                EquipoJugador = e.EquipoJugador.Select(c => new CombatienteResumenDto
                {
                    Nombre = c.Nombre,
                    SaludActual = c.SaludActual,
                    SaludMaxima = c.SaludMaxima,
                    Debilitado = c.Debilitado
                }).ToList(),
                EquipoEnemigo = e.EquipoEnemigo.Select(c => new CombatienteResumenDto
                {
                    Nombre = c.Nombre,
                    SaludActual = c.SaludActual,
                    SaludMaxima = c.SaludMaxima,
                    Debilitado = c.Debilitado
                }).ToList(),
                UltimosEventos = e.UltimosEventos
            };
        }

        private static PokemonDTO ToPokemonDtoCombate(CombatienteInterno c)
        {
            return new PokemonDTO
            {
                Nombre = c.Nombre,
                Nivel = 50,
                SaludTotalBase = c.SaludMaxima,
                AtaqueBase = c.Ataque,
                DefensaBase = c.Defensa,
                AtaqueEspecialBase = c.AtaqueEspecial,
                DefensaEspecialBase = c.DefensaEspecial,
                VelocidadBase = c.Velocidad,
                Tipos = c.Tipos
            };
        }

        private static MovimientoDTO ToMovimientoDtoCombate(MovimientoInterno m)
        {
            return new MovimientoDTO
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Tipo = m.Tipo,
                Poder = m.Poder,
                Categoria = m.Categoria
            };
        }

        private enum EstadoCombate
        {
            EnCurso = 0,
            Finalizado = 1
        }

        private sealed class CombateEstadoInterno
        {
            public object SyncRoot { get; } = new();
            public Guid CombateId { get; set; }
            public EstadoCombate Estado { get; set; } = EstadoCombate.EnCurso;
            public int Turno { get; set; } = 1;
            public string? Ganador { get; set; }

            public int ActivoJugador { get; set; } = 0;
            public int ActivoEnemigo { get; set; } = 0;

            public List<CombatienteInterno> EquipoJugador { get; set; } = [];
            public List<CombatienteInterno> EquipoEnemigo { get; set; } = [];
            public List<string> UltimosEventos { get; set; } = [];
            public List<string> HistorialEventos { get; set; } = [];
        }

        private sealed class CombatienteInterno
        {
            public string Nombre { get; set; } = string.Empty;
            public int SaludActual { get; set; }
            public int SaludMaxima { get; set; }
            public int Ataque { get; set; }
            public int Defensa { get; set; }
            public int AtaqueEspecial { get; set; }
            public int DefensaEspecial { get; set; }
            public int Velocidad { get; set; }
            public List<Tipo> Tipos { get; set; } = [];
            public List<MovimientoInterno> Movimientos { get; set; } = [];
            public bool Debilitado => SaludActual <= 0;
        }

        private sealed record MovimientoInterno(
            int Id,
            string Nombre,
            Tipo Tipo,
            int Poder,
            CategoriaMovimiento Categoria);
    }
}