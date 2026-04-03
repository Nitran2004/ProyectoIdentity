using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIdentity.Migrations
{
    public partial class cambiosdb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayPalPlanId",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayPalSuscripcionId",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PagosSuscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SuscripcionUsuarioId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EstadoPago = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PayPalTransaccionId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PayPalEventoId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosSuscripcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosSuscripcion_SuscripcionesUsuario_SuscripcionUsuarioId",
                        column: x => x.SuscripcionUsuarioId,
                        principalTable: "SuscripcionesUsuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhooksPayPal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayPalEventoId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TipoEvento = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PayPalSuscripcionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContenidoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaEvento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Procesado = table.Column<bool>(type: "bit", nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhooksPayPal", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosSuscripcion_SuscripcionUsuarioId",
                table: "PagosSuscripcion",
                column: "SuscripcionUsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagosSuscripcion");

            migrationBuilder.DropTable(
                name: "WebhooksPayPal");

            migrationBuilder.DropColumn(
                name: "PayPalPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PayPalSuscripcionId",
                table: "AspNetUsers");
        }
    }
}
