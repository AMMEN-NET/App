using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmmenTravel.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Entidades_Estadisticas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialBusquedas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminoBusqueda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncontroResultados = table.Column<bool>(type: "bit", nullable: false),
                    FiltrosUtilizados = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialBusquedas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosApiExterna",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreApi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TiempoDuracionMs = table.Column<int>(type: "int", nullable: false),
                    CodigoEstadoHttp = table.Column<int>(type: "int", nullable: false),
                    FueExitoso = table.Column<bool>(type: "bit", nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosApiExterna", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialBusquedas");

            migrationBuilder.DropTable(
                name: "RegistrosApiExterna");
        }
    }
}
