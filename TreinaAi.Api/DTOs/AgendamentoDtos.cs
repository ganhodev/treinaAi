namespace TreinaAi.Api.DTOs;

public class CriarAgendamentoDto
{
    public int HorarioTemplateId { get; set; }
    public DateOnly Data { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
}