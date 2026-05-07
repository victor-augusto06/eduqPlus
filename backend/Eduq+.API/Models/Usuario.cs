using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Models;

[Table("usuario")]
[Index("Email", Name = "Email", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(255)]
    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string SenhaHash { get; set; } = null!;

    [StringLength(50)]
    public string Role { get; set; } = null!;

    [InverseProperty("Usuario")]
    public virtual ICollection<Avaliacao> Avaliacaos { get; set; } = new List<Avaliacao>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Denuncia> Denuncia { get; set; } = new List<Denuncia>();
}
