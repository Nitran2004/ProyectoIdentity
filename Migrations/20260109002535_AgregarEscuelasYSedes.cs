using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIdentity.Migrations
{
    public partial class AgregarEscuelasYSedes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SedeEscuela_Escuelas_EscuelaId",
                table: "SedeEscuela");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SedeEscuela",
                table: "SedeEscuela");

            migrationBuilder.RenameTable(
                name: "SedeEscuela",
                newName: "SedesEscuelas");

            migrationBuilder.RenameIndex(
                name: "IX_SedeEscuela_EscuelaId",
                table: "SedesEscuelas",
                newName: "IX_SedesEscuelas_EscuelaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SedesEscuelas",
                table: "SedesEscuelas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SedesEscuelas_Escuelas_EscuelaId",
                table: "SedesEscuelas",
                column: "EscuelaId",
                principalTable: "Escuelas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SedesEscuelas_Escuelas_EscuelaId",
                table: "SedesEscuelas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SedesEscuelas",
                table: "SedesEscuelas");

            migrationBuilder.RenameTable(
                name: "SedesEscuelas",
                newName: "SedeEscuela");

            migrationBuilder.RenameIndex(
                name: "IX_SedesEscuelas_EscuelaId",
                table: "SedeEscuela",
                newName: "IX_SedeEscuela_EscuelaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SedeEscuela",
                table: "SedeEscuela",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SedeEscuela_Escuelas_EscuelaId",
                table: "SedeEscuela",
                column: "EscuelaId",
                principalTable: "Escuelas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
