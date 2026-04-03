using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIdentity.Migrations
{
    public partial class CrearTablaSuscripcionesUsuario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsElNacional",
                table: "TablaPosiciones");

            migrationBuilder.DropColumn(
                name: "PreapprovalId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ImagenClub",
                table: "TablaPosiciones",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Visitante",
                table: "Partidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Local",
                table: "Partidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImagenLocal",
                table: "Partidos",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImagenVisitante",
                table: "Partidos",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlanesMembresia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PayPalProductoId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PayPalPlanId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesMembresia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuscripcionesUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlanMembresiaId = table.Column<int>(type: "int", nullable: false),
                    PayPalSuscripcionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCancelacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EsActual = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuscripcionesUsuario_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuscripcionesUsuario_PlanesMembresia_PlanMembresiaId",
                        column: x => x.PlanMembresiaId,
                        principalTable: "PlanesMembresia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesUsuario_PlanMembresiaId",
                table: "SuscripcionesUsuario",
                column: "PlanMembresiaId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesUsuario_UsuarioId",
                table: "SuscripcionesUsuario",
                column: "UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuscripcionesUsuario");

            migrationBuilder.DropTable(
                name: "PlanesMembresia");

            migrationBuilder.DropColumn(
                name: "ImagenLocal",
                table: "Partidos");

            migrationBuilder.DropColumn(
                name: "ImagenVisitante",
                table: "Partidos");

            migrationBuilder.AlterColumn<string>(
                name: "ImagenClub",
                table: "TablaPosiciones",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsElNacional",
                table: "TablaPosiciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Visitante",
                table: "Partidos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Local",
                table: "Partidos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PreapprovalId",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
