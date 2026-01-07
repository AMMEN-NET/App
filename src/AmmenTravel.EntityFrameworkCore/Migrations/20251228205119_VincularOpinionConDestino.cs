using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmmenTravel.Migrations
{
    /// <inheritdoc />
    public partial class VincularOpinionConDestino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppOpiniones_DestinoTuristicoId",
                table: "AppOpiniones",
                column: "DestinoTuristicoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppOpiniones_AppDestinos_DestinoTuristicoId",
                table: "AppOpiniones",
                column: "DestinoTuristicoId",
                principalTable: "AppDestinos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppOpiniones_AppDestinos_DestinoTuristicoId",
                table: "AppOpiniones");

            migrationBuilder.DropIndex(
                name: "IX_AppOpiniones_DestinoTuristicoId",
                table: "AppOpiniones");
        }
    }
}
