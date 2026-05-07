namespace EduqPlus.API.DTOs {
    public class AvaliacaoCreateDTO {
        public Guid CursoId { get; set; }
        public Guid UsuarioId { get; set; }
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
        public string? UrlComprovante { get; set; }
        public DateTime Data { get; set; }
        public string StatusComprovante { get; set; } = null!;
        public bool IsCompraVerificada { get; set; }
    }
}
