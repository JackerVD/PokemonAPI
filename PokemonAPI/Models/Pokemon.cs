namespace PokemonAPI.Models
{
    public class Pokemon
    {
        public int PokemonId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int SaludTotalBase { get; set; }
        public int AtaqueBase { get; set; }
        public int DefensaBase { get; set; }
        public int AtaqueEspecialBase { get; set; }
        public int DefensaEspecialBase { get; set; }
        public int VelocidadBase { get; set; }
        public ICollection<PokemonTipo> Tipos { get; set; } = [];
        public ICollection<PokemonMovimientoPosible> MovimientosPosibles { get; set; } = [];
        public ICollection<PokemonMovimiento> Movimientos { get; set; } = [];

    }
}
