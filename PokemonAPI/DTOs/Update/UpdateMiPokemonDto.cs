using System.ComponentModel.DataAnnotations;

namespace PokemonAPI.DTOs.Update
{
    public class UpdateMiPokemonDto
    {
        [StringLength(80)]
        public string? NombrePersonalizado { get; set; }

        [Range(1, 100)]
        public int? Nivel { get; set; }

        [Range(0, 9999)]
        public int? SaludActual { get; set; }
    }
}
