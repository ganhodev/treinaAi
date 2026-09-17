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

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM \"HorariosTemplate\" WHERE \"Id\" = {dto.HorarioTemplateId} FOR UPDATE");

            var horario = await _context.HorariosTemplate
                .SingleOrDefaultAsync(h => h.Id == dto.HorarioTemplateId);

            if (horario == null)
            {
                return NotFound("Horário não encontrado.");
            }

            if (horario.ProfessorId == usuarioId)
            {
                return BadRequest("Professores não podem se inscrever no próprio horário.");
            }

            var diasSemanaMap = new Dictionary<DiaSemana, DayOfWeek>
            {
                { DiaSemana.segunda, DayOfWeek.Monday },
                { DiaSemana.terca, DayOfWeek.Tuesday },
                { DiaSemana.quarta, DayOfWeek.Wednesday },
                { DiaSemana.quinta, DayOfWeek.Thursday },
                { DiaSemana.sexta, DayOfWeek.Friday }
            };

            if (dto.Data < DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest("Não é possível agendar em uma data passada.");
            }

            if (dto.Data.DayOfWeek != diasSemanaMap[horario.DiaSemana])
            {
                return BadRequest($"A data informada não corresponde a uma {horario.DiaSemana}-feira.");
            }

            var jaInscrito = await _context.Agendamentos
                .AnyAsync(a => a.HorarioTemplateId == dto.HorarioTemplateId
                    && a.Data == dto.Data
                    && a.UsuarioId == usuarioId
                    && a.Status == StatusAgendamento.Confirmado);

            if (jaInscrito)
            {
                return BadRequest("Você já está inscrito neste horário nesta data.");
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
            await transaction.CommitAsync();

            return Ok(agendamento);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (usuarioId == null)
        {
            return Unauthorized();
        }

        var agendamento = await _context.Agendamentos
            .Include(a => a.HorarioTemplate)
            .FirstOrDefaultAsync(a => a.Id == id
                && (a.UsuarioId == usuarioId
                    || a.HorarioTemplate!.ProfessorId == usuarioId));

        if (agendamento == null)
        {
            return NotFound("Agendamento não encontrado.");
        }

        if (agendamento.Status == StatusAgendamento.Cancelado)
        {
            return BadRequest("Este agendamento já foi cancelado.");
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

[HttpGet("meus")]
public async Task<IActionResult> ListarMeusAgendamentos()
{
    var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (usuarioId == null)
    {
        return Unauthorized();
    }

    var agendamentos = await _context.Agendamentos
        .Include(a => a.HorarioTemplate)
        .Where(a => a.UsuarioId == usuarioId && a.Status == StatusAgendamento.Confirmado)
        .OrderBy(a => a.Data)
        .Select(a => new MeuAgendamentoDto
        {
            Id = a.Id,
            DiaSemana = a.HorarioTemplate!.DiaSemana.ToString(),
            Data = a.Data,
            HoraInicio = a.HorarioTemplate!.HoraInicio.ToString("HH:mm"),
            HoraFim = a.HorarioTemplate!.HoraFim.ToString("HH:mm"),
            Status = a.Status.ToString()
        })
        .ToListAsync();

    return Ok(agendamentos);
}
}
