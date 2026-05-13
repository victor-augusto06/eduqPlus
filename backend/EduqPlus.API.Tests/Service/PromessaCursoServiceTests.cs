using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;
using EduqPlus.API.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduqPlus.API.Tests.Services;

public class PromessaCursoServiceTests {
    private EduqPlusContext CriarContexto() {
        var options = new DbContextOptionsBuilder<EduqPlusContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new EduqPlusContext(options);
    }

    private Usuario CriarUsuarioFake(Guid id, ERoleUsuario role) {
        return new Usuario {
            Id = id,
            Nome = "User Teste",
            Email = $"user{id}@test.com",
            SenhaHash = "hash",
            Role = role
        };
    }

    private Curso CriarCursoFake(Guid id, Guid usuarioId) {
        return new Curso {
            Id = id,
            UsuarioId = usuarioId,
            Titulo = "Curso de C#"
        };
    }

    [Fact]
    public async Task CriarPromessaAsync_DeveCriarComSucesso_QuandoUsuarioEhDonoDoCurso() {
        var context = CriarContexto();
        var donoId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        context.Cursos.Add(CriarCursoFake(cursoId, donoId));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new PromessaCursoService(context);
        var dto = new PromessaCursoCreateDTO { CursoId = cursoId, Descricao = "Promessa" };

        var resultado = await service.CriarPromessaAsync(dto, donoId);

        resultado.Should().NotBeNull();
        context.PromessaCursos.Count().Should().Be(1);
    }

    [Fact]
    public async Task CriarPromessaAsync_DeveLancarExcecao_QuandoUsuarioNaoEhDono() {
        var context = CriarContexto();
        var cursoId = Guid.NewGuid();

        context.Cursos.Add(CriarCursoFake(cursoId, Guid.NewGuid()));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new PromessaCursoService(context);
        var dto = new PromessaCursoCreateDTO { CursoId = cursoId };

        Func<Task> acao = async () => await service.CriarPromessaAsync(dto, Guid.NewGuid());

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para adicionar promessas a este curso.");
    }

    [Fact]
    public async Task AlterarPromessaAsync_DeveLancarExcecao_QuandoNaoForDono() {
        var context = CriarContexto();
        var cursoId = Guid.NewGuid();
        var promessaId = Guid.NewGuid();

        context.Cursos.Add(CriarCursoFake(cursoId, Guid.NewGuid()));
        context.PromessaCursos.Add(new PromessaCurso { Id = promessaId, CursoId = cursoId, Descricao = "Original" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new PromessaCursoService(context);
        var dto = new PromessaCursoUpdateDTO { Descricao = "Nova" };

        Func<Task> acao = async () => await service.AlterarPromessaAsync(promessaId, Guid.NewGuid(), dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para alterar esta promessa.");
    }

    [Fact]
    public async Task AlterarPromessaAdminAsync_DeveAlterar_QuandoForAdmin() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var promessaId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.PromessaCursos.Add(new PromessaCurso { Id = promessaId, Descricao = "Original" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new PromessaCursoService(context);
        var dto = new PromessaCursoUpdateAdminDTO { Descricao = "Nova Admin", CumpridaNaAuditoria = true };

        var resultado = await service.AlterarPromessaAdminAsync(promessaId, adminId, dto);

        resultado.Descricao.Should().Be("Nova Admin");
        resultado.CumpridaNaAuditoria.Should().BeTrue();
    }

    [Fact]
    public async Task ExcluirPromessaAsync_DeveRetornarTrue_QuandoForDono() {
        var context = CriarContexto();
        var donoId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        var promessaId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(donoId, ERoleUsuario.Comum));
        context.Cursos.Add(CriarCursoFake(cursoId, donoId));
        context.PromessaCursos.Add(new PromessaCurso { Id = promessaId, CursoId = cursoId, Descricao = "Original" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new PromessaCursoService(context);
        var resultado = await service.ExcluirPromessaAsync(promessaId, donoId);

        resultado.Should().BeTrue();
        context.PromessaCursos.Count().Should().Be(0);
    }

    [Fact]
    public async Task ExcluirPromessaAsync_DeveRetornarFalse_QuandoNaoTiverPermissao() {
        var context = CriarContexto();
        var invasorId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        var promessaId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(invasorId, ERoleUsuario.Comum));
        context.Cursos.Add(CriarCursoFake(cursoId, Guid.NewGuid()));

        context.PromessaCursos.Add(new PromessaCurso {
            Id = promessaId,
            CursoId = cursoId,
            Descricao = "Promessa de Teste"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new PromessaCursoService(context);
        var resultado = await service.ExcluirPromessaAsync(promessaId, invasorId);

        resultado.Should().BeFalse();
        context.PromessaCursos.Count().Should().Be(1);
    }
}