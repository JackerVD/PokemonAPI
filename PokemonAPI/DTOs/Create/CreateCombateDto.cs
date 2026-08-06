using System.ComponentModel.DataAnnotations;

namespace PokemonAPI.DTOs.Create
{
    public class CreateCombateDto
    {
        [Required]
        [MinLength(1)]
        public List<int> MiPokemonIds { get; set; } = [];

        [Range(1, 6)]
        public int CantidadEnemigos { get; set; } = 2;
    }
}
