namespace PokemonAPI.Models
{
    public class MiPokemon
    {
        public int MiPokemonId { get; set; }
        public int PokemonId { get; set; }
        public string? NombrePersonalizado { get; set; }
        public int Nivel { get; set; }
        public int SaludActual { get; set; }
        public Pokemon PokemonBase { get; set; } = null!;
        public ICollection<MiPokemonMovimiento> Movimientos { get; set; } = [];
    }
}
