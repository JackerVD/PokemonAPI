using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movimientos",
                columns: table => new
                {
                    MovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Poder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimientos", x => x.MovimientoId);
                    table.CheckConstraint("CK_Movimiento_Poder", "[Poder] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Pokemons",
                columns: table => new
                {
                    PokemonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    SaludTotalBase = table.Column<int>(type: "int", nullable: false),
                    AtaqueBase = table.Column<int>(type: "int", nullable: false),
                    DefensaBase = table.Column<int>(type: "int", nullable: false),
                    AtaqueEspecialBase = table.Column<int>(type: "int", nullable: false),
                    DefensaEspecialBase = table.Column<int>(type: "int", nullable: false),
                    VelocidadBase = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pokemons", x => x.PokemonId);
                    table.CheckConstraint("CK_Pokemon_AtaqueBase", "[AtaqueBase] >= 1");
                    table.CheckConstraint("CK_Pokemon_AtaqueEspecialBase", "[AtaqueEspecialBase] >= 1");
                    table.CheckConstraint("CK_Pokemon_DefensaBase", "[DefensaBase] >= 1");
                    table.CheckConstraint("CK_Pokemon_DefensaEspecialBase", "[DefensaEspecialBase] >= 1");
                    table.CheckConstraint("CK_Pokemon_SaludTotalBase", "[SaludTotalBase] >= 1");
                    table.CheckConstraint("CK_Pokemon_VelocidadBase", "[VelocidadBase] >= 1");
                });

            migrationBuilder.CreateTable(
                name: "MisPokemons",
                columns: table => new
                {
                    MiPokemonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PokemonId = table.Column<int>(type: "int", nullable: false),
                    NombrePersonalizado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    SaludActual = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisPokemons", x => x.MiPokemonId);
                    table.CheckConstraint("CK_MiPokemon_Nivel", "[Nivel] >= 1 AND [Nivel] <= 100");
                    table.CheckConstraint("CK_MiPokemon_SaludActual", "[SaludActual] >= 0");
                    table.ForeignKey(
                        name: "FK_MisPokemons_Pokemons_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemons",
                        principalColumn: "PokemonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PokemonMovimientosPosibles",
                columns: table => new
                {
                    PokemonId = table.Column<int>(type: "int", nullable: false),
                    MovimientoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PokemonMovimientosPosibles", x => new { x.PokemonId, x.MovimientoId });
                    table.ForeignKey(
                        name: "FK_PokemonMovimientosPosibles_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PokemonMovimientosPosibles_Pokemons_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemons",
                        principalColumn: "PokemonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PokemonTipos",
                columns: table => new
                {
                    PokemonId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PokemonTipos", x => new { x.PokemonId, x.Tipo });
                    table.ForeignKey(
                        name: "FK_PokemonTipos_Pokemons_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemons",
                        principalColumn: "PokemonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MiPokemonMovimientos",
                columns: table => new
                {
                    MiPokemonId = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    MovimientoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiPokemonMovimientos", x => new { x.MiPokemonId, x.Slot });
                    table.CheckConstraint("CK_MiPokemonMovimiento_Slot", "[Slot] >= 1 AND [Slot] <= 4");
                    table.ForeignKey(
                        name: "FK_MiPokemonMovimientos_MisPokemons_MiPokemonId",
                        column: x => x.MiPokemonId,
                        principalTable: "MisPokemons",
                        principalColumn: "MiPokemonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MiPokemonMovimientos_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MiPokemonMovimientos_MiPokemonId_MovimientoId",
                table: "MiPokemonMovimientos",
                columns: new[] { "MiPokemonId", "MovimientoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MiPokemonMovimientos_MovimientoId",
                table: "MiPokemonMovimientos",
                column: "MovimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_MisPokemons_PokemonId",
                table: "MisPokemons",
                column: "PokemonId");

            migrationBuilder.CreateIndex(
                name: "IX_PokemonMovimientosPosibles_MovimientoId",
                table: "PokemonMovimientosPosibles",
                column: "MovimientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MiPokemonMovimientos");

            migrationBuilder.DropTable(
                name: "PokemonMovimientosPosibles");

            migrationBuilder.DropTable(
                name: "PokemonTipos");

            migrationBuilder.DropTable(
                name: "MisPokemons");

            migrationBuilder.DropTable(
                name: "Movimientos");

            migrationBuilder.DropTable(
                name: "Pokemons");
        }
    }
}
