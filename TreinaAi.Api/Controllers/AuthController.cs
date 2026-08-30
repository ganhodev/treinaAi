using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TreinaAi.Api.DTOs;
using TreinaAi.Api.Models;

namespace TreinaAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<Usuario> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

   [HttpPost("registrar")]
public async Task<IActionResult> Registrar(CadastroDto dto)
{
    var usuario = new Usuario
    {
        UserName = dto.Email,
        Email = dto.Email,
        Nome = dto.Nome,
        EhProfessor = dto.EhProfessor
    };

    var resultado = await _userManager.CreateAsync(usuario, dto.Senha);

    if (!resultado.Succeeded)
    {
        return BadRequest(resultado.Errors);
    }

    return Ok(new { mensagem = "Usuário criado com sucesso!" });
}

    [HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto)
{
    var usuario = await _userManager.FindByEmailAsync(dto.Email);

    if (usuario == null)
    {
        return Unauthorized("Email ou senha inválidos.");
    }

    var senhaValida = await _userManager.CheckPasswordAsync(usuario, dto.Senha);

    if (!senhaValida)
    {
        return Unauthorized("Email ou senha inválidos.");
    }

    var token = GerarToken(usuario);

    return Ok(new AuthResponseDto
    {
        Token = token,
        Nome = usuario.Nome,
        EhProfessor = usuario.EhProfessor
    });
}

[HttpGet("usuarios")]
public IActionResult ListarUsuarios()
{
    var usuarios = _userManager.Users
        .Select(u => new { u.Id, u.Nome, u.Email, u.EhProfessor })
        .ToList();

    return Ok(usuarios);
}

private string GerarToken(Usuario usuario)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id),
        new Claim(ClaimTypes.Email, usuario.Email!),
        new Claim(ClaimTypes.Name, usuario.Nome)
    };

    var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(4),
        signingCredentials: credenciais
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
}