namespace TreinaAi.Api.DTOs;

public class CriarAgendamentoDto
{
    public int HorarioTemplateId { get; set; }

    public DateOnly Data { get; set; }
}

public class AgendamentoProfessorDto
{
    public int Id { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public DateOnly Data { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFim { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}