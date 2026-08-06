namespace PokemonAPI.Models
{
    public class PokemonTipo
    {
        public int PokemonId { get; set; }
        public Pokemon Pokemon { get; set; } = null!;
        public Tipo Tipo { get; set; }
    }
}
