using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmmenTravel.src.AmmenTravel.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AgregaHistorialEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHistorialNotificacionesEventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinoTuristicoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventoTicketmasterId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHistorialNotificacionesEventos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHistorialNotificacionesEventos_UserId_DestinoTuristicoId",
                table: "AppHistorialNotificacionesEventos",
                columns: new[] { "UserId", "DestinoTuristicoId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHistorialNotificacionesEventos");
        }
    }
}
