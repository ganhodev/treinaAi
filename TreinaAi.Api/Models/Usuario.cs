using Microsoft.AspNetCore.Identity;

namespace TreinaAi.Api.Models;

public class Usuario : IdentityUser
{
    public string Nome { get; set; } = string.Empty;
    
    public bool EhProfessor { get; set; } = false;
}