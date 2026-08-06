namespace PokemonAPI.DTOs
{
    public class EstadoCombateDto
    {
        public Guid CombateId { get; set; }
        public string Estado { get; set; } = "EnCurso"; // EnCurso | Finalizado
        public int Turno { get; set; }
        public string? Ganador { get; set; } // Jugador | Enemigo

        public CombatienteActivoDto ActivoJugador { get; set; } = new();
        public CombatienteActivoDto ActivoEnemigo { get; set; } = new();

        public List<CombatienteResumenDto> EquipoJugador { get; set; } = [];
        public List<CombatienteResumenDto> EquipoEnemigo { get; set; } = [];

        public List<string> UltimosEventos { get; set; } = [];
    }

    public class CombatienteActivoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int SaludActual { get; set; }
        public int SaludMaxima { get; set; }
        public List<string> Movimientos { get; set; } = [];
    }

    public class CombatienteResumenDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int SaludActual { get; set; }
        public int SaludMaxima { get; set; }
        public bool Debilitado { get; set; }
    }
}
