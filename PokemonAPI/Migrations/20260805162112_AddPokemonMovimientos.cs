using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPokemonMovimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Pokemons");

            migrationBuilder.CreateTable(
                name: "PokemonMovimientos",
                columns: table => new
                {
                    PokemonId = table.Column<int>(type: "int", nullable: false),
                    MovimientoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PokemonMovimientos", x => new { x.PokemonId, x.MovimientoId });
                    table.ForeignKey(
                        name: "FK_PokemonMovimientos_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PokemonMovimientos_Pokemons_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemons",
                        principalColumn: "PokemonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PokemonMovimientos_MovimientoId",
                table: "PokemonMovimientos",
                column: "MovimientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PokemonMovimientos");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Pokemons",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
