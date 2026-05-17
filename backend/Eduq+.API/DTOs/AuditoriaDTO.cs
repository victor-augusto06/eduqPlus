using EduqPlus.API.Enums;
using EduqPlus.API.Models;

namespace EduqPlus.API.DTOs {
    public class AuditoriaCreateDTO {
        public Guid CursoId { get; set; }
        public Guid AuditorId { get; set; }
        public string CriterioAnalisado { get; set; } = string.Empty;
        public EStatusAuditoria Resultado { get; set; }
        public string? ObservacaoAuditor {  get; set; }
    }

    public class AuditoriaUpdateDTO {
        public string CriterioAnalisado { get; set; } = string.Empty;
        public EStatusAuditoria Resultado { get; set; }
        public string? ObservacaoAuditor { get; set; }
    }

    public class AuditoriaResponseDTO {
        public Guid Id { get; set; }
        public Guid CursoId { get; set; }
        public Guid AuditorId { get; set; }
        public string NomeAuditor { get; set; } = string.Empty;
        public string TituloCurso { get; set; } = string.Empty;
        public string CriterioAnalisado { get; set; } = string.Empty;
        public DateTime DataAuditoria { get; set; }
        public EStatusAuditoria Resultado { get; set; }
        public string? ObservacaoAuditor { get; set; }
    }
}