using Microsoft.AspNetCore.Mvc;
using PokemonAPI.Data;
using PokemonAPI.DTOs;
using PokemonAPI.DTOs.Create;
using PokemonAPI.Services;

namespace PokemonAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CombatesController : ControllerBase
    {
        private readonly PokemonDbContext _db;
        private readonly CombateService _service;

        public CombatesController(PokemonDbContext db, CombateService service)
        {
            _db = db;
            _service = service;
        }

        /// <summary>
        /// Crea un nuevo combate en memoria.
        /// </summary>
        /// <param name="dto">Datos iniciales del combate (equipo del jugador y cantidad de enemigos).</param>
        /// <returns>
        /// 201 con el estado inicial del combate; 400 si la configuración inicial es inválida.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<EstadoCombateDto>> Crear([FromBody] CreateCombateDto dto)
        {
            try
            {
                var estado = await _service.CrearCombateAsync(_db, dto);
                return CreatedAtAction(nameof(Obtener), new { id = estado.CombateId }, estado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el estado actual de un combate.
        /// </summary>
        /// <param name="id">Identificador del combate.</param>
        /// <returns>200 con el estado del combate; 404 si no existe.</returns>
        [HttpGet("{id:guid}")]
        public ActionResult<EstadoCombateDto> Obtener(Guid id)
        {
            try
            {
                return Ok(_service.Obtener(id));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Obtiene el historial completo de eventos del combate.
        /// </summary>
        /// <param name="id">Identificador del combate.</param>
        /// <returns>200 con la lista de eventos; 404 si no existe.</returns>
        [HttpGet("{id:guid}/historial")]
        public ActionResult<IEnumerable<string>> ObtenerHistorial(Guid id)
        {
            try
            {
                return Ok(_service.ObtenerHistorial(id));
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        /// <summary>
        /// Ejecuta un turno del combate y devuelve el nuevo estado.
        /// </summary>
        /// <param name="id">Identificador del combate.</param>
        /// <returns>
        /// 200 con el estado actualizado; 404 si no existe; 409 si el combate ya finalizó.
        /// </returns>
        [HttpPost("{id:guid}/turno")]
        public async Task<ActionResult<EstadoCombateDto>> EjecutarTurno(Guid id)
        {
            try
            {
                var estado = await _service.EjecutarTurnoAsync(id);
                return Ok(estado);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Simula el combate completo hasta que finalice o se alcance un número máximo de turnos.
        /// </summary>
        /// <param name="id">Identificador del combate.</param>
        /// <param name="maxTurnos">Número máximo de turnos a simular.</param>
        /// <returns>200 con el estado tras la simulación; 404 si no existe; 400 si el límite es inválido.</returns>
        [HttpPost("{id:guid}/simular")]
        public async Task<ActionResult<EstadoCombateDto>> SimularCombateCompleto(Guid id, [FromQuery] int maxTurnos = 100)
        {
            if (maxTurnos < 1)
                return BadRequest("maxTurnos debe ser mayor o igual que 1.");

            try
            {
                var estado = await _service.SimularCombateCompletoAsync(id, maxTurnos);
                return Ok(estado);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un combate en memoria.
        /// </summary>
        /// <param name="id">Identificador del combate.</param>
        /// <returns>204 si se elimina; 404 si no existe.</returns>
        [HttpDelete("{id:guid}")]
        public IActionResult Eliminar(Guid id)
        {
            return _service.Eliminar(id) ? NoContent() : NotFound();
        }
    }
}