using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduqPlus.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Models;

[Table("denuncia")]
[Index("CursoId", Name = "FK_Denuncia_Curso")]
[Index("UsuarioId", Name = "FK_Denuncia_Usuario")]
public partial class Denuncia
{
    [Key]
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid CursoId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Data { get; set; }

    [StringLength(100)]
    public string Categoria { get; set; } = null!;

    [Column(TypeName = "text")]
    public string RelatoDetalhado { get; set; } = null!;

    public EStatusDenuncia Status { get; set; }

    [ForeignKey("CursoId")]
    [InverseProperty("Denuncia")]
    public virtual Curso Curso { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Denuncia")]
    public virtual Usuario Usuario { get; set; } = null!;
}
