namespace EduqPlus.API.DTOs {
    public class ProdutorCreateDTO {
        public string Nome { get; set; } = null!;
        public string? NichoPrincipal { get; set; }
        public string? LinksSociais { get; set; }
    }
    public class ProdutorResponseDTO {
        public Guid Id { get; set; }
        public string Nome { get; set; } = null!;
        public string? NichoPrincipal { get; set; }
        public string? LinksSociais { get; set; }
    }
}
