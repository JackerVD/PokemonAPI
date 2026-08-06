using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonAPI.Data;
using PokemonAPI.DTOs;
using PokemonAPI.DTOs.Update;
using PokemonAPI.Models;

namespace PokemonAPI.Controllers
{
    [ApiController]
    [Route("api/mispokemons/{miPokemonId:int}/movimientos")]
    public class MiPokemonMovimientosController : ControllerBase
    {
        private readonly PokemonDbContext _context;

        public MiPokemonMovimientosController(PokemonDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene los movimientos asignados (slots) de un MiPokemon.
        /// </summary>
        /// <param name="miPokemonId">Identificador del MiPokemon.</param>
        /// <returns>200 con los movimientos ordenados por slot; 404 si el MiPokemon no existe.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoSlotDTO>>> GetMovimientos(int miPokemonId)
        {
            var existe = await _context.MisPokemons.AnyAsync(mp => mp.MiPokemonId == miPokemonId);
            if (!existe)
                return NotFound("MiPokemon no existe.");

            var movimientos = await _context.MiPokemonMovimientos
                .AsNoTracking()
                .Where(mm => mm.MiPokemonId == miPokemonId)
                .OrderBy(mm => mm.Slot)
                .Select(mm => new MovimientoSlotDTO
                {
                    Slot = mm.Slot,
                    MovimientoId = mm.MovimientoId,
                    Nombre = mm.Movimiento.Nombre,
                    Tipo = mm.Movimiento.Tipo,
                    Poder = mm.Movimiento.Poder,
                    Categoria = mm.Movimiento.Categoria
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        /// <summary>
        /// Asigna o reemplaza un movimiento en un slot (1..4) de un MiPokemon.
        /// </summary>
        /// <param name="miPokemonId">Identificador del MiPokemon.</param>
        /// <param name="slot">Slot del movimiento (1 a 4).</param>
        /// <param name="dto">Movimiento a asignar.</param>
        /// <returns>
        /// 204 si se actualiza correctamente; 404 si no existe el MiPokemon; 400 si la petición es inválida; 409 en conflicto de datos.
        /// </returns>
        [HttpPut("{slot:int}")]
        public async Task<IActionResult> UpsertMovimientoEnSlot(
            int miPokemonId,
            int slot,
            [FromBody] UpdateMiPokemonMovimientoSlotDto dto)
        {
            if (slot < 1 || slot > 4)
                return BadRequest("El slot debe estar entre 1 y 4.");

            var miPokemon = await _context.MisPokemons
                .FirstOrDefaultAsync(mp => mp.MiPokemonId == miPokemonId);

            if (miPokemon is null)
                return NotFound("MiPokemon no existe.");

            var movimientoId = dto.MovimientoId;

            var movimientoExiste = await _context.Movimientos
                .AnyAsync(m => m.MovimientoId == movimientoId);

            if (!movimientoExiste)
                return BadRequest("El movimiento no existe.");

            var esPosible = await _context.PokemonMovimientosPosibles
                .AnyAsync(pm => pm.PokemonId == miPokemon.PokemonId && pm.MovimientoId == movimientoId);

            if (!esPosible)
                return BadRequest("Ese movimiento no es posible para este Pokemon.");

            var repetido = await _context.MiPokemonMovimientos
                .AnyAsync(mm =>
                    mm.MiPokemonId == miPokemonId &&
                    mm.MovimientoId == movimientoId &&
                    mm.Slot != slot);

            if (repetido)
                return BadRequest("Ese movimiento ya está asignado en otro slot.");

            var actual = await _context.MiPokemonMovimientos
                .FirstOrDefaultAsync(mm => mm.MiPokemonId == miPokemonId && mm.Slot == slot);

            if (actual is null)
            {
                _context.MiPokemonMovimientos.Add(new MiPokemonMovimiento
                {
                    MiPokemonId = miPokemonId,
                    Slot = slot,
                    MovimientoId = movimientoId
                });
            }
            else
            {
                actual.MovimientoId = movimientoId;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("No se pudo actualizar el slot por un conflicto de datos.");
            }

            return NoContent();
        }

        /// <summary>
        /// Actualiza varios slots de movimientos en una sola petición.
        /// </summary>
        /// <param name="miPokemonId">Identificador del MiPokemon.</param>
        /// <param name="dto">Lista de cambios de slot y movimiento.</param>
        /// <returns>
        /// 204 si se actualiza correctamente; 404 si no existe el MiPokemon; 400 si la petición es inválida; 409 en conflicto de datos.
        /// </returns>
        [HttpPatch]
        public async Task<IActionResult> PatchMovimientosEnLote(
            int miPokemonId,
            [FromBody] UpdateMiPokemonMovimientosLoteDto dto)
        {
            if (dto.Movimientos is null || dto.Movimientos.Count == 0)
                return BadRequest("Debes enviar al menos un cambio.");

            var miPokemon = await _context.MisPokemons
                .FirstOrDefaultAsync(mp => mp.MiPokemonId == miPokemonId);

            if (miPokemon is null)
                return NotFound("MiPokemon no existe.");

            // Evita dos cambios sobre el mismo slot en la misma petición
            var slotsDuplicados = dto.Movimientos
                .GroupBy(x => x.Slot)
                .Any(g => g.Count() > 1);

            if (slotsDuplicados)
                return BadRequest("No puedes repetir el mismo slot en la misma petición.");

            var movimientoIds = dto.Movimientos.Select(x => x.MovimientoId).Distinct().ToList();

            var movimientosExistentes = await _context.Movimientos
                .Where(m => movimientoIds.Contains(m.MovimientoId))
                .Select(m => m.MovimientoId)
                .ToListAsync();

            if (movimientosExistentes.Count != movimientoIds.Count)
                return BadRequest("Uno o más movimientos no existen.");

            var posibles = await _context.PokemonMovimientosPosibles
                .Where(pm => pm.PokemonId == miPokemon.PokemonId && movimientoIds.Contains(pm.MovimientoId))
                .Select(pm => pm.MovimientoId)
                .Distinct()
                .ToListAsync();

            if (posibles.Count != movimientoIds.Count)
                return BadRequest("Uno o más movimientos no son posibles para este Pokemon.");

            var actuales = await _context.MiPokemonMovimientos
                .Where(mm => mm.MiPokemonId == miPokemonId)
                .ToListAsync();

            foreach (var item in dto.Movimientos)
            {
                var existente = actuales.FirstOrDefault(x => x.Slot == item.Slot);
                if (existente is null)
                {
                    _context.MiPokemonMovimientos.Add(new MiPokemonMovimiento
                    {
                        MiPokemonId = miPokemonId,
                        Slot = item.Slot,
                        MovimientoId = item.MovimientoId
                    });
                }
                else
                {
                    existente.MovimientoId = item.MovimientoId;
                }
            }

            // Valida que tras aplicar cambios no haya el mismo movimiento repetido en slots distintos
            var resultado = await _context.MiPokemonMovimientos
                .Where(mm => mm.MiPokemonId == miPokemonId)
                .Select(mm => new { mm.Slot, mm.MovimientoId })
                .ToListAsync();

            if (resultado.GroupBy(x => x.MovimientoId).Any(g => g.Count() > 1))
                return BadRequest("No puedes asignar el mismo movimiento en varios slots.");

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("No se pudo actualizar los movimientos por un conflicto de datos.");
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina el movimiento asignado en un slot de un MiPokemon.
        /// </summary>
        /// <param name="miPokemonId">Identificador del MiPokemon.</param>
        /// <param name="slot">Slot del movimiento (1 a 4).</param>
        /// <returns>204 si se elimina; 404 si no existe el MiPokemon o el slot.</returns>
        [HttpDelete("{slot:int}")]
        public async Task<IActionResult> DeleteMovimientoEnSlot(int miPokemonId, int slot)
        {
            if (slot < 1 || slot > 4)
                return BadRequest("El slot debe estar entre 1 y 4.");

            var existeMiPokemon = await _context.MisPokemons
                .AnyAsync(mp => mp.MiPokemonId == miPokemonId);

            if (!existeMiPokemon)
                return NotFound("MiPokemon no existe.");

            var actual = await _context.MiPokemonMovimientos
                .FirstOrDefaultAsync(mm => mm.MiPokemonId == miPokemonId && mm.Slot == slot);

            if (actual is null)
                return NotFound("No hay movimiento asignado en ese slot.");

            _context.MiPokemonMovimientos.Remove(actual);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}