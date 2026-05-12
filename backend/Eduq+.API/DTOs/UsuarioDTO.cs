using EduqPlus.API.Enums;

namespace EduqPlus.API.DTOs {
    public class UsuarioCreateDTO {
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Senha { get; set; } = null!; 
    }

    public class UsuarioUpdateDTO {
        public string Nome { get; set; } = null!;
    }

    public class UsuarioResponseDTO {
        public Guid Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public ERoleUsuario Role { get; set; } 
    }
}
