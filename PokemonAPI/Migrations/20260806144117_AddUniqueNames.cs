using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NombrePersonalizado",
                table: "MisPokemons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pokemons_Nombre",
                table: "Pokemons",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_Nombre",
                table: "Movimientos",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pokemons_Nombre",
                table: "Pokemons");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_Nombre",
                table: "Movimientos");

            migrationBuilder.AlterColumn<string>(
                name: "NombrePersonalizado",
                table: "MisPokemons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
