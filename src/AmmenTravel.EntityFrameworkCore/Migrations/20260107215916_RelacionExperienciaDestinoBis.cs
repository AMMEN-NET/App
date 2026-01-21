using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmmenTravel.Migrations
{
    /// <inheritdoc />
    public partial class RelacionExperienciaDestinoBis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_AppExperiencias_AppDestinos_DestinoId",
                table: "AppExperiencias",
                column: "DestinoId",
                principalTable: "AppDestinos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppExperiencias_AppDestinos_DestinoId",
                table: "AppExperiencias");
        }
    }
}
