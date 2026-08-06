using PokemonAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace PokemonAPI.DTOs.Create
{
    public class CreateMovimientoDto
    {
        [Required]
        [StringLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public Tipo Tipo { get; set; }

        [Required]
        public CategoriaMovimiento Categoria { get; set; }

        [Range(0, 999)]
        public int Poder { get; set; }
    }
}
