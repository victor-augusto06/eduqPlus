using EduqPlus.API.Enums;

namespace EduqPlus.API.DTOs {
    public class CursoCreateDTO {
        public Guid ProdutorId { get; set; }
        public Guid CategoriaId { get; set; }
        public Guid? UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string DescricaoOriginal { get; set; } = string.Empty;
        public string PlataformaHospedagem {  get; set; } = string.Empty;
        public ICollection<PromessaCursoCreateDTO> PromessaCursos { get; set; } = new List<PromessaCursoCreateDTO>();
    }

    public class CursoUpdateDTO {
        public string Titulo { get; set; } = string.Empty;
        public string DescricaoOriginal { get; set; } = string.Empty;
        public Guid CategoriaId { get; set; }
        public string PlataformaHospedagem { get; set; } = string.Empty;
        public ICollection<PromessaCursoCreateDTO> PromessaCursos { get; set; } = new List<PromessaCursoCreateDTO>();
    }

    public class CursoUpdateAdminDTO: CursoUpdateDTO {
        public EStatusAuditoria StatusAuditoria { get; set; }
        public int? TrustScore { get; set; }
        public string? ResumoReputacao { get; set; }
        public DateTime? DataUltimaAnaliseIa { get; set; }
    }

    public class  CursoResponseDTO {
        public Guid Id {get; set; }
        public Guid CategoriaId { get; set; }
        public Guid ProdutorId { get; set; }
        public Guid? UsuarioId { get; set; }
        public string DescricaoOriginal { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? PlataformaHospedagem { get; set; }
        public int? TrustScore { get; set; }
        public EStatusAuditoria StatusAuditoria {  get; set; }
        public string? ResumoReputacao { get; set; }
        public ICollection<PromessaCursoResponseDTO> PromessaCursos { get; set; } = new List<PromessaCursoResponseDTO>();
        public ICollection<AuditoriaResponseDTO> Auditoria { get; set; } = new List<AuditoriaResponseDTO>();
        public ICollection<AvaliacaoResponseDTO> Avaliacoes { get; set; } = new List<AvaliacaoResponseDTO>();
        public ICollection<DenunciaResponseDTO> Denuncia { get; set; } = new List<DenunciaResponseDTO>();
    }
}
