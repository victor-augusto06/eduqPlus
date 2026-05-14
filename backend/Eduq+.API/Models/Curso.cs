using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduqPlus.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Models;

[Table("curso")]
[Index("ProdutorId", Name = "FK_Curso_Produtor")]
public partial class Curso
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProdutorId { get; set; }

    public Guid CategoriaId { get; set; }

    public Guid? UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("Cursos")]
    public virtual Usuario? Usuario { get; set; }

    [StringLength(255)]
    public string Titulo { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? DescricaoOriginal { get; set; }

    [StringLength(100)]
    public string? PlataformaHospedagem { get; set; }

    public EStatusAuditoria? StatusAuditoria { get; set; }

    public int? TrustScore { get; set; }

    [Column(TypeName = "text")]
    public string? ResumoReputacao { get; set; }

    [Column("DataUltimaAnaliseIA", TypeName = "datetime")]
    public DateTime? DataUltimaAnaliseIa { get; set; }

    [InverseProperty("Curso")]
    public virtual ICollection<Auditoria> Auditoria { get; set; } = new List<Auditoria>();

    [InverseProperty("Curso")]
    public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();

    [InverseProperty("Curso")]
    public virtual ICollection<Denuncia> Denuncia { get; set; } = new List<Denuncia>();

    [ForeignKey("ProdutorId")]
    [InverseProperty("Cursos")]
    public virtual Produtor Produtor { get; set; } = null!;

    [ForeignKey("CategoriaId")]
    [InverseProperty("Cursos")]
    public virtual Categoria Categoria { get; set; } = null!;

    [InverseProperty("Curso")]
    public virtual ICollection<PromessaCurso> PromessaCursos { get; set; } = new List<PromessaCurso>();
}
