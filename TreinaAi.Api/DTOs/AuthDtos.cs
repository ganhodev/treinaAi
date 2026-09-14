using System.ComponentModel.DataAnnotations;

namespace TreinaAi.Api.DTOs;

   public class CadastroDto
   {
      [Required]
      public string Nome { get; set; } = string.Empty;

      [Required]
      [EmailAddress]
      public string Email { get; set; } = string.Empty;

      [Required]
      public string Senha { get; set; } = string.Empty;

      public bool EhProfessor { get; set; } 
   }

public class LoginDto
   {
      [Required]
      [EmailAddress]
      public string Email { get; set; } = string.Empty;

      [Required]
      public string Senha { get; set; } = string.Empty;
   }

public class AuthResponseDto {
    public string Token { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public bool EhProfessor { get; set; } 
}