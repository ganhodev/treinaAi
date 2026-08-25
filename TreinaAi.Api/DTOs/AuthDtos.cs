namespace TreinaAi.Api.DTOs;

   public class CadastroDto
   {
      public string Nome { get; set; } = string.Empty;

      public string Email { get; set; } = string.Empty;

      public string Senha { get; set; } = string.Empty;

      public bool EhProfessor { get; set; } 
   }

public class LoginDto
   {
      public string Email { get; set; } = string.Empty;

      public string Senha { get; set; } = string.Empty;
   }

public class AuthResponseDto {
    public string Token { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public bool EhProfessor { get; set; } 
}