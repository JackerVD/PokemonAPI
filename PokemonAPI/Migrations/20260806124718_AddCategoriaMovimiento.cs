using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriaMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Movimiento_Poder",
                table: "Movimientos");

            migrationBuilder.AddColumn<int>(
                name: "Categoria",
                table: "Movimientos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Movimiento_Categoria",
                table: "Movimientos",
                sql: "[Categoria] IN (0,1,2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Movimiento_Poder_Segun_Categoria",
                table: "Movimientos",
                sql: "([Categoria] = 2 AND [Poder] = 0) OR ([Categoria] IN (0,1) AND [Poder] > 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Movimiento_Categoria",
                table: "Movimientos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Movimiento_Poder_Segun_Categoria",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Movimientos");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Movimiento_Poder",
                table: "Movimientos",
                sql: "[Poder] >= 0");
        }
    }
}
