using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Models;

[Table("avaliacao")]
[Index("CursoId", Name = "FK_Avaliacao_Curso")]
[Index("UsuarioId", Name = "FK_Avaliacao_Usuario")]
public partial class Avaliacao
{
    [Key]
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid CursoId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Data { get; set; }

    public int NotaEntrega { get; set; }

    public int NotaSuporte { get; set; }

    [Column(TypeName = "text")]
    public string? Comentario { get; set; }

    [StringLength(500)]
    public string? UrlComprovante { get; set; }

    [StringLength(50)]
    public string StatusComprovante { get; set; } = null!;

    public bool IsCompraVerificada { get; set; }

    [ForeignKey("CursoId")]
    [InverseProperty("Avaliacaos")]
    public virtual Curso Curso { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Avaliacaos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
