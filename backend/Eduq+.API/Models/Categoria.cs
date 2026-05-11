using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Models;

[Table("categorias")]
[Index("Nome", Name = "Nome", IsUnique = true)]
public partial class Categoria {
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = null!;

    [InverseProperty("Categoria")]
    public virtual ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}