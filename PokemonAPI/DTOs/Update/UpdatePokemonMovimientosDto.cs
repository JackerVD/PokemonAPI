using System.ComponentModel.DataAnnotations;

namespace PokemonAPI.DTOs.Update
{
    public class UpdatePokemonMovimientosDto
    {
        [Required]
        [MinLength(1)]
        public List<int> MovimientoIds { get; set; } = [];
    }
}
