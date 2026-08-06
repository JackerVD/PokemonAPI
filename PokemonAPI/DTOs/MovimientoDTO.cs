using PokemonAPI.Models;

namespace PokemonAPI.DTOs
{
    public class MovimientoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public Tipo Tipo { get; set; }
        public CategoriaMovimiento Categoria { get; set; }
        public int Poder { get; set; }
    }
}
