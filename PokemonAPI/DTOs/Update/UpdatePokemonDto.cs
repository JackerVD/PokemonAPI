using System.ComponentModel.DataAnnotations;
using PokemonAPI.Models;

namespace PokemonAPI.DTOs.Update
{
    public class UpdatePokemonDto
    {
        [Required]
        [StringLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 999)]
        public int SaludTotalBase { get; set; }

        [Range(1, 999)]
        public int AtaqueBase { get; set; }

        [Range(1, 999)]
        public int DefensaBase { get; set; }

        [Range(1, 999)]
        public int AtaqueEspecialBase { get; set; }

        [Range(1, 999)]
        public int DefensaEspecialBase { get; set; }

        [Range(1, 999)]
        public int VelocidadBase { get; set; }

        [MinLength(1)]
        [MaxLength(2)]
        public List<Tipo> Tipos { get; set; } = [];
    }
}