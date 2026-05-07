using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Models;

[Table("promessacurso")]
[Index("CursoId", Name = "FK_PromessaCurso_Curso")]
public partial class Promessacurso
{
    [Key]
    public Guid Id { get; set; }

    public Guid CursoId { get; set; }

    [Column(TypeName = "text")]
    public string Descricao { get; set; } = null!;

    public bool? CumpridaNaAuditoria { get; set; }

    [ForeignKey("CursoId")]
    [InverseProperty("Promessacursos")]
    public virtual Curso Curso { get; set; } = null!;
}
