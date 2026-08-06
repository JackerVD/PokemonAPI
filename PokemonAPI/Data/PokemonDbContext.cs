using Microsoft.EntityFrameworkCore;
using PokemonAPI.Models;

namespace PokemonAPI.Data
{
    public class PokemonDbContext : DbContext
    {
        public PokemonDbContext(DbContextOptions<PokemonDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pokemon> Pokemons => Set<Pokemon>();
        public DbSet<Movimiento> Movimientos => Set<Movimiento>();
        public DbSet<MiPokemon> MisPokemons => Set<MiPokemon>();
        public DbSet<PokemonTipo> PokemonTipos => Set<PokemonTipo>();
        public DbSet<PokemonMovimientoPosible> PokemonMovimientosPosibles => Set<PokemonMovimientoPosible>();
        public DbSet<MiPokemonMovimiento> MiPokemonMovimientos => Set<MiPokemonMovimiento>();
        public DbSet<PokemonMovimiento> PokemonMovimientos => Set<PokemonMovimiento>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Pokemon
            modelBuilder.Entity<Pokemon>(entity =>
            {
                entity.HasKey(p => p.PokemonId);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(80);
                entity.HasIndex(p => p.Nombre).IsUnique();
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Pokemon_SaludTotalBase", "[SaludTotalBase] >= 1");
                    t.HasCheckConstraint("CK_Pokemon_AtaqueBase", "[AtaqueBase] >= 1");
                    t.HasCheckConstraint("CK_Pokemon_DefensaBase", "[DefensaBase] >= 1");
                    t.HasCheckConstraint("CK_Pokemon_AtaqueEspecialBase", "[AtaqueEspecialBase] >= 1");
                    t.HasCheckConstraint("CK_Pokemon_DefensaEspecialBase", "[DefensaEspecialBase] >= 1");
                    t.HasCheckConstraint("CK_Pokemon_VelocidadBase", "[VelocidadBase] >= 1");
                });
            });

            // Movimiento
            modelBuilder.Entity<Movimiento>(entity =>
            {
                entity.HasKey(m => m.MovimientoId);
                entity.Property(m => m.Nombre).IsRequired().HasMaxLength(80);
                entity.HasIndex(m => m.Nombre).IsUnique();
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Movimiento_Categoria", "[Categoria] IN (0,1,2)");
                    t.HasCheckConstraint(
                        "CK_Movimiento_Poder_Segun_Categoria",
                        "([Categoria] = 2 AND [Poder] = 0) OR ([Categoria] IN (0,1) AND [Poder] > 0)"
                    );
                });
            });

            // MiPokemon
            modelBuilder.Entity<MiPokemon>(entity =>
            {
                entity.HasKey(mp => mp.MiPokemonId);

                entity.HasOne(mp => mp.PokemonBase)
                    .WithMany()
                    .HasForeignKey(mp => mp.PokemonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_MiPokemon_Nivel", "[Nivel] >= 1 AND [Nivel] <= 100");
                    t.HasCheckConstraint("CK_MiPokemon_SaludActual", "[SaludActual] >= 0");
                });
            });

            // PokemonTipo (N:N Pokemon <-> Tipo)
            modelBuilder.Entity<PokemonTipo>(entity =>
            {
                entity.HasKey(pt => new { pt.PokemonId, pt.Tipo });

                entity.HasOne(pt => pt.Pokemon)
                    .WithMany(p => p.Tipos)
                    .HasForeignKey(pt => pt.PokemonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PokemonMovimiento>(entity =>
            {
                entity.HasKey(pm => new { pm.PokemonId, pm.MovimientoId });

                entity.HasOne(pm => pm.Pokemon)
                    .WithMany(p => p.Movimientos)
                    .HasForeignKey(pm => pm.PokemonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pm => pm.Movimiento)
                    .WithMany(m => m.PokemonsQueLoTienen)
                    .HasForeignKey(pm => pm.MovimientoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PokemonMovimientoPosible (N:N Pokemon <-> Movimiento)
            modelBuilder.Entity<PokemonMovimientoPosible>(entity =>
            {
                entity.HasKey(pm => new { pm.PokemonId, pm.MovimientoId });

                entity.HasOne(pm => pm.Pokemon)
                    .WithMany(p => p.MovimientosPosibles)
                    .HasForeignKey(pm => pm.PokemonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pm => pm.Movimiento)
                    .WithMany(m => m.PokemonsQuePuedenAprenderlo)
                    .HasForeignKey(pm => pm.MovimientoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MiPokemonMovimiento (asignacion a slots 1..4)
            modelBuilder.Entity<MiPokemonMovimiento>(entity =>
            {
                // Clave natural por hueco del Pokemon
                entity.HasKey(mm => new { mm.MiPokemonId, mm.Slot });

                entity.HasOne(mm => mm.MiPokemon)
                    .WithMany(mp => mp.Movimientos)
                    .HasForeignKey(mm => mm.MiPokemonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mm => mm.Movimiento)
                    .WithMany(m => m.MisPokemonsQueLoTienen)
                    .HasForeignKey(mm => mm.MovimientoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Evita repetir el mismo movimiento en el mismo MiPokemon
                entity.HasIndex(mm => new { mm.MiPokemonId, mm.MovimientoId }).IsUnique();

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_MiPokemonMovimiento_Slot", "[Slot] >= 1 AND [Slot] <= 4");
                });
            });
        }
    }
}