using PokemonAPI.Models;

namespace PokemonAPI.DTOs
{
    public class MiPokemonDTO
    {
        public int MiPokemonId { get; set; }
        public int PokemonBaseId { get; set; }
        public string PokemonBaseNombre { get; set; } = string.Empty;

        public string NombreMostrado { get; set; } = string.Empty;
        public string? NombrePersonalizado { get; set; }

        public int Nivel { get; set; }
        public int SaludActual { get; set; }

        public int SaludTotalBase { get; set; }
        public int AtaqueBase { get; set; }
        public int DefensaBase { get; set; }
        public int AtaqueEspecialBase { get; set; }
        public int DefensaEspecialBase { get; set; }
        public int VelocidadBase { get; set; }

        public List<Tipo> Tipos { get; set; } = [];
        public List<MovimientoSlotDTO> Movimientos { get; set; } = [];
    }

}
