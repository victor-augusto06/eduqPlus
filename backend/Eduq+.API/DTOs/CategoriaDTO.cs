namespace EduqPlus.API.DTOs {
    public class CategoriaCreateDTO {
        public string Nome { get; set; } = string.Empty;
    }

    public class CategoriaResponseDTO {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}