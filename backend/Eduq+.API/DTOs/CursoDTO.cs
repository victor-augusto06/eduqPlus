using EduqPlus.API.Enums;

namespace EduqPlus.API.DTOs {
    public class CursoCreateDTO {
        public Guid ProdutorId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string DescricaoOriginal { get; set; } = string.Empty;
        public string PlataformaHospedagem {  get; set; } = string.Empty;
    }
    public class  CursoResponseDTO {
        public Guid Id {get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int? TrustScore { get; set; }
        public EStatusAuditoria StatusAuditoria {  get; set; }
    }
}
