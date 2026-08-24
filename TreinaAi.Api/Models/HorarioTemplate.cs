namespace TreinaAi.Api.Models;

public enum DiaSemana {
    segunda,
    terca,
    quarta,
    quinta,
    sexta,

}

public class HorarioTemplate 
{
    public int Id { get; set; }

    public DiaSemana DiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFim { get; set; } 

    public int CapacidadeMaxima { get; set; }

    public string ProfessorId { get; set; } = string.Empty;

    public Usuario? Professor { get; set; }
}