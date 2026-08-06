using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonAPI.Data;
using PokemonAPI.DTOs;
using PokemonAPI.DTOs.Create;
using PokemonAPI.DTOs.Update;
using PokemonAPI.Models;

namespace PokemonAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MisPokemonsController : ControllerBase
    {
        private readonly PokemonDbContext _context;

        public MisPokemonsController(PokemonDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la lista de todos los MiPokemons con sus datos base, tipos y movimientos asignados.
        /// </summary>
        /// <returns>200 con la colección de MiPokemons.</returns>
        // GET: api/mispokemons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MiPokemonDTO>>> GetMisPokemons()
        {
            var misPokemons = await BuildDetailQuery(asNoTracking: true)
                .OrderBy(mp => mp.NombrePersonalizado ?? mp.PokemonBase.Nombre)
                .ToListAsync();

            return Ok(misPokemons.Select(ToDto).ToList());
        }

        /// <summary>
        /// Obtiene un MiPokemon por su identificador.
        /// </summary>
        /// <param name="id">Identificador del MiPokemon.</param>
        /// <returns>
        /// 200 con el detalle del MiPokemon si existe; 404 si no se encuentra.
        /// </returns>
        // GET: api/mispokemons/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MiPokemonDTO>> GetMiPokemon(int id)
        {
            var miPokemon = await BuildDetailQuery(asNoTracking: true)
                .FirstOrDefaultAsync(mp => mp.MiPokemonId == id);

            if (miPokemon is null)
                return NotFound();

            return Ok(ToDto(miPokemon));
        }

        /// <summary>
        /// Crea un nuevo MiPokemon a partir de un Pokemon base.
        /// </summary>
        /// <remarks>
        /// Si no se envía salud actual, se utiliza la salud base del Pokemon.
        /// </remarks>
        /// <param name="dto">Datos de creación del MiPokemon.</param>
        /// <returns>201 con el recurso creado; 400 si hay validaciones de negocio inválidas.</returns>
        // POST: api/mispokemons
        [HttpPost]
        public async Task<ActionResult<MiPokemonDTO>> CreateMiPokemon([FromBody] CreateMiPokemonDto dto)
        {
            var pokemonBase = await _context.Pokemons
                .FirstOrDefaultAsync(p => p.PokemonId == dto.PokemonId);

            if (pokemonBase is null)
                return BadRequest("El Pokemon base no existe.");

            if (dto.Nivel < 1 || dto.Nivel > 100)
                return BadRequest("El nivel debe estar entre 1 y 100.");

            var saludInicial = dto.SaludActual ?? pokemonBase.SaludTotalBase;
            if (saludInicial < 0)
                return BadRequest("La salud actual no puede ser negativa.");

            if (saludInicial > pokemonBase.SaludTotalBase)
                return BadRequest($"La salud actual no puede ser mayor que la salud base ({pokemonBase.SaludTotalBase}).");

            var miPokemon = new MiPokemon
            {
                PokemonId = dto.PokemonId,
                NombrePersonalizado = NormalizeCustomName(dto.NombrePersonalizado),
                Nivel = dto.Nivel,
                SaludActual = saludInicial
            };

            _context.MisPokemons.Add(miPokemon);
            await _context.SaveChangesAsync();

            var created = await BuildDetailQuery(asNoTracking: true)
                .FirstAsync(mp => mp.MiPokemonId == miPokemon.MiPokemonId);

            return CreatedAtAction(nameof(GetMiPokemon), new { id = created.MiPokemonId }, ToDto(created));
        }

        /// <summary>
        /// Actualiza parcialmente un MiPokemon existente.
        /// </summary>
        /// <remarks>
        /// Permite actualizar nombre personalizado, nivel y/o salud actual.
        /// Si no se envía ningún campo, devuelve 400.
        /// </remarks>
        /// <param name="id">Identificador del MiPokemon.</param>
        /// <param name="dto">Campos a actualizar.</param>
        /// <returns>204 si se actualiza, 404 si no existe, 400 si la petición es inválida.</returns>
        // PUT: api/mispokemons/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMiPokemon(int id, [FromBody] UpdateMiPokemonDto dto)
        {
            if (dto.NombrePersonalizado is null && !dto.Nivel.HasValue && !dto.SaludActual.HasValue)
                return BadRequest("Debes enviar al menos un campo para actualizar.");

            var miPokemon = await _context.MisPokemons
                .Include(mp => mp.PokemonBase)
                .FirstOrDefaultAsync(mp => mp.MiPokemonId == id);

            if (miPokemon is null)
                return NotFound();

            var saludBase = miPokemon.PokemonBase.SaludTotalBase;

            if (dto.Nivel.HasValue)
            {
                if (dto.Nivel.Value < 1 || dto.Nivel.Value > 100)
                    return BadRequest("El nivel debe estar entre 1 y 100.");

                miPokemon.Nivel = dto.Nivel.Value;
            }

            if (dto.SaludActual.HasValue)
            {
                if (dto.SaludActual.Value < 0)
                    return BadRequest("La salud actual no puede ser negativa.");

                if (dto.SaludActual.Value > saludBase)
                    return BadRequest($"La salud actual no puede ser mayor que la salud base ({saludBase}).");

                miPokemon.SaludActual = dto.SaludActual.Value;
            }

            if (dto.NombrePersonalizado is not null)
                miPokemon.NombrePersonalizado = NormalizeCustomName(dto.NombrePersonalizado);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Elimina un MiPokemon por su identificador.
        /// </summary>
        /// <param name="id">Identificador del MiPokemon.</param>
        /// <returns>
        /// 204 si se elimina; 404 si no existe; 409 si tiene restricciones de datos relacionadas.
        /// </returns>
        // DELETE: api/mispokemons/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMiPokemon(int id)
        {
            var miPokemon = await _context.MisPokemons.FindAsync(id);
            if (miPokemon is null)
                return NotFound();

            _context.MisPokemons.Remove(miPokemon);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("No se puede eliminar el MiPokemon porque tiene datos relacionados.");
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene los movimientos posibles del Pokemon base asociado a un MiPokemon.
        /// </summary>
        /// <param name="id">Identificador del MiPokemon.</param>
        /// <returns>200 con la lista de movimientos posibles; 404 si el MiPokemon no existe.</returns>
        // GET: api/mispokemons/{id}/movimientos-posibles
        [HttpGet("{id:int}/movimientos-posibles")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientosPosiblesDeMiPokemon(int id)
        {
            var miPokemon = await _context.MisPokemons
                .AsNoTracking()
                .FirstOrDefaultAsync(mp => mp.MiPokemonId == id);

            if (miPokemon is null)
                return NotFound();

            var movimientos = await _context.PokemonMovimientosPosibles
                .AsNoTracking()
                .Where(pm => pm.PokemonId == miPokemon.PokemonId)
                .Select(pm => new MovimientoDTO
                {
                    Id = pm.MovimientoId,
                    Nombre = pm.Movimiento.Nombre,
                    Tipo = pm.Movimiento.Tipo,
                    Categoria = pm.Movimiento.Categoria,
                    Poder = pm.Movimiento.Poder
                })
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return Ok(movimientos);
        }

        /// <summary>
        /// Construye la consulta base para obtener los detalles de un MiPokemon, incluyendo su Pokemon base, tipos y movimientos.
        /// </summary>
        /// <param name="asNoTracking">Indica si la consulta debe ser ejecutada sin seguimiento.</param>
        /// <returns>La consulta IQueryable para obtener los detalles del MiPokemon.</returns>
        private IQueryable<MiPokemon> BuildDetailQuery(bool asNoTracking)
        {
            var query = _context.MisPokemons
                .Include(mp => mp.PokemonBase)
                    .ThenInclude(p => p.Tipos)
                .Include(mp => mp.Movimientos)
                    .ThenInclude(mm => mm.Movimiento)
                .AsQueryable();

            return asNoTracking ? query.AsNoTracking() : query;
        }

        /// <summary>
        /// Normaliza el nombre personalizado de un MiPokemon, eliminando espacios en blanco y convirtiendo cadenas vacías a null.
        /// </summary>
        /// <param name="nombre">El nombre personalizado del MiPokemon.</param>
        /// <returns>El nombre normalizado o null si estaba vacío.</returns>
        private static string? NormalizeCustomName(string? nombre)
        {
            return string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim();
        }

        private static MiPokemonDTO ToDto(MiPokemon mp)
        {
            var p = mp.PokemonBase;

            return new MiPokemonDTO
            {
                MiPokemonId = mp.MiPokemonId,
                PokemonBaseId = p.PokemonId,
                PokemonBaseNombre = p.Nombre,

                NombrePersonalizado = mp.NombrePersonalizado,
                NombreMostrado = mp.NombrePersonalizado ?? p.Nombre,

                Nivel = mp.Nivel,
                SaludActual = mp.SaludActual,

                SaludTotalBase = p.SaludTotalBase,
                AtaqueBase = p.AtaqueBase,
                DefensaBase = p.DefensaBase,
                AtaqueEspecialBase = p.AtaqueEspecialBase,
                DefensaEspecialBase = p.DefensaEspecialBase,
                VelocidadBase = p.VelocidadBase,

                Tipos = p.Tipos.Select(t => t.Tipo).ToList(),

                Movimientos = mp.Movimientos
                    .OrderBy(m => m.Slot)
                    .Select(m => new MovimientoSlotDTO
                    {
                        Slot = m.Slot,
                        MovimientoId = m.MovimientoId,
                        Nombre = m.Movimiento.Nombre,
                        Tipo = m.Movimiento.Tipo,
                        Categoria = m.Movimiento.Categoria,
                        Poder = m.Movimiento.Poder
                    })
                    .ToList()
            };
        }
    }
}