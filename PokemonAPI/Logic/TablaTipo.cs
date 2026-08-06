using PokemonAPI.Models;

namespace PokemonAPI.Logic
{
    public static class TablaTipo
    {
        public static readonly IReadOnlyDictionary<(Tipo Atacante, Tipo Defensor), double> Excepciones =
            new Dictionary<(Tipo Atacante, Tipo Defensor), double>
            {
                // ACERO
                { (Tipo.Acero, Tipo.Hada), 2 },
                { (Tipo.Acero, Tipo.Hielo), 2 },
                { (Tipo.Acero, Tipo.Roca), 2 },
                { (Tipo.Acero, Tipo.Acero), 0.5 },
                { (Tipo.Acero, Tipo.Agua), 0.5 },
                { (Tipo.Acero, Tipo.Electrico), 0.5 },
                { (Tipo.Acero, Tipo.Fuego), 0.5 },

                // AGUA
                { (Tipo.Agua, Tipo.Fuego), 2 },
                { (Tipo.Agua, Tipo.Roca), 2 },
                { (Tipo.Agua, Tipo.Tierra), 2 },
                { (Tipo.Agua, Tipo.Agua), 0.5 },
                { (Tipo.Agua, Tipo.Dragon), 0.5 },
                { (Tipo.Agua, Tipo.Planta), 0.5 },

                // BICHO
                { (Tipo.Bicho, Tipo.Planta), 2 },
                { (Tipo.Bicho, Tipo.Psiquico), 2 },
                { (Tipo.Bicho, Tipo.Siniestro), 2 },
                { (Tipo.Bicho, Tipo.Acero), 0.5 },
                { (Tipo.Bicho, Tipo.Fantasma), 0.5 },
                { (Tipo.Bicho, Tipo.Fuego), 0.5 },
                { (Tipo.Bicho, Tipo.Hada), 0.5 },
                { (Tipo.Bicho, Tipo.Lucha), 0.5 },
                { (Tipo.Bicho, Tipo.Veneno), 0.5 },
                { (Tipo.Bicho, Tipo.Volador), 0.5 },

                // DRAGON
                { (Tipo.Dragon, Tipo.Dragon), 2 },
                { (Tipo.Dragon, Tipo.Acero), 0.5 },
                { (Tipo.Dragon, Tipo.Hada), 0 },

                // ELECTRICO
                { (Tipo.Electrico, Tipo.Agua), 2 },
                { (Tipo.Electrico, Tipo.Volador), 2 },
                { (Tipo.Electrico, Tipo.Dragon), 0.5 },
                { (Tipo.Electrico, Tipo.Electrico), 0.5 },
                { (Tipo.Electrico, Tipo.Planta), 0.5 },
                { (Tipo.Electrico, Tipo.Tierra), 0 },

                // FANTASMA
                { (Tipo.Fantasma, Tipo.Fantasma), 2 },
                { (Tipo.Fantasma, Tipo.Psiquico), 2 },
                { (Tipo.Fantasma, Tipo.Siniestro), 0.5 },
                { (Tipo.Fantasma, Tipo.Normal), 0 },

                // FUEGO
                { (Tipo.Fuego, Tipo.Acero), 2 },
                { (Tipo.Fuego, Tipo.Bicho), 2 },
                { (Tipo.Fuego, Tipo.Hielo), 2 },
                { (Tipo.Fuego, Tipo.Planta), 2 },
                { (Tipo.Fuego, Tipo.Agua), 0.5 },
                { (Tipo.Fuego, Tipo.Dragon), 0.5 },
                { (Tipo.Fuego, Tipo.Fuego), 0.5 },
                { (Tipo.Fuego, Tipo.Roca), 0.5 },

                // HADA
                { (Tipo.Hada, Tipo.Dragon), 2 },
                { (Tipo.Hada, Tipo.Lucha), 2 },
                { (Tipo.Hada, Tipo.Siniestro), 2 },
                { (Tipo.Hada, Tipo.Acero), 0.5 },
                { (Tipo.Hada, Tipo.Fuego), 0.5 },
                { (Tipo.Hada, Tipo.Veneno), 0.5 },

                // HIELO
                { (Tipo.Hielo, Tipo.Dragon), 2 },
                { (Tipo.Hielo, Tipo.Planta), 2 },
                { (Tipo.Hielo, Tipo.Tierra), 2 },
                { (Tipo.Hielo, Tipo.Volador), 2 },
                { (Tipo.Hielo, Tipo.Acero), 0.5 },
                { (Tipo.Hielo, Tipo.Agua), 0.5 },
                { (Tipo.Hielo, Tipo.Fuego), 0.5 },
                { (Tipo.Hielo, Tipo.Hielo), 0.5 },

                // LUCHA
                { (Tipo.Lucha, Tipo.Acero), 2 },
                { (Tipo.Lucha, Tipo.Hielo), 2 },
                { (Tipo.Lucha, Tipo.Normal), 2 },
                { (Tipo.Lucha, Tipo.Roca), 2 },
                { (Tipo.Lucha, Tipo.Siniestro), 2 },
                { (Tipo.Lucha, Tipo.Bicho), 0.5 },
                { (Tipo.Lucha, Tipo.Hada), 0.5 },
                { (Tipo.Lucha, Tipo.Psiquico), 0.5 },
                { (Tipo.Lucha, Tipo.Veneno), 0.5 },
                { (Tipo.Lucha, Tipo.Volador), 0.5 },
                { (Tipo.Lucha, Tipo.Fantasma), 0 },

                // NORMAL
                { (Tipo.Normal, Tipo.Acero), 0.5 },
                { (Tipo.Normal, Tipo.Roca), 0.5 },
                { (Tipo.Normal, Tipo.Fantasma), 0 },

                // PLANTA
                { (Tipo.Planta, Tipo.Agua), 2 },
                { (Tipo.Planta, Tipo.Roca), 2 },
                { (Tipo.Planta, Tipo.Tierra), 2 },
                { (Tipo.Planta, Tipo.Acero), 0.5 },
                { (Tipo.Planta, Tipo.Bicho), 0.5 },
                { (Tipo.Planta, Tipo.Dragon), 0.5 },
                { (Tipo.Planta, Tipo.Fuego), 0.5 },
                { (Tipo.Planta, Tipo.Planta), 0.5 },
                { (Tipo.Planta, Tipo.Veneno), 0.5 },
                { (Tipo.Planta, Tipo.Volador), 0.5 },

                // PSIQUICO
                { (Tipo.Psiquico, Tipo.Lucha), 2 },
                { (Tipo.Psiquico, Tipo.Veneno), 2 },
                { (Tipo.Psiquico, Tipo.Acero), 0.5 },
                { (Tipo.Psiquico, Tipo.Psiquico), 0.5 },
                { (Tipo.Psiquico, Tipo.Siniestro), 0 },

                // ROCA
                { (Tipo.Roca, Tipo.Bicho), 2 },
                { (Tipo.Roca, Tipo.Fuego), 2 },
                { (Tipo.Roca, Tipo.Hielo), 2 },
                { (Tipo.Roca, Tipo.Volador), 2 },
                { (Tipo.Roca, Tipo.Acero), 0.5 },
                { (Tipo.Roca, Tipo.Lucha), 0.5 },
                { (Tipo.Roca, Tipo.Tierra), 0.5 },

                // SINIESTRO
                { (Tipo.Siniestro, Tipo.Fantasma), 2 },
                { (Tipo.Siniestro, Tipo.Psiquico), 2 },
                { (Tipo.Siniestro, Tipo.Hada), 0.5 },
                { (Tipo.Siniestro, Tipo.Lucha), 0.5 },
                { (Tipo.Siniestro, Tipo.Siniestro), 0.5 },

                // TIERRA
                { (Tipo.Tierra, Tipo.Acero), 2 },
                { (Tipo.Tierra, Tipo.Electrico), 2 },
                { (Tipo.Tierra, Tipo.Fuego), 2 },
                { (Tipo.Tierra, Tipo.Roca), 2 },
                { (Tipo.Tierra, Tipo.Veneno), 2 },
                { (Tipo.Tierra, Tipo.Bicho), 0.5 },
                { (Tipo.Tierra, Tipo.Planta), 0.5 },
                { (Tipo.Tierra, Tipo.Volador), 0 },

                // VENENO
                { (Tipo.Veneno, Tipo.Hada), 2 },
                { (Tipo.Veneno, Tipo.Planta), 2 },
                { (Tipo.Veneno, Tipo.Fantasma), 0.5 },
                { (Tipo.Veneno, Tipo.Roca), 0.5 },
                { (Tipo.Veneno, Tipo.Tierra), 0.5 },
                { (Tipo.Veneno, Tipo.Veneno), 0.5 },
                { (Tipo.Veneno, Tipo.Acero), 0 },

                // VOLADOR
                { (Tipo.Volador, Tipo.Bicho), 2 },
                { (Tipo.Volador, Tipo.Lucha), 2 },
                { (Tipo.Volador, Tipo.Planta), 2 },
                { (Tipo.Volador, Tipo.Acero), 0.5 },
                { (Tipo.Volador, Tipo.Electrico), 0.5 },
                { (Tipo.Volador, Tipo.Roca), 0.5 },
            };

        public static double GetEfectividad(Tipo atacante, Tipo defensor)
        {
            return Excepciones.TryGetValue((atacante, defensor), out var mult) ? mult : 1d;
        }

        // Defensor con 1 o 2 tipos
        public static double GetEfectividad(Tipo atacante, IReadOnlyCollection<Tipo> tiposDefensor)
        {
            if (tiposDefensor is null || tiposDefensor.Count == 0 || tiposDefensor.Count > 2)
                throw new ArgumentException("El defensor debe tener 1 o 2 tipos.", nameof(tiposDefensor));

            double total = 1d;
            foreach (var tipoDefensor in tiposDefensor.Distinct())
            {
                total *= GetEfectividad(atacante, tipoDefensor);
            }

            return total;
        }

        // Conveniencia para 1 o 2 tipos
        public static double GetEfectividad(Tipo atacante, Tipo defensor1, Tipo? defensor2 = null)
        {
            var mult1 = GetEfectividad(atacante, defensor1);
            if (defensor2 is null) return mult1;

            var mult2 = GetEfectividad(atacante, defensor2.Value);
            return mult1 * mult2;
        }
    }
}