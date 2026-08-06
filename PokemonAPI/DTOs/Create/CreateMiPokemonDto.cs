using System.ComponentModel.DataAnnotations;

namespace PokemonAPI.DTOs.Create
{
    public class CreateMiPokemonDto
    {
        [Required]
        public int PokemonId { get; set; }

        [StringLength(80)]
        public string? NombrePersonalizado { get; set; }

        [Range(1, 100)]
        public int Nivel { get; set; } = 50;

        [Range(0, 9999)]
        public int? SaludActual { get; set; }
    }
}