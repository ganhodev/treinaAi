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
    public async Task<IActionResult> Listar([FromQuery] DateOnly? data = null)
    {
        var horarios = await _context.HorariosTemplate
            .Include(h => h.Professor)
            .OrderBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraInicio)
            .ToListAsync();

        var datasPorHorario = horarios.ToDictionary(
            h => h.Id,
            h => data ?? ObterProximaDataParaHorario(DateOnly.FromDateTime(DateTime.Today), h.DiaSemana));
        var ids = datasPorHorario.Keys.ToArray();
        var datas = datasPorHorario.Values.Distinct().ToArray();
        var vagasOcupadas = await _context.Agendamentos
            .Where(a => ids.Contains(a.HorarioTemplateId)
                && datas.Contains(a.Data)
                && a.Status == StatusAgendamento.Confirmado)
            .GroupBy(a => new { a.HorarioTemplateId, a.Data })
            .Select(g => new { g.Key.HorarioTemplateId, g.Key.Data, Total = g.Count() })
            .ToDictionaryAsync(x => (x.HorarioTemplateId, x.Data), x => x.Total);

        var resultado = new List<HorarioDisponivelDto>();

        foreach (var horario in horarios)
        {
            var dataHorario = datasPorHorario[horario.Id];
            vagasOcupadas.TryGetValue((horario.Id, dataHorario), out var ocupadas);

            resultado.Add(new HorarioDisponivelDto
            {
                Id = horario.Id,
                DiaSemana = horario.DiaSemana.ToString(),
                HoraInicio = horario.HoraInicio.ToString("HH:mm"),
                HoraFim = horario.HoraFim.ToString("HH:mm"),
                ProfessorNome = horario.Professor?.Nome ?? "",
                CapacidadeMaxima = horario.CapacidadeMaxima,
                VagasDisponiveis = Math.Max(0, horario.CapacidadeMaxima - ocupadas)
            });
        }

        return Ok(resultado);
    }

    private static DateOnly ObterProximaDataParaHorario(DateOnly dataReferencia, DiaSemana diaSemana)
    {
        var diaSemanaMap = new Dictionary<DiaSemana, DayOfWeek>
        {
            { DiaSemana.segunda, DayOfWeek.Monday },
            { DiaSemana.terca, DayOfWeek.Tuesday },
            { DiaSemana.quarta, DayOfWeek.Wednesday },
            { DiaSemana.quinta, DayOfWeek.Thursday },
            { DiaSemana.sexta, DayOfWeek.Friday }
        };

        var diaAlvo = diaSemanaMap[diaSemana];
        var data = dataReferencia;

        while (data.DayOfWeek != diaAlvo)
        {
            data = data.AddDays(1);
        }

        return data;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarHorarioDto dto)
    {
        var usuarioLogado = await _userManager.GetUserAsync(User);

        if (usuarioLogado == null || !usuarioLogado.EhProfessor)
        {
            return Forbid();
        }

        if (!Enum.IsDefined(dto.DiaSemana))
        {
            return BadRequest("O dia da semana informado é inválido.");
        }

        if (dto.HoraFim <= dto.HoraInicio)
        {
            return BadRequest("O horário final deve ser posterior ao horário inicial.");
        }

        var duplicado = await _context.HorariosTemplate.AnyAsync(h =>
            h.ProfessorId == usuarioLogado.Id &&
            h.DiaSemana == dto.DiaSemana &&
            h.HoraInicio == dto.HoraInicio &&
            h.HoraFim == dto.HoraFim);
        if (duplicado)
        {
            return Conflict("Você já possui um horário com essa configuração.");
        }

        var horario = new HorarioTemplate
        {
            DiaSemana = dto.DiaSemana,
            HoraInicio = dto.HoraInicio,
            HoraFim = dto.HoraFim,
            CapacidadeMaxima = dto.CapacidadeMaxima,
            ProfessorId = usuarioLogado.Id
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

    [Authorize]
    [HttpDelete("{id}")]
public async Task<IActionResult> Deletar(int id)
{
    var usuarioLogado = await _userManager.GetUserAsync(User);

    if (usuarioLogado == null || !usuarioLogado.EhProfessor)
    {
        return Forbid();
    }

    var horario = await _context.HorariosTemplate
        .FirstOrDefaultAsync(h => h.Id == id && h.ProfessorId == usuarioLogado.Id);

    if (horario == null)
    {
        return NotFound("Horário não encontrado.");
    }

    var possuiAgendamentos = await _context.Agendamentos
        .AnyAsync(a => a.HorarioTemplateId == id);

    if (possuiAgendamentos)
    {
        return Conflict("Não é possível excluir um horário que possui agendamentos.");
    }

    _context.HorariosTemplate.Remove(horario);
    await _context.SaveChangesAsync();

    return NoContent();
}
}