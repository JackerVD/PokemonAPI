using PokemonAPI.DTOs;
using PokemonAPI.Logic;
using PokemonAPI.Models;

namespace PokemonAPI.Services
{
    public class DanoService
    {
        private readonly Random _random = new();

        /// <summary>
        /// Calcula el daño de un movimiento según categoría (físico/especial), efectividad de tipos y factor aleatorio.
        /// </summary>
        /// <param name="atacante">El Pokémon que ataca</param>
        /// <param name="defensor">El Pokémon que defiende</param>
        /// <param name="movimiento">El movimiento utilizado</param>
        /// <returns>El daño causado por el movimiento</returns>
        public int CalcularDano(PokemonDTO atacante, PokemonDTO defensor, MovimientoDTO movimiento)
        {
            // Los movimientos de estado no hacen daño directo.
            if (movimiento.Categoria == CategoriaMovimiento.Estado || movimiento.Poder <= 0)
                return 0;

            var efectividad = TablaTipo.GetEfectividad(movimiento.Tipo, defensor.Tipos);
            if (efectividad == 0)
                return 0;

            int statAtaque = movimiento.Categoria == CategoriaMovimiento.Especial
                ? atacante.AtaqueEspecialBase
                : atacante.AtaqueBase;

            int statDefensa = movimiento.Categoria == CategoriaMovimiento.Especial
                ? defensor.DefensaEspecialBase
                : defensor.DefensaBase;

            double randomFactor = _random.Next(85, 101) / 100.0;

            double danoBase =
                (((2 * atacante.Nivel / 5.0 + 2) * movimiento.Poder * ((double)statAtaque / Math.Max(1, statDefensa))) / 50.0) + 2;

            double danoFinal = danoBase * efectividad * randomFactor;

            return Math.Max(1, (int)Math.Floor(danoFinal));
        }
    }
}