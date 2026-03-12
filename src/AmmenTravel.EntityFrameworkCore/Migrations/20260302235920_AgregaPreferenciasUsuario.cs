using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmmenTravel.Migrations
{
    /// <inheritdoc />
    public partial class AgregaPreferenciasUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPreferenciasNotificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnPantalla = table.Column<bool>(type: "bit", nullable: false),
                    PorEmail = table.Column<bool>(type: "bit", nullable: false),
                    Frecuencia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPreferenciasNotificaciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPreferenciasNotificaciones_UserId",
                table: "AppPreferenciasNotificaciones",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPreferenciasNotificaciones");
        }
    }
}
