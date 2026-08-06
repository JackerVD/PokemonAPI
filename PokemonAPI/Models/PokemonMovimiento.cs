namespace PokemonAPI.Models
{
    public class PokemonMovimiento
    {
        public int PokemonId { get; set; }
        public Pokemon Pokemon { get; set; } = null!;
        public int MovimientoId { get; set; }
        public Movimiento Movimiento { get; set; } = null!;
    }
}
