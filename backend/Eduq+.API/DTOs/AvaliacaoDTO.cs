using EduqPlus.API.Enums;

namespace EduqPlus.API.DTOs {
    public class AvaliacaoCreateDTO {
        public Guid CursoId { get; set; }
        public Guid UsuarioId { get; set; }
        public int NotaEntrega { get; set; }
        public int NotaSuporte { get; set; }
        public string? Comentario { get; set; }
        public IFormFile? UrlComprovante { get; set; }

    }
    public class AvaliacaoUpdateDTO {
        public int NotaEntrega { get; set; }
        public int NotaSuporte { get; set; }
        public string? Comentario { get; set; }
        public string? UrlComprovante { get; set; }
    }
    public class AvaliacaoResponseDTO {
        public Guid Id { get; set; }
        public Guid CursoId { get; set; }
        public Guid UsuarioId { get; set; }
        public int NotaEntrega { get; set; }
        public int NotaSuporte { get; set; }
        public string? Comentario { get; set; }
        public DateTime Data { get; set; }
        public EStatusComprovante StatusComprovante { get; set; }
        public bool IsCompraVerificada { get; set; }
    }
    public class AvaliacaoResponseAdminDTO: AvaliacaoResponseDTO {
        public string? UrlComprovante { get; set; }
    }
}
