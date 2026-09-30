using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreinaAi.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HorariosTemplate_ProfessorId",
                table: "HorariosTemplate");

            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_HorarioTemplateId",
                table: "Agendamentos");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosTemplate_ProfessorId_DiaSemana_HoraInicio_HoraFim",
                table: "HorariosTemplate",
                columns: new[] { "ProfessorId", "DiaSemana", "HoraInicio", "HoraFim" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_HorarioTemplateId_Data_UsuarioId",
                table: "Agendamentos",
                columns: new[] { "HorarioTemplateId", "Data", "UsuarioId" },
                unique: true,
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HorariosTemplate_ProfessorId_DiaSemana_HoraInicio_HoraFim",
                table: "HorariosTemplate");

            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_HorarioTemplateId_Data_UsuarioId",
                table: "Agendamentos");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosTemplate_ProfessorId",
                table: "HorariosTemplate",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_HorarioTemplateId",
                table: "Agendamentos",
                column: "HorarioTemplateId");
        }
    }
}
