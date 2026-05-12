using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Models;

[Table("produtor")]
public partial class Produtor
{
    [Key]
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    [StringLength(255)]
    public string Nome { get; set; } = null!;

    [StringLength(100)]
    public string? NichoPrincipal { get; set; }

    [Column(TypeName = "text")]
    public string? LinksSociais { get; set; }

    [InverseProperty("Produtor")]
    public virtual ICollection<Curso> Cursos { get; set; } = new List<Curso>();

    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;
}
