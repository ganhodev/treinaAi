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
}