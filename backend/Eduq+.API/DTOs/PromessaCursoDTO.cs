namespace EduqPlus.API.DTOs {
    public class PromessaCursoCreateDTO {
        public Guid CursoId { get; set; }
        public string Descricao { get; set; } = null!;
    }

    public class PromessaCursoUpdateDTO {
        public string Descricao { get; set; } = null!;
    }

    public class PromessaCursoUpdateAdminDTO : PromessaCursoUpdateDTO {
        public bool? CumpridaNaAuditoria { get; set; }
    }

    public class PromessaCursoResponseDTO {
        public Guid Id { get; set; }
        public Guid CursoId { get; set; }
        public string Descricao { get; set; } = null!;
        public bool? CumpridaNaAuditoria { get; set; }
    }
}