using EduqPlus.API.Enums;

namespace EduqPlus.API.DTOs {
    public class DenunciaCreateDTO {
        public Guid UsuarioId { get; set; }
        public Guid CursoId { get; set; }
        public string Categoria { get; set; } = null!;
        public string RelatoDetalhado { get; set; } = null!;
    }
    public class DenunciaResponseDTO {
        public Guid Id { get; set; }
        public Guid CursoId { get; set; }
        public Guid UsuarioId { get; set; }
        public DateTime Data { get; set; }
        public string Categoria { get; set; } = null!;
        public string RelatoDetalhado { get; set; } = null!;
        public EStatusDenuncia Status { get; set; }
    }
}
