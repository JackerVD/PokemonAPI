namespace PokemonAPI.Models
{
    public class Movimiento
    {
        public int MovimientoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public Tipo Tipo { get; set; }
        public int Poder { get; set; }
        public CategoriaMovimiento Categoria { get; set; } = CategoriaMovimiento.Fisico;
        public ICollection<PokemonMovimientoPosible> PokemonsQuePuedenAprenderlo { get; set; } = [];
        public ICollection<MiPokemonMovimiento> MisPokemonsQueLoTienen { get; set; } = [];
        public ICollection<PokemonMovimiento> PokemonsQueLoTienen { get; set; } = [];

    }
}
