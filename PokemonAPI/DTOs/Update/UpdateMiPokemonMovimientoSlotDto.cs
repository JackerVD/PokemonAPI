using System.ComponentModel.DataAnnotations;

namespace PokemonAPI.DTOs.Update
{
    public class UpdateMiPokemonMovimientoSlotDto
    {
        [Required]
        public int MovimientoId { get; set; }
    }
}