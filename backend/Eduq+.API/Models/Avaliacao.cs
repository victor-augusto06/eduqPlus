using EduqPlus.API.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public double NotaEntrega { get; set; }

    public double NotaSuporte { get; set; }

    [Column(TypeName = "text")]
    public string? Comentario { get; set; }

    [StringLength(500)]
    public string? UrlComprovante { get; set; }

    public EStatusComprovante StatusComprovante { get; set; }

    public bool IsCompraVerificada { get; set; }

    [ForeignKey("CursoId")]
    [InverseProperty("Avaliacoes")]
    public virtual Curso Curso { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Avaliacoes")]
    public virtual Usuario Usuario { get; set; } = null!;
}
