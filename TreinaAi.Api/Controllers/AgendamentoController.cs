using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TreinaAi.Api.Data;
using TreinaAi.Api.DTOs;
using TreinaAi.Api.Models;

namespace TreinaAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        UsuarioId = dto.UsuarioId,
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
}