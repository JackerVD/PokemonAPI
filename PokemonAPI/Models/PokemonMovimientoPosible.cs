namespace PokemonAPI.Models
{
    public class PokemonMovimientoPosible
    {
        public int PokemonId { get; set; }
        public Pokemon Pokemon { get; set; } = null!;
        public int MovimientoId { get; set; }
        public Movimiento Movimiento { get; set; } = null!;
    }
}
