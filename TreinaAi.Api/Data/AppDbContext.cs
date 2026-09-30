using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TreinaAi.Api.Models;

namespace TreinaAi.Api.Data;

public class AppDbContext : IdentityDbContext<Usuario>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<HorarioTemplate> HorariosTemplate { get; set; }
    public DbSet<Agendamento> Agendamentos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HorarioTemplate>()
            .HasIndex(h => new { h.ProfessorId, h.DiaSemana, h.HoraInicio, h.HoraFim })
            .IsUnique();

        modelBuilder.Entity<Agendamento>()
            .HasIndex(a => new { a.HorarioTemplateId, a.Data, a.UsuarioId })
            .IsUnique()
            .HasFilter("\"Status\" = 0");
    }
}