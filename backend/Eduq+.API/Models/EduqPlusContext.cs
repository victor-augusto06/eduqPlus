using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace EduqPlus.API.Models;

public partial class EduqPlusContext : DbContext
{
    public EduqPlusContext()
    {
    }

    public EduqPlusContext(DbContextOptions<EduqPlusContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditoria> Auditoria { get; set; }

    public virtual DbSet<Avaliacao> Avaliacaos { get; set; }

    public virtual DbSet<Curso> Cursos { get; set; }

    public virtual DbSet<Denuncia> Denuncia { get; set; }

    public virtual DbSet<Produtor> Produtors { get; set; }

    public virtual DbSet<Promessacurso> Promessacursos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;user=root;password=projetovictor;database=EduqPlus", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.DataAuditoria).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Curso).WithMany(p => p.Auditoria).HasConstraintName("FK_Auditoria_Curso");
        });

        modelBuilder.Entity<Avaliacao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Data).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.StatusComprovante).HasDefaultValueSql("'NaoEnviado'");

            entity.HasOne(d => d.Curso).WithMany(p => p.Avaliacaos).HasConstraintName("FK_Avaliacao_Curso");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Avaliacaos).HasConstraintName("FK_Avaliacao_Usuario");
        });

        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.StatusAuditoria).HasDefaultValueSql("'NaoAuditado'");
            entity.Property(e => e.TrustScore).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.Produtor).WithMany(p => p.Cursos).HasConstraintName("FK_Curso_Produtor");
        });

        modelBuilder.Entity<Denuncia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Data).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasDefaultValueSql("'EmAnalise'");

            entity.HasOne(d => d.Curso).WithMany(p => p.Denuncia).HasConstraintName("FK_Denuncia_Curso");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Denuncia).HasConstraintName("FK_Denuncia_Usuario");
        });

        modelBuilder.Entity<Produtor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<Promessacurso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Curso).WithMany(p => p.Promessacursos).HasConstraintName("FK_PromessaCurso_Curso");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Role).HasDefaultValueSql("'Comum'");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
