using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TreinaAi.Api.Data;
using TreinaAi.Api.DTOs;
using TreinaAi.Api.Models;

namespace TreinaAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgendamentoController : ControllerBase
{
    private readonly AppDbContext _context;

    public AgendamentoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarAgendamentoDto dto)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (usuarioId == null)
        {
            return Unauthorized();
        }

        var horario = await _context.HorariosTemplate.FindAsync(dto.HorarioTemplateId);

        if (horario == null)
        {
            return NotFound("Horário não encontrado.");
        }

        var vagasOcupadas = await _context.Agendamentos
            .CountAsync(a => a.HorarioTemplateId == dto.HorarioTemplateId
                && a.Data == dto.Data
                && a.Status == StatusAgendamento.Confirmado);

        if (vagasOcupadas >= horario.CapacidadeMaxima)
        {
            return BadRequest("Não há vagas disponíveis para este horário.");
        }

        var agendamento = new Agendamento
        {
            HorarioTemplateId = dto.HorarioTemplateId,
            Data = dto.Data,
            UsuarioId = usuarioId,
            Status = StatusAgendamento.Confirmado
        };

        _context.Agendamentos.Add(agendamento);
        await _context.SaveChangesAsync();

        return Ok(agendamento);
    }

    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var agendamento = await _context.Agendamentos.FindAsync(id);

        if (agendamento == null)
        {
            return NotFound("Agendamento não encontrado.");
        }

        agendamento.Status = StatusAgendamento.Cancelado;
        await _context.SaveChangesAsync();

        return Ok(agendamento);
    }

    [HttpGet("professor")]
public async Task<IActionResult> ListarPorProfessor()
{
    var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (usuarioId == null)
    {
        return Unauthorized();
    }

    var usuarioLogado = await _context.Users.FindAsync(usuarioId);

    if (usuarioLogado == null || !usuarioLogado.EhProfessor)
    {
        return Forbid();
    }

    var agendamentos = await _context.Agendamentos
        .Include(a => a.Usuario)
        .Include(a => a.HorarioTemplate)
        .Where(a => a.HorarioTemplate!.ProfessorId == usuarioId)
        .Select(a => new AgendamentoProfessorDto
        {
            Id = a.Id,
            AlunoNome = a.Usuario!.Nome,
            Data = a.Data,
            HoraInicio = a.HorarioTemplate!.HoraInicio.ToString("HH:mm"),
            HoraFim = a.HorarioTemplate!.HoraFim.ToString("HH:mm"),
            Status = a.Status.ToString()
        })
        .ToListAsync();

    return Ok(agendamentos);
}
}
