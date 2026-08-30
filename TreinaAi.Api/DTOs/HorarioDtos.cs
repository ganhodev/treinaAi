using TreinaAi.Api.Models;

namespace TreinaAi.Api.DTOs;

public class HorarioDisponivelDto
{
    public int Id { get; set; }

    public string DiaSemana { get; set; } = string.Empty;

    public string HoraInicio { get; set; } = string.Empty;

    public string HoraFim { get; set; } = string.Empty;

    public string ProfessorNome { get; set; } = string.Empty;

    public int VagasDisponiveis { get; set; }

    public int CapacidadeMaxima { get; set; }
}

public class CriarHorarioDto
{
    public DiaSemana DiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFim { get; set; }

    public int CapacidadeMaxima { get; set; }
    
    public string ProfessorId { get; set; } = string.Empty;
}