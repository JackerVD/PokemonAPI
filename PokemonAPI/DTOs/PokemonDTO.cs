using PokemonAPI.Models;

namespace PokemonAPI.DTOs
{
    public class PokemonDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Nivel { get; set; } = 50;

        public int SaludTotalBase { get; set; }
        public int AtaqueBase { get; set; }
        public int DefensaBase { get; set; }
        public int AtaqueEspecialBase { get; set; }
        public int DefensaEspecialBase { get; set; }
        public int VelocidadBase { get; set; }

        public List<Tipo> Tipos { get; set; } = [];
        public List<MovimientoDTO> Movimientos { get; set; } = [];
        public List<MovimientoDTO> MovimientosPosibles { get; set; } = [];
    }
}