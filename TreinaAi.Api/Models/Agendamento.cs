namespace TreinaAi.Api.Models;

 public enum StatusAgendamento {
        Confirmado,
        Pendente,
        Cancelado
    }

public class Agendamento {
   public int Id { get; set; }

    public DateOnly Data { get; set; } 

    public StatusAgendamento Status { get; set; } = StatusAgendamento.Confirmado;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public int HorarioTemplateId { get; set; }

    public HorarioTemplate? HorarioTemplate { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public Usuario? Usuario { get; set; }

}