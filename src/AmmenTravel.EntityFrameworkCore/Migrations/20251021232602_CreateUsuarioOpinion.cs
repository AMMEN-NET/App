using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmmenTravel.Migrations
{
    /// <inheritdoc />
    public partial class CreateUsuarioOpinion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "poblacion",
                table: "AppDestinos",
                newName: "Poblacion");

            migrationBuilder.RenameColumn(
                name: "pais",
                table: "AppDestinos",
                newName: "Pais");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "AppDestinos",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "fotoURL",
                table: "AppDestinos",
                newName: "FotoURL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Poblacion",
                table: "AppDestinos",
                newName: "poblacion");

            migrationBuilder.RenameColumn(
                name: "Pais",
                table: "AppDestinos",
                newName: "pais");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "AppDestinos",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "FotoURL",
                table: "AppDestinos",
                newName: "fotoURL");
        }
    }
}
