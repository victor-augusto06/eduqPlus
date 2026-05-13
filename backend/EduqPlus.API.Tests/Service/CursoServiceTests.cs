using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;
using EduqPlus.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduqPlus.API.Tests.Services;

public class CursoServiceTests {
    private EduqPlusContext CriarContexto() {
        var options = new DbContextOptionsBuilder<EduqPlusContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new EduqPlusContext(options);
    }

    private Usuario CriarUsuarioFake(Guid id, ERoleUsuario role) {
        return new Usuario {
            Id = id,
            Nome = "Usuario Teste",
            Email = $"teste{id}@eduq.com",
            SenhaHash = "hash_senha",
            Role = role
        };
    }

    [Fact]
    public async Task CriarCursoAsync_DeveCriarComSucesso_QuandoDadosValidos() {
        var context = CriarContexto();
        var service = new CursoService(context);
        var dto = new CursoCreateDTO {
            Titulo = "Curso de .NET",
            DescricaoOriginal = "Descricao",
            UsuarioId = Guid.NewGuid(),
            PromessaCursos = new List<PromessaCursoCreateDTO> { new() { Descricao = "Promessa 1" } }
        };

        var resultado = await service.CriarCursoAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Titulo.Should().Be(dto.Titulo);
        context.Cursos.Count().Should().Be(1);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoCursoNaoExiste() {
        var context = CriarContexto();
        var service = new CursoService(context);

        Func<Task> acao = async () => await service.ObterPorIdAsync(Guid.NewGuid());

        await acao.Should().ThrowAsync<Exception>().WithMessage("Curso não encontrado.");
    }

    [Fact]
    public async Task AlterarCursoAsync_DeveLancarExcecao_QuandoUsuarioNaoForDono() {
        var context = CriarContexto();
        var donoId = Guid.NewGuid();
        var invasorId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        context.Cursos.Add(new Curso { Id = cursoId, Titulo = "Original", UsuarioId = donoId });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new CursoService(context);
        var dto = new CursoUpdateDTO { Titulo = "Hackeado" };

        Func<Task> acao = async () => await service.AlterarCursoAsync(cursoId, invasorId, dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para alterar este curso.");
    }

    [Fact]
    public async Task AlterarCursoAdminAsync_DeveAlterar_QuandoUsuarioEhAdmin() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Cursos.Add(new Curso { Id = cursoId, Titulo = "Original", UsuarioId = Guid.NewGuid() });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new CursoService(context);
        var dto = new CursoUpdateAdminDTO { Titulo = "Alterado por Admin", StatusAuditoria = EStatusAuditoria.Aprovado };

        var resultado = await service.AlterarCursoAdminAsync(cursoId, adminId, dto);

        resultado.Titulo.Should().Be("Alterado por Admin");
        resultado.StatusAuditoria.Should().Be(EStatusAuditoria.Aprovado);
    }

    [Fact]
    public async Task ExcluirCursoAsync_DeveRetornarTrue_QuandoUsuarioEhDono() {
        var context = CriarContexto();
        var donoId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(donoId, ERoleUsuario.Comum));
        context.Cursos.Add(new Curso { Id = cursoId, Titulo = "Curso Teste", UsuarioId = donoId });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new CursoService(context);
        var resultado = await service.ExcluirCursoAsync(cursoId, donoId);

        resultado.Should().BeTrue();
        context.Cursos.Count().Should().Be(0);
    }

    [Fact]
    public async Task ExcluirCursoAsync_DeveRetornarTrue_QuandoUsuarioEhAdminMasNaoEhDono() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Cursos.Add(new Curso { Id = cursoId, Titulo = "Curso Teste", UsuarioId = Guid.NewGuid() });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new CursoService(context);
        var resultado = await service.ExcluirCursoAsync(cursoId, adminId);

        resultado.Should().BeTrue();
    }
}