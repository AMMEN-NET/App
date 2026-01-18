using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmmenTravel.Migrations
{
    /// <inheritdoc />
    public partial class AgregadaNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNotificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    LinkReferencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotificaciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNotificaciones_UserId",
                table: "AppNotificaciones",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNotificaciones");
        }
    }
}
