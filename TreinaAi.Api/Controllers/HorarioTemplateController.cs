using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TreinaAi.Api.Data;
using TreinaAi.Api.DTOs;
using TreinaAi.Api.Models;

namespace TreinaAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HorarioTemplateController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public HorarioTemplateController(AppDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var horarios = await _context.HorariosTemplate
            .Include(h => h.Professor)
            .ToListAsync();

        var resultado = new List<HorarioDisponivelDto>();

        foreach (var horario in horarios)
        {
            var vagasOcupadas = await _context.Agendamentos
                .CountAsync(a => a.HorarioTemplateId == horario.Id
                    && a.Status == StatusAgendamento.Confirmado);

            resultado.Add(new HorarioDisponivelDto
            {
                Id = horario.Id,
                DiaSemana = horario.DiaSemana.ToString(),
                HoraInicio = horario.HoraInicio.ToString("HH:mm"),
                HoraFim = horario.HoraFim.ToString("HH:mm"),
                ProfessorNome = horario.Professor?.Nome ?? "",
                CapacidadeMaxima = horario.CapacidadeMaxima,
                VagasDisponiveis = horario.CapacidadeMaxima - vagasOcupadas
            });
        }

        return Ok(resultado);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Criar(CriarHorarioDto dto)
    {
        var usuarioLogado = await _userManager.GetUserAsync(User);

        if (usuarioLogado == null || !usuarioLogado.EhProfessor)
        {
            return Forbid();
        }

        var horario = new HorarioTemplate
        {
            DiaSemana = dto.DiaSemana,
            HoraInicio = dto.HoraInicio,
            HoraFim = dto.HoraFim,
            CapacidadeMaxima = dto.CapacidadeMaxima,
            ProfessorId = dto.ProfessorId
        };

        _context.HorariosTemplate.Add(horario);
        await _context.SaveChangesAsync();

        return Ok(new
    {
        horario.Id,
        horario.DiaSemana,
        horario.HoraInicio,
        horario.HoraFim,
        horario.CapacidadeMaxima,
        horario.ProfessorId
    });
    }
}