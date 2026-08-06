namespace PokemonAPI.Models
{
    public class MiPokemonMovimiento
    {
        public int MiPokemonId { get; set; }
        public MiPokemon MiPokemon { get; set; } = null!;
        public int MovimientoId { get; set; }
        public Movimiento Movimiento { get; set; } = null!;
        public int Slot { get; set; } // 1, 2, 3 o 4
    }
}
