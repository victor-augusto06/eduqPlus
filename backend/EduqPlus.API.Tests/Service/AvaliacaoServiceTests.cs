using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using EduqPlus.API.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace EduqPlus.API.Tests.Services;

public class AvaliacaoServiceTests {
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

    private Curso CriarCursoFake() {
        return new Curso {
            Id = Guid.NewGuid(),
            Titulo = "Curso Teste",
            DescricaoOriginal = "Descricao"
        };
    }

    [Fact]
    public async Task CriarAvaliacaoAsync_DeveCriarComSucesso() {
        var context = CriarContexto();
        var service = new AvaliacaoService(context, new Mock<ICursoService>().Object, new Mock<IWebHostEnvironment>().Object);
        var curso = CriarCursoFake();
        context.Cursos.Add(curso);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new AvaliacaoCreateDTO {
            CursoId = curso.Id,
            UsuarioId = Guid.NewGuid(),
            NotaEntrega = 5,
            NotaSuporte = 4,
            Comentario = "Ótimo curso"
        };

        var resultado = await service.CriarAvaliacaoAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Comentario.Should().Be(dto.Comentario);
        context.Avaliacoes.Count().Should().Be(1);
    }

    [Fact]
    public async Task AtualizarAvaliacaoAsync_DeveLancarExcecao_QuandoUsuarioNaoForDono() {
        var context = CriarContexto();
        var donoId = Guid.NewGuid();
        var invasorId = Guid.NewGuid();
        var avaliacaoId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        context.Cursos.Add(new Curso { Id = cursoId, Titulo = "Curso Teste", UsuarioId = Guid.NewGuid() });

        context.Avaliacoes.Add(new Avaliacao {
            Id = avaliacaoId,
            UsuarioId = donoId,
            CursoId = cursoId
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AvaliacaoService(context, new Mock<ICursoService>().Object, new Mock<IWebHostEnvironment>().Object);
        var dto = new AvaliacaoUpdateDTO { Comentario = "Texto alterado" };

        Func<Task> acao = async () => await service.AtualizarAvaliacaoAsync(avaliacaoId, invasorId, dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para alterar esta avaliação.");
    }

    [Fact]
    public async Task ValidarComprovanteAsync_DeveAprovar_QuandoUsuarioForAdmin() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var avaliacaoId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Avaliacoes.Add(new Avaliacao { Id = avaliacaoId, UsuarioId = Guid.NewGuid(), CursoId = Guid.NewGuid() });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AvaliacaoService(context, new Mock<ICursoService>().Object, new Mock<IWebHostEnvironment>().Object);
        var resultado = await service.ValidarComprovanteAsync(avaliacaoId, adminId, EStatusComprovante.Aprovado);

        resultado.StatusComprovante.Should().Be(EStatusComprovante.Aprovado);
        resultado.IsCompraVerificada.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarComprovanteAsync_DeveLancarExcecao_QuandoUsuarioNaoForAdmin() {
        var context = CriarContexto();
        var comumId = Guid.NewGuid();
        var avaliacaoId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(comumId, ERoleUsuario.Comum));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AvaliacaoService(context, new Mock<ICursoService>().Object, new Mock<IWebHostEnvironment>().Object);
        Func<Task> acao = async () => await service.ValidarComprovanteAsync(avaliacaoId, comumId, EStatusComprovante.Aprovado);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Apenas administradores podem validar comprovantes.");
    }

    [Fact]
    public async Task ExcluirAvaliacaoAsync_DeveRetornarFalse_QuandoAvaliacaoNaoExiste() {
        var context = CriarContexto();
        var service = new AvaliacaoService(context, new Mock<ICursoService>().Object, new Mock<IWebHostEnvironment>().Object);

        var resultado = await service.ExcluirAvaliacaoAsync(Guid.NewGuid(), Guid.NewGuid());

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObterAvaliacoesValidadasAsync_DeveRetornarApenasVerificadas() {
        var context = CriarContexto();
        var cursoId = Guid.NewGuid();

        context.Avaliacoes.AddRange(
            new Avaliacao { Id = Guid.NewGuid(), CursoId = cursoId, IsCompraVerificada = true, UsuarioId = Guid.NewGuid() },
            new Avaliacao { Id = Guid.NewGuid(), CursoId = cursoId, IsCompraVerificada = false, UsuarioId = Guid.NewGuid() }
        );
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AvaliacaoService(context, new Mock<ICursoService>().Object, new Mock<IWebHostEnvironment>().Object);
        var resultado = await service.ObterAvaliacoesValidadasAsync(cursoId);

        resultado.Should().HaveCount(1);
        resultado.All(a => a.IsCompraVerificada).Should().BeTrue();
    }
}