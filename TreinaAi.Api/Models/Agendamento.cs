namespace TreinaAi.Api.Models;

 public enum StatusAgendamento {
        Confirmado,
        Pendente,
        Cancelado
    }

public class Agendamento {
    public int IdAgendamento { get; set; }

    public DateOnly dataAgendamento { get; set; } 

    public StatusAgendamento Status { get; set; } = StatusAgendamento.Confirmado;

    public DateTime AgendamentoCriadoEm { get; set; } = DateTime.UtcNow;

    public int HorarioTemplateId { get; set; }

    public HorarioTemplate? HorarioTemplate { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public Usuario? Usuario { get; set; }

}