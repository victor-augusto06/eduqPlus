using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduqPlus.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Models;

[Table("auditoria")]
[Index("CursoId", Name = "FK_Auditoria_Curso")]
public partial class Auditoria
{
    [Key]
    public Guid Id { get; set; }

    public Guid CursoId { get; set; }

    public Guid AuditorId { get; set; }

    [ForeignKey("AuditorId")]
    public virtual Usuario Auditor { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime DataAuditoria { get; set; }

    [StringLength(100)]
    public string CriterioAnalisado { get; set; } = null!;

    public EStatusAuditoria Resultado { get; set; }

    [Column(TypeName = "text")]
    public string? ObservacaoAuditor { get; set; }

    [ForeignKey("CursoId")]
    [InverseProperty("Auditoria")]
    public virtual Curso Curso { get; set; } = null!;
}
