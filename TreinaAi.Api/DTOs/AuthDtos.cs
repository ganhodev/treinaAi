using System.ComponentModel.DataAnnotations;

namespace TreinaAi.Api.DTOs;

   public class CadastroDto
   {
         [Required, StringLength(120, MinimumLength = 2)]
      public string Nome { get; set; } = string.Empty;

      [Required]
      [EmailAddress]
      public string Email { get; set; } = string.Empty;

      [Required, StringLength(100, MinimumLength = 6)]
      public string Senha { get; set; } = string.Empty;
   }

public class LoginDto
   {
      [Required]
      [EmailAddress]
      public string Email { get; set; } = string.Empty;

      [Required, StringLength(100, MinimumLength = 1)]
      public string Senha { get; set; } = string.Empty;
   }

public class AuthResponseDto {
    public string Token { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public bool EhProfessor { get; set; } 
}