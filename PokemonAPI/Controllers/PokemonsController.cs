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
    public class PokemonsController : ControllerBase
    {
        private readonly PokemonDbContext _context;

        public PokemonsController(PokemonDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el listado de pokemons base.
        /// </summary>
        /// <param name="includeMovimientos">Incluye los movimientos reales del pokemon.</param>
        /// <param name="includeMovimientosPosibles">Incluye los movimientos posibles del pokemon.</param>
        /// <returns>200 con la colección de pokemons.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PokemonDTO>>> GetPokemons(
            [FromQuery] bool includeMovimientos = false,
            [FromQuery] bool includeMovimientosPosibles = false)
        {
            IQueryable<Pokemon> query = _context.Pokemons
                .AsNoTracking()
                .Include(p => p.Tipos);

            if (includeMovimientos)
            {
                query = query
                    .Include(p => p.Movimientos)
                    .ThenInclude(pm => pm.Movimiento);
            }

            if (includeMovimientosPosibles)
            {
                query = query
                    .Include(p => p.MovimientosPosibles)
                    .ThenInclude(pm => pm.Movimiento);
            }

            var pokemons = await query.ToListAsync();
            var dto = pokemons
                .Select(p => ToPokemonDto(p, includeMovimientos, includeMovimientosPosibles))
                .ToList();

            return Ok(dto);
        }

        /// <summary>
        /// Obtiene un pokemon base por su identificador.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <param name="includeMovimientos">Incluye los movimientos reales del pokemon.</param>
        /// <param name="includeMovimientosPosibles">Incluye los movimientos posibles del pokemon.</param>
        /// <returns>200 con el pokemon; 404 si no existe.</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PokemonDTO>> GetPokemon(
            int id,
            [FromQuery] bool includeMovimientos = true,
            [FromQuery] bool includeMovimientosPosibles = true)
        {
            IQueryable<Pokemon> query = _context.Pokemons
                .AsNoTracking()
                .Include(p => p.Tipos);

            if (includeMovimientos)
            {
                query = query
                    .Include(p => p.Movimientos)
                    .ThenInclude(pm => pm.Movimiento);
            }

            if (includeMovimientosPosibles)
            {
                query = query
                    .Include(p => p.MovimientosPosibles)
                    .ThenInclude(pm => pm.Movimiento);
            }

            var pokemon = await query.FirstOrDefaultAsync(p => p.PokemonId == id);

            if (pokemon is null)
                return NotFound();

            return Ok(ToPokemonDto(pokemon, includeMovimientos, includeMovimientosPosibles));
        }

        /// <summary>
        /// Crea un nuevo pokemon base.
        /// </summary>
        /// <param name="dto">Datos del pokemon a crear.</param>
        /// <returns>201 con el recurso creado; 400 si los datos son inválidos; 409 si hay conflicto de nombre.</returns>
        [HttpPost]
        public async Task<ActionResult<PokemonDTO>> CreatePokemon([FromBody] CreatePokemonDto dto)
        {
            var nombre = dto.Nombre.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El nombre del pokemon es obligatorio.");

            var tiposUnicos = NormalizarYValidarTipos(dto.Tipos);
            if (tiposUnicos is null)
                return BadRequest("Un Pokemon debe tener 1 o 2 tipos, sin repetir.");

            var existeNombre = await _context.Pokemons.AnyAsync(p => p.Nombre == nombre);
            if (existeNombre)
                return Conflict("Ya existe un pokemon con ese nombre.");

            var pokemon = new Pokemon
            {
                Nombre = nombre,
                SaludTotalBase = dto.SaludTotalBase,
                AtaqueBase = dto.AtaqueBase,
                DefensaBase = dto.DefensaBase,
                AtaqueEspecialBase = dto.AtaqueEspecialBase,
                DefensaEspecialBase = dto.DefensaEspecialBase,
                VelocidadBase = dto.VelocidadBase,
                Tipos = tiposUnicos.Select(t => new PokemonTipo { Tipo = t }).ToList()
            };

            _context.Pokemons.Add(pokemon);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Ya existe un pokemon con ese nombre o hay una restriccion de datos.");
            }

            var created = await _context.Pokemons
                .AsNoTracking()
                .Include(p => p.Tipos)
                .FirstAsync(p => p.PokemonId == pokemon.PokemonId);

            var response = ToPokemonDto(created, includeMovimientos: false, includeMovimientosPosibles: false);

            return CreatedAtAction(nameof(GetPokemon), new { id = created.PokemonId }, response);
        }

        /// <summary>
        /// Actualiza un pokemon base existente.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <param name="dto">Datos actualizados del pokemon.</param>
        /// <returns>204 si se actualiza; 404 si no existe; 400 si es inválido; 409 si hay conflicto de nombre.</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePokemon(int id, [FromBody] UpdatePokemonDto dto)
        {
            var nombre = dto.Nombre.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El nombre del pokemon es obligatorio.");

            var tiposUnicos = NormalizarYValidarTipos(dto.Tipos);
            if (tiposUnicos is null)
                return BadRequest("Un Pokemon debe tener 1 o 2 tipos, sin repetir.");

            var pokemon = await _context.Pokemons
                .Include(p => p.Tipos)
                .FirstOrDefaultAsync(p => p.PokemonId == id);

            if (pokemon is null)
                return NotFound();

            var existeNombre = await _context.Pokemons
                .AnyAsync(p => p.Nombre == nombre && p.PokemonId != id);

            if (existeNombre)
                return Conflict("Ya existe un pokemon con ese nombre.");

            pokemon.Nombre = nombre;
            pokemon.SaludTotalBase = dto.SaludTotalBase;
            pokemon.AtaqueBase = dto.AtaqueBase;
            pokemon.DefensaBase = dto.DefensaBase;
            pokemon.AtaqueEspecialBase = dto.AtaqueEspecialBase;
            pokemon.DefensaEspecialBase = dto.DefensaEspecialBase;
            pokemon.VelocidadBase = dto.VelocidadBase;

            _context.PokemonTipos.RemoveRange(pokemon.Tipos);
            pokemon.Tipos = tiposUnicos.Select(t => new PokemonTipo
            {
                PokemonId = pokemon.PokemonId,
                Tipo = t
            }).ToList();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Ya existe un pokemon con ese nombre o hay una restriccion de datos.");
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina un pokemon base.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <returns>204 si se elimina; 404 si no existe; 409 si tiene datos relacionados.</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePokemon(int id)
        {
            var pokemon = await _context.Pokemons.FindAsync(id);
            if (pokemon is null)
                return NotFound();

            _context.Pokemons.Remove(pokemon);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("No se puede eliminar el Pokemon porque tiene datos relacionados.");
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene los movimientos reales de un pokemon base.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <returns>200 con la lista de movimientos; 404 si el pokemon no existe.</returns>
        [HttpGet("{id:int}/movimientos")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientos(int id)
        {
            var exists = await _context.Pokemons.AnyAsync(p => p.PokemonId == id);
            if (!exists)
                return NotFound();

            var movimientos = await _context.PokemonMovimientos
                .AsNoTracking()
                .Where(pm => pm.PokemonId == id)
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
        /// Reemplaza la lista completa de movimientos reales de un pokemon.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <param name="dto">Lista completa de movimientos a establecer.</param>
        /// <returns>204 si se actualiza; 404 si no existe; 400 si los datos son inválidos.</returns>
        [HttpPut("{id:int}/movimientos")]
        public async Task<IActionResult> UpdatePokemonMovimientos(int id, [FromBody] UpdatePokemonMovimientosDto dto)
        {
            var pokemon = await _context.Pokemons
                .Include(p => p.Movimientos)
                .FirstOrDefaultAsync(p => p.PokemonId == id);

            if (pokemon is null)
                return NotFound();

            var idsUnicos = dto.MovimientoIds.Distinct().ToList();
            if (idsUnicos.Count != dto.MovimientoIds.Count)
                return BadRequest("No se pueden repetir movimientos.");

            var movimientosExistentes = await _context.Movimientos
                .Where(m => idsUnicos.Contains(m.MovimientoId))
                .Select(m => m.MovimientoId)
                .ToListAsync();

            if (movimientosExistentes.Count != idsUnicos.Count)
                return BadRequest("Uno o mas movimientos no existen.");

            _context.PokemonMovimientos.RemoveRange(pokemon.Movimientos);

            pokemon.Movimientos = idsUnicos.Select(idMovimiento => new PokemonMovimiento
            {
                PokemonId = id,
                MovimientoId = idMovimiento
            }).ToList();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Actualiza parcialmente los movimientos reales de un pokemon.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <param name="dto">Movimientos a agregar y/o eliminar.</param>
        /// <returns>204 si se actualiza; 404 si no existe; 400 si la petición es inválida.</returns>
        [HttpPatch("{id:int}/movimientos")]
        public async Task<IActionResult> PatchMovimientos(int id, [FromBody] UpdateMovimientosPokemonDto dto)
        {
            if ((dto.Agregar?.Any() != true) && (dto.Eliminar?.Any() != true))
                return BadRequest("Debes enviar al menos un movimiento para agregar o eliminar.");

            var pokemon = await _context.Pokemons
                .Include(p => p.Movimientos)
                .FirstOrDefaultAsync(p => p.PokemonId == id);

            if (pokemon is null)
                return NotFound();

            if (dto.Eliminar?.Any() == true)
            {
                var idsEliminar = dto.Eliminar.Distinct().ToList();
                var aEliminar = await _context.PokemonMovimientos
                    .Where(pm => pm.PokemonId == id && idsEliminar.Contains(pm.MovimientoId))
                    .ToListAsync();

                _context.PokemonMovimientos.RemoveRange(aEliminar);
            }

            if (dto.Agregar?.Any() == true)
            {
                var idsAgregar = dto.Agregar.Distinct().ToList();

                var idsValidos = await _context.Movimientos
                    .Where(m => idsAgregar.Contains(m.MovimientoId))
                    .Select(m => m.MovimientoId)
                    .ToListAsync();

                var existentes = pokemon.Movimientos
                    .Select(pm => pm.MovimientoId)
                    .ToHashSet();

                var nuevos = idsValidos
                    .Where(idMov => !existentes.Contains(idMov))
                    .ToList();

                foreach (var idMov in nuevos)
                {
                    pokemon.Movimientos.Add(new PokemonMovimiento
                    {
                        PokemonId = id,
                        MovimientoId = idMov
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Obtiene los movimientos posibles de un pokemon base.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <returns>200 con la lista de movimientos posibles; 404 si el pokemon no existe.</returns>
        [HttpGet("{id:int}/movimientos-posibles")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientosPosibles(int id)
        {
            var exists = await _context.Pokemons.AnyAsync(p => p.PokemonId == id);
            if (!exists)
                return NotFound();

            var movimientos = await _context.PokemonMovimientosPosibles
                .AsNoTracking()
                .Where(pm => pm.PokemonId == id)
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
        /// Reemplaza la lista completa de movimientos posibles de un pokemon.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <param name="dto">Lista completa de movimientos posibles a establecer.</param>
        /// <returns>204 si se actualiza; 404 si no existe; 400 si los datos son inválidos.</returns>
        [HttpPut("{id:int}/movimientos-posibles")]
        public async Task<IActionResult> UpdateMovimientosPosibles(int id, [FromBody] UpdatePokemonMovimientosDto dto)
        {
            var pokemon = await _context.Pokemons
                .Include(p => p.MovimientosPosibles)
                .FirstOrDefaultAsync(p => p.PokemonId == id);

            if (pokemon is null)
                return NotFound();

            var idsUnicos = dto.MovimientoIds.Distinct().ToList();
            if (idsUnicos.Count != dto.MovimientoIds.Count)
                return BadRequest("No se pueden repetir movimientos.");

            var existentes = await _context.Movimientos
                .Where(m => idsUnicos.Contains(m.MovimientoId))
                .Select(m => m.MovimientoId)
                .ToListAsync();

            if (existentes.Count != idsUnicos.Count)
                return BadRequest("Uno o mas movimientos no existen.");

            _context.PokemonMovimientosPosibles.RemoveRange(pokemon.MovimientosPosibles);

            pokemon.MovimientosPosibles = idsUnicos.Select(movId => new PokemonMovimientoPosible
            {
                PokemonId = id,
                MovimientoId = movId
            }).ToList();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Actualiza parcialmente los movimientos posibles de un pokemon.
        /// </summary>
        /// <param name="id">Identificador del pokemon.</param>
        /// <param name="dto">Movimientos posibles a agregar y/o eliminar.</param>
        /// <returns>204 si se actualiza; 404 si no existe; 400 si la petición es inválida.</returns>
        [HttpPatch("{id:int}/movimientos-posibles")]
        public async Task<IActionResult> PatchMovimientosPosibles(int id, [FromBody] UpdateMovimientosPokemonDto dto)
        {
            if ((dto.Agregar?.Any() != true) && (dto.Eliminar?.Any() != true))
                return BadRequest("Debes enviar al menos un movimiento para agregar o eliminar.");

            var pokemon = await _context.Pokemons
                .Include(p => p.MovimientosPosibles)
                .FirstOrDefaultAsync(p => p.PokemonId == id);

            if (pokemon is null)
                return NotFound();

            if (dto.Eliminar?.Any() == true)
            {
                var idsEliminar = dto.Eliminar.Distinct().ToList();
                var aEliminar = await _context.PokemonMovimientosPosibles
                    .Where(pm => pm.PokemonId == id && idsEliminar.Contains(pm.MovimientoId))
                    .ToListAsync();

                _context.PokemonMovimientosPosibles.RemoveRange(aEliminar);
            }

            if (dto.Agregar?.Any() == true)
            {
                var idsAgregar = dto.Agregar.Distinct().ToList();

                var idsValidos = await _context.Movimientos
                    .Where(m => idsAgregar.Contains(m.MovimientoId))
                    .Select(m => m.MovimientoId)
                    .ToListAsync();

                var existentes = pokemon.MovimientosPosibles
                    .Select(pm => pm.MovimientoId)
                    .ToHashSet();

                var nuevos = idsValidos
                    .Where(idMov => !existentes.Contains(idMov))
                    .ToList();

                foreach (var idMov in nuevos)
                {
                    pokemon.MovimientosPosibles.Add(new PokemonMovimientoPosible
                    {
                        PokemonId = id,
                        MovimientoId = idMov
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        /// <summary>
        /// Normaliza y valida la lista de tipos de un pokemon. Elimina duplicados y verifica que haya 1 o 2 tipos.
        /// </summary>
        /// <param name="tipos">Lista de tipos a normalizar y validar.</param>
        /// <returns>Lista de tipos normalizados y validados, o null si la lista es inválida.</returns>
        private static List<Tipo>? NormalizarYValidarTipos(List<Tipo> tipos)
        {
            if (tipos is null)
                return null;

            var unicos = tipos.Distinct().ToList();
            if (unicos.Count < 1 || unicos.Count > 2)
                return null;

            return unicos;
        }

        private static PokemonDTO ToPokemonDto(Pokemon p, bool includeMovimientos, bool includeMovimientosPosibles)
        {
            var dto = new PokemonDTO
            {
                Id = p.PokemonId,
                Nombre = p.Nombre,
                SaludTotalBase = p.SaludTotalBase,
                AtaqueBase = p.AtaqueBase,
                DefensaBase = p.DefensaBase,
                AtaqueEspecialBase = p.AtaqueEspecialBase,
                DefensaEspecialBase = p.DefensaEspecialBase,
                VelocidadBase = p.VelocidadBase,
                Tipos = [.. p.Tipos.Select(t => t.Tipo)],
                Movimientos = [],
                MovimientosPosibles = []
            };

            if (includeMovimientos)
            {
                dto.Movimientos = p.Movimientos
                    .Select(pm => new MovimientoDTO
                    {
                        Id = pm.MovimientoId,
                        Nombre = pm.Movimiento.Nombre,
                        Tipo = pm.Movimiento.Tipo,
                        Categoria = pm.Movimiento.Categoria,
                        Poder = pm.Movimiento.Poder
                    })
                    .OrderBy(m => m.Nombre)
                    .ToList();
            }

            if (includeMovimientosPosibles)
            {
                dto.MovimientosPosibles = p.MovimientosPosibles
                    .Select(pm => new MovimientoDTO
                    {
                        Id = pm.MovimientoId,
                        Nombre = pm.Movimiento.Nombre,
                        Tipo = pm.Movimiento.Tipo,
                        Categoria = pm.Movimiento.Categoria,
                        Poder = pm.Movimiento.Poder
                    })
                    .OrderBy(m => m.Nombre)
                    .ToList();
            }

            return dto;
        }
    }
}