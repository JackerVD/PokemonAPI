using PokemonAPI.Models;

namespace PokemonAPI.DTOs
{
    public class MovimientoSlotDTO
    {
        public int Slot { get; set; } // 1..4
        public int MovimientoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public Tipo Tipo { get; set; }
        public int Poder { get; set; }
        public CategoriaMovimiento Categoria { get; set; }
    }
}