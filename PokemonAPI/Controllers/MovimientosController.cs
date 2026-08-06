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
    public class MovimientosController : ControllerBase
    {
        private readonly PokemonDbContext _context;

        public MovimientosController(PokemonDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el listado de movimientos.
        /// </summary>
        /// <returns>200 con la colección de movimientos ordenada por nombre.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientos()
        {
            var dto = await _context.Movimientos
                .AsNoTracking()
                .OrderBy(m => m.Nombre)
                .Select(m => new MovimientoDTO
                {
                    Id = m.MovimientoId,
                    Nombre = m.Nombre,
                    Tipo = m.Tipo,
                    Poder = m.Poder,
                    Categoria = m.Categoria
                })
                .ToListAsync();

            return Ok(dto);
        }

        /// <summary>
        /// Obtiene un movimiento por su identificador.
        /// </summary>
        /// <param name="id">Identificador del movimiento.</param>
        /// <returns>200 con el movimiento si existe; 404 si no se encuentra.</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MovimientoDTO>> GetMovimiento(int id)
        {
            var dto = await _context.Movimientos
                .AsNoTracking()
                .Where(m => m.MovimientoId == id)
                .Select(m => new MovimientoDTO
                {
                    Id = m.MovimientoId,
                    Nombre = m.Nombre,
                    Tipo = m.Tipo,
                    Poder = m.Poder,
                    Categoria = m.Categoria
                })
                .FirstOrDefaultAsync();

            if (dto is null)
                return NotFound();

            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo movimiento.
        /// </summary>
        /// <param name="dto">Datos del movimiento a crear.</param>
        /// <returns>
        /// 201 con el recurso creado; 400 si la petición es inválida; 409 si el nombre ya existe o hay conflicto de datos.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<MovimientoDTO>> CreateMovimiento([FromBody] CreateMovimientoDto dto)
        {
            var nombre = dto.Nombre.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El nombre del movimiento es obligatorio.");

            var errorCategoria = ValidarPoderSegunCategoria(dto.Categoria, dto.Poder);
            if (errorCategoria is not null)
                return BadRequest(errorCategoria);

            var existeNombre = await _context.Movimientos.AnyAsync(m => m.Nombre == nombre);
            if (existeNombre)
                return Conflict("Ya existe un movimiento con ese nombre.");

            var movimiento = new Movimiento
            {
                Nombre = nombre,
                Tipo = dto.Tipo,
                Poder = dto.Poder,
                Categoria = dto.Categoria
            };

            _context.Movimientos.Add(movimiento);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Ya existe un movimiento con ese nombre o hay una restriccion de datos.");
            }

            var response = new MovimientoDTO
            {
                Id = movimiento.MovimientoId,
                Nombre = movimiento.Nombre,
                Tipo = movimiento.Tipo,
                Poder = movimiento.Poder,
                Categoria = movimiento.Categoria
            };

            return CreatedAtAction(nameof(GetMovimiento), new { id = movimiento.MovimientoId }, response);
        }

        /// <summary>
        /// Actualiza un movimiento existente.
        /// </summary>
        /// <param name="id">Identificador del movimiento.</param>
        /// <param name="dto">Datos actualizados del movimiento.</param>
        /// <returns>
        /// 204 si se actualiza correctamente; 404 si no existe; 400 si los datos son inválidos; 409 si hay conflicto de nombre.
        /// </returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMovimiento(int id, [FromBody] UpdateMovimientoDto dto)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento is null)
                return NotFound();

            var errorCategoria = ValidarPoderSegunCategoria(dto.Categoria, dto.Poder);
            if (errorCategoria is not null)
                return BadRequest(errorCategoria);

            var nombre = dto.Nombre.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("El nombre del movimiento es obligatorio.");

            // Excluye el propio movimiento para evitar falso conflicto
            var existeNombre = await _context.Movimientos
                .AnyAsync(m => m.Nombre == nombre && m.MovimientoId != id);

            if (existeNombre)
                return Conflict("Ya existe un movimiento con ese nombre.");

            movimiento.Nombre = nombre;
            movimiento.Tipo = dto.Tipo;
            movimiento.Poder = dto.Poder;
            movimiento.Categoria = dto.Categoria;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Ya existe un movimiento con ese nombre o hay una restriccion de datos.");
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina un movimiento.
        /// </summary>
        /// <param name="id">Identificador del movimiento.</param>
        /// <returns>204 si se elimina; 404 si no existe; 409 si tiene referencias relacionadas.</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMovimiento(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento is null)
                return NotFound();

            _context.Movimientos.Remove(movimiento);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("No se puede eliminar el movimiento porque tiene datos relacionados.");
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene los pokemons que tienen este movimiento en su lista de movimientos reales.
        /// </summary>
        /// <param name="id">Identificador del movimiento.</param>
        /// <returns>200 con la lista de pokemons; 404 si el movimiento no existe.</returns>
        [HttpGet("{id:int}/pokemons")]
        public async Task<ActionResult<IEnumerable<PokemonDTO>>> GetPokemonsPorMovimiento(int id)
        {
            var exists = await _context.Movimientos.AnyAsync(m => m.MovimientoId == id);
            if (!exists)
                return NotFound();

            var pokemons = await _context.Pokemons
                .AsNoTracking()
                .Include(p => p.Tipos)
                .Where(p => p.Movimientos.Any(pm => pm.MovimientoId == id))
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(pokemons.Select(ToPokemonDtoSinMovimientos).ToList());
        }

        /// <summary>
        /// Obtiene los pokemons que pueden aprender este movimiento.
        /// </summary>
        /// <param name="id">Identificador del movimiento.</param>
        /// <returns>200 con la lista de pokemons; 404 si el movimiento no existe.</returns>
        [HttpGet("{id:int}/pokemons-posibles")]
        public async Task<ActionResult<IEnumerable<PokemonDTO>>> GetPokemonsPorMovimientoPosible(int id)
        {
            var exists = await _context.Movimientos.AnyAsync(m => m.MovimientoId == id);
            if (!exists)
                return NotFound();

            var pokemons = await _context.Pokemons
                .AsNoTracking()
                .Include(p => p.Tipos)
                .Where(p => p.MovimientosPosibles.Any(pm => pm.MovimientoId == id))
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(pokemons.Select(ToPokemonDtoSinMovimientos).ToList());
        }

        /// <summary>
        /// Valida que el poder del movimiento sea consistente con su categoría.
        /// </summary>
        /// <param name="categoria">Categoría del movimiento.</param>
        /// <param name="poder">Poder del movimiento.</param>
        /// <returns>Mensaje de error si la validación falla, null si es válida.</returns>
        private static string? ValidarPoderSegunCategoria(CategoriaMovimiento categoria, int poder)
        {
            if (categoria == CategoriaMovimiento.Estado && poder != 0)
                return "Si la categoria es Estado, el poder debe ser 0.";

            if ((categoria == CategoriaMovimiento.Fisico || categoria == CategoriaMovimiento.Especial) && poder <= 0)
                return "Si la categoria es Fisico o Especial, el poder debe ser mayor que 0.";

            return null;
        }

        private static PokemonDTO ToPokemonDtoSinMovimientos(Pokemon p)
        {
            return new PokemonDTO
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
        }
    }
}