using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;
using EduqPlus.API.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduqPlus.API.Tests.Services;

public class DenunciaServiceTests {
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

    [Fact]
    public async Task CriarDenunciaAsync_DeveCriarComSucesso() {
        var context = CriarContexto();
        var service = new DenunciaService(context);
        var dto = new DenunciaCreateDTO {
            CursoId = Guid.NewGuid(),
            Categoria = ETipoDenuncia.FraudeOuGolpe,
            RelatoDetalhado = "Descrição detalhada"
        };
        var usuarioId = Guid.NewGuid();

        var resultado = await service.CriarDenunciaAsync(dto, usuarioId);

        resultado.Should().NotBeNull();
        resultado.Status.Should().Be(EStatusDenuncia.EmAnalise);
        context.Denuncia.Count().Should().Be(1);
    }

    [Fact]
    public async Task AlterarDenunciaAsync_DeveLancarExcecao_QuandoNaoForDono() {
        var context = CriarContexto();
        var donoId = Guid.NewGuid();
        var invasorId = Guid.NewGuid();
        var denunciaId = Guid.NewGuid();

        context.Denuncia.Add(new Denuncia {
            Id = denunciaId,
            UsuarioId = donoId,
            CursoId = Guid.NewGuid(),
            Categoria = ETipoDenuncia.Spam,
            RelatoDetalhado = "Texto"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new DenunciaService(context);
        var dto = new DenunciaUpdateDTO { Categoria = ETipoDenuncia.DiscursoDeOdio };

        Func<Task> acao = async () => await service.AlterarDenunciaAsync(denunciaId, invasorId, dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para alterar esta denúncia.");
    }

    [Fact]
    public async Task AlterarDenunciaAdminAsync_DeveLancarExcecao_QuandoNaoForAdmin() {
        var context = CriarContexto();
        var usuarioId = Guid.NewGuid();
        var denunciaId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(usuarioId, ERoleUsuario.Comum));
        context.Denuncia.Add(new Denuncia {
            Id = denunciaId,
            UsuarioId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            Categoria = ETipoDenuncia.Spam,
            RelatoDetalhado = "Texto"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new DenunciaService(context);
        var dto = new DenunciaUpdateAdminDTO { Status = EStatusDenuncia.Aceita };

        Func<Task> acao = async () => await service.AlterarDenunciaAdminAsync(denunciaId, usuarioId, dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para alterar esta denúncia.");
    }

    [Fact]
    public async Task ExcluirDenunciaAsync_DeveRetornarTrue_QuandoUsuarioForAdmin() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var denunciaId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Denuncia.Add(new Denuncia {
            Id = denunciaId,
            UsuarioId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            Categoria = ETipoDenuncia.Spam,
            RelatoDetalhado = "Texto"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new DenunciaService(context);

        var resultado = await service.ExcluirDenunciaAsync(denunciaId, adminId);

        resultado.Should().BeTrue();
        context.Denuncia.Count().Should().Be(0);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoNaoExistir() {
        var context = CriarContexto();
        var service = new DenunciaService(context);

        Func<Task> acao = async () => await service.ObterPorIdAsync(Guid.NewGuid());

        await acao.Should().ThrowAsync<Exception>().WithMessage("Denúncia não foi encontrada.");
    }
}