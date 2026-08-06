using System.ComponentModel.DataAnnotations;

namespace PokemonAPI.DTOs.Update
{
    public class UpdateMiPokemonMovimientosLoteDto
    {
        [Required]
        [MinLength(1)]
        public List<UpdateMiPokemonMovimientoLoteItemDto> Movimientos { get; set; } = [];
    }

    public class UpdateMiPokemonMovimientoLoteItemDto
    {
        [Range(1, 4)]
        public int Slot { get; set; }

        [Required]
        public int MovimientoId { get; set; }
    }
}