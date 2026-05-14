using EduqPlus.API.Enums;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EduqPlus.API.Models;

public partial class EduqPlusContext : DbContext {
    public EduqPlusContext() {
    }

    public EduqPlusContext(DbContextOptions<EduqPlusContext> options)
        : base(options) {
    }

    public virtual DbSet<Auditoria> Auditoria { get; set; }
    public virtual DbSet<Avaliacao> Avaliacoes { get; set; }
    public virtual DbSet<Curso> Cursos { get; set; }
    public virtual DbSet<Denuncia> Denuncia { get; set; }
    public virtual DbSet<Produtor> Produtors { get; set; }
    public virtual DbSet<PromessaCurso> PromessaCursos { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Categoria> Categorias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {

        bool isNotInMemory = this.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";

        if (isNotInMemory) {
            modelBuilder
                .UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");
        }

        modelBuilder.Entity<Auditoria>(entity => {
            entity.HasKey(e => e.Id);

            if (isNotInMemory) {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.Property(e => e.DataAuditoria).HasDefaultValueSql("CURRENT_TIMESTAMP");
            }

            entity.Property(e => e.Resultado)
                .HasConversion<string>()
                .HasMaxLength(50);

            var fkCurso = entity.HasOne(d => d.Curso).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.CursoId);

            var fkAuditor = entity.HasOne(d => d.Auditor).WithMany()
                .HasForeignKey(d => d.AuditorId)
                .OnDelete(DeleteBehavior.Restrict);

            if (isNotInMemory) {
                fkCurso.HasConstraintName("FK_Auditoria_Curso");
                fkAuditor.HasConstraintName("FK_Auditoria_Auditor");
            }
        });

        modelBuilder.Entity<Avaliacao>(entity => {
            entity.HasKey(e => e.Id);

            if (isNotInMemory) {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.Property(e => e.Data).HasDefaultValueSql("CURRENT_TIMESTAMP");
            }

            entity.Property(e => e.StatusComprovante)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(EStatusComprovante.Pendente);

            var fkCurso = entity.HasOne(d => d.Curso).WithMany(p => p.Avaliacoes)
                .HasForeignKey(d => d.CursoId);

            var fkUsuario = entity.HasOne(d => d.Usuario).WithMany(p => p.Avaliacoes)
                .HasForeignKey(d => d.UsuarioId);

            if (isNotInMemory) {
                fkCurso.HasConstraintName("FK_Avaliacao_Curso");
                fkUsuario.HasConstraintName("FK_Avaliacao_Usuario");
            }
        });

        modelBuilder.Entity<Curso>(entity => {
            entity.HasKey(e => e.Id);

            if (isNotInMemory) {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
            }

            entity.Property(e => e.StatusAuditoria)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(EStatusAuditoria.NaoAuditado)
                .HasSentinel(EStatusAuditoria.NaoAuditado);

            entity.Property(e => e.TrustScore).HasDefaultValue(0);

            entity.Property(e => e.VetorSemantico)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<float[]>(v, (System.Text.Json.JsonSerializerOptions?)null)!
                );

            var fkProdutor = entity.HasOne(d => d.Produtor).WithMany(p => p.Cursos)
                .HasForeignKey(d => d.ProdutorId);

            var fkUsuario = entity.HasOne(d => d.Usuario).WithMany(p => p.Cursos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            if (isNotInMemory) {
                fkProdutor.HasConstraintName("FK_Curso_Produtor");
                fkUsuario.HasConstraintName("FK_Curso_Usuario");
            }
        });

        modelBuilder.Entity<Denuncia>(entity => {
            entity.HasKey(e => e.Id);

            if (isNotInMemory) {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.Property(e => e.Data).HasDefaultValueSql("CURRENT_TIMESTAMP");
            }

            entity.Property(e => e.Categoria)
                .HasConversion<string>()
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(EStatusDenuncia.EmAnalise);

            var fkCurso = entity.HasOne(d => d.Curso).WithMany(p => p.Denuncia)
                .HasForeignKey(d => d.CursoId);

            var fkUsuario = entity.HasOne(d => d.Usuario).WithMany(p => p.Denuncia)
                .HasForeignKey(d => d.UsuarioId);

            if (isNotInMemory) {
                fkCurso.HasConstraintName("FK_Denuncia_Curso");
                fkUsuario.HasConstraintName("FK_Denuncia_Usuario");
            }
        });

        modelBuilder.Entity<Produtor>(entity => {
            entity.HasKey(e => e.Id);
            if (isNotInMemory)
                entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<PromessaCurso>(entity => {
            entity.HasKey(e => e.Id);
            if (isNotInMemory)
                entity.HasKey(e => e.Id).HasName("PRIMARY");

            var fkCurso = entity.HasOne(d => d.Curso).WithMany(p => p.PromessaCursos)
                .HasForeignKey(d => d.CursoId);

            if (isNotInMemory)
                fkCurso.HasConstraintName("FK_PromessaCurso_Curso");
        });

        modelBuilder.Entity<Usuario>(entity => {
            entity.HasKey(e => e.Id);
            if (isNotInMemory)
                entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(ERoleUsuario.Comum);
        });

        modelBuilder.Entity<Categoria>(entity => {
            entity.HasKey(e => e.Id);
            if (isNotInMemory)
                entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);

            var fkCursos = entity.HasMany(c => c.Cursos).WithOne(p => p.Categoria)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            if (isNotInMemory)
                fkCursos.HasConstraintName("FK_Curso_Categoria");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}