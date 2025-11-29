using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmmenTravel.Migrations
{
    /// <inheritdoc />
    public partial class agregadoDeListaDeFavoritos2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppListasFavoritos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppListasFavoritos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLineasListasFavoritos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListaFavoritoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinoTuristicoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLineasListasFavoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLineasListasFavoritos_AppDestinos_DestinoTuristicoId",
                        column: x => x.DestinoTuristicoId,
                        principalTable: "AppDestinos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppLineasListasFavoritos_AppListasFavoritos_ListaFavoritoId",
                        column: x => x.ListaFavoritoId,
                        principalTable: "AppListasFavoritos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLineasListasFavoritos_DestinoTuristicoId",
                table: "AppLineasListasFavoritos",
                column: "DestinoTuristicoId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLineasListasFavoritos_ListaFavoritoId",
                table: "AppLineasListasFavoritos",
                column: "ListaFavoritoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLineasListasFavoritos");

            migrationBuilder.DropTable(
                name: "AppListasFavoritos");
        }
    }
}
