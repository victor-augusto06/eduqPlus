using EduqPlus.API.Models;

namespace EduqPlus.API.DTOs {
    public class AuditoriaCreateDTO {
        public Guid CursoId { get; set; }
        public string CriterioAnalisado { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public string? ObservacaoAuditor {  get; set; }
    }

    public class AuditoriaResponseDTO {
        public Guid Id { get; set; }
        public Guid CursoId { get; set; }
        public string TituloCurso { get; set; } = string.Empty;
        public DateTime DataAuditoria { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public string? ObservacaoAuditor { get; set; }
    }
}