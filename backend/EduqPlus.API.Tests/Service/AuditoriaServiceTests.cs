using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;
using EduqPlus.API.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduqPlus.API.Tests.Services;

public class AuditoriaServiceTests {
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

    private Curso CriarCursoFake(Guid id, Guid usuarioId) {
        return new Curso {
            Id = id,
            UsuarioId = usuarioId,
            Titulo = "Curso Teste"
        };
    }

    [Fact]
    public async Task CriarAuditoriaAsync_DeveLancarExcecao_QuandoAuditorNaoForEncontrado() {
        var context = CriarContexto();
        var service = new AuditoriaService(context);
        var dto = new AuditoriaCreateDTO { AuditorId = Guid.NewGuid() };

        Func<Task> acao = async () => await service.CriarAuditoriaAsync(dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Apenas administradores podem realizar auditorias.");
    }

    [Fact]
    public async Task CriarAuditoriaAsync_DeveLancarExcecao_QuandoUsuarioNaoForAdmin() {
        var context = CriarContexto();
        var usuarioId = Guid.NewGuid();
        context.Usuarios.Add(CriarUsuarioFake(usuarioId, ERoleUsuario.Comum));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AuditoriaService(context);
        var dto = new AuditoriaCreateDTO { AuditorId = usuarioId };

        Func<Task> acao = async () => await service.CriarAuditoriaAsync(dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Apenas administradores podem realizar auditorias.");
    }

    [Fact]
    public async Task CriarAuditoriaAsync_DeveCriarComSucesso_QuandoAuditorEhAdmin() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Cursos.Add(CriarCursoFake(cursoId, adminId));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AuditoriaService(context);
        var dto = new AuditoriaCreateDTO {
            AuditorId = adminId,
            CursoId = cursoId,
            Resultado = EStatusAuditoria.Aprovado,
            ObservacaoAuditor = "Tudo certo"
        };

        var resultado = await service.CriarAuditoriaAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Resultado.Should().Be(EStatusAuditoria.Aprovado);
        context.Auditoria.Count().Should().Be(1);
    }

    [Fact]
    public async Task AtualizarAuditoriaAsync_DeveLancarExcecao_QuandoAuditoriaNaoExiste() {
        var context = CriarContexto();
        var service = new AuditoriaService(context);
        var dto = new AuditoriaUpdateDTO();

        Func<Task> acao = async () => await service.AtualizarAuditoriaAsync(Guid.NewGuid(), Guid.NewGuid(), dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Auditoria não encontrada.");
    }

    [Fact]
    public async Task AtualizarAuditoriaAsync_DeveAtualizarComSucesso() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        var auditoriaId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Cursos.Add(CriarCursoFake(cursoId, adminId));
        context.Auditoria.Add(new Auditoria {
            Id = auditoriaId,
            AuditorId = adminId,
            CursoId = cursoId,
            Resultado = EStatusAuditoria.EmAnalise,
            CriterioAnalisado = "Qualidade do áudio"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AuditoriaService(context);
        var dto = new AuditoriaUpdateDTO {
            Resultado = EStatusAuditoria.Aprovado,
            ObservacaoAuditor = "Nova observação"
        };

        var resultado = await service.AtualizarAuditoriaAsync(auditoriaId, adminId, dto);

        resultado.Resultado.Should().Be(EStatusAuditoria.Aprovado);
        resultado.ObservacaoAuditor.Should().Be("Nova observação");
    }

    [Fact]
    public async Task ObterAuditoriasPendentesAsync_DeveRetornarApenasEmAnalise() {
        var context = CriarContexto();
        var cursoId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Cursos.Add(CriarCursoFake(cursoId, adminId));

        context.Auditoria.AddRange(
            new Auditoria { Id = Guid.NewGuid(), CursoId = cursoId, AuditorId = adminId, Resultado = EStatusAuditoria.EmAnalise, CriterioAnalisado = "Vídeo" },
            new Auditoria { Id = Guid.NewGuid(), CursoId = cursoId, AuditorId = adminId, Resultado = EStatusAuditoria.Aprovado, CriterioAnalisado = "Áudio" }
        );
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AuditoriaService(context);
        var resultado = await service.ObterAuditoriasPendentesAsync();

        resultado.Should().HaveCount(1);
        resultado.First().Resultado.Should().Be(EStatusAuditoria.EmAnalise);
    }

    [Fact]
    public async Task ObterAuditoriasConcluidasAsync_DeveRetornarDiferentesDeEmAnalise() {
        var context = CriarContexto();
        var cursoId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Cursos.Add(CriarCursoFake(cursoId, adminId));

        context.Auditoria.AddRange(
            new Auditoria { Id = Guid.NewGuid(), CursoId = cursoId, AuditorId = adminId, Resultado = EStatusAuditoria.EmAnalise, CriterioAnalisado = "Didática" },
            new Auditoria { Id = Guid.NewGuid(), CursoId = cursoId, AuditorId = adminId, Resultado = EStatusAuditoria.Reprovado, CriterioAnalisado = "Conteúdo" }
        );
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new AuditoriaService(context);
        var resultado = await service.ObterAuditoriasConcluidasAsync(null);

        resultado.Should().HaveCount(1);
        resultado.First().Resultado.Should().Be(EStatusAuditoria.Reprovado);
    }
}