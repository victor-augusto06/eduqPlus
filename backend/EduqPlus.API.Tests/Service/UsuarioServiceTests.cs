using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;
using EduqPlus.API.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduqPlus.API.Tests.Services;

public class UsuarioServiceTests {
    private EduqPlusContext CriarContexto() {
        var options = new DbContextOptionsBuilder<EduqPlusContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new EduqPlusContext(options);
    }

    [Fact]
    public async Task RegistrarAsync_DeveCriarComSucesso_QuandoEmailNaoExiste() {
        var context = CriarContexto();
        var service = new UsuarioService(context);
        var dto = new UsuarioCreateDTO {
            Nome = "Novo Usuario",
            Email = "novo@teste.com",
            Senha = "senha_forte"
        };

        var resultado = await service.RegistrarAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Email.Should().Be(dto.Email);
        resultado.Role.Should().Be(ERoleUsuario.Comum);
        context.Usuarios.Count().Should().Be(1);
    }

    [Fact]
    public async Task RegistrarAsync_DeveLancarExcecao_QuandoEmailJaExiste() {
        var context = CriarContexto();
        var email = "existente@teste.com";

        context.Usuarios.Add(new Usuario {
            Id = Guid.NewGuid(),
            Nome = "Existente",
            Email = email,
            SenhaHash = "hash"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new UsuarioService(context);
        var dto = new UsuarioCreateDTO { Nome = "Outro", Email = email, Senha = "123" };

        Func<Task> acao = async () => await service.RegistrarAsync(dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Este e-mail já está em uso.");
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarUsuario_QuandoCredenciaisForemValidas() {
        var context = CriarContexto();
        var email = "login@teste.com";
        var senhaLimpa = "minha_senha";
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaLimpa);

        context.Usuarios.Add(new Usuario {
            Id = Guid.NewGuid(),
            Nome = "User Login",
            Email = email,
            SenhaHash = senhaHash
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new UsuarioService(context);
        var resultado = await service.LoginAsync(email, senhaLimpa);

        resultado.Should().NotBeNull();
        resultado.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoginAsync_DeveLancarExcecao_QuandoSenhaEstiverIncorreta() {
        var context = CriarContexto();
        var email = "login@teste.com";
        var senhaLimpa = "senha_correta";
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaLimpa);

        context.Usuarios.Add(new Usuario {
            Id = Guid.NewGuid(),
            Nome = "User Login",
            Email = email,
            SenhaHash = senhaHash
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new UsuarioService(context);

        Func<Task> acao = async () => await service.LoginAsync(email, "senha_errada");

        await acao.Should().ThrowAsync<Exception>().WithMessage("E-mail ou senha inválidos.");
    }

    [Fact]
    public async Task AlterarPerfilAsync_DeveLancarExcecao_QuandoIdForDiferenteDoRequisitante() {
        var context = CriarContexto();
        var idAlvo = Guid.NewGuid();
        var idRequisitante = Guid.NewGuid();
        var service = new UsuarioService(context);
        var dto = new UsuarioUpdateDTO();

        Func<Task> acao = async () => await service.AlterarPerfilAsync(idAlvo, idRequisitante, dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para alterar o perfil de outro usuário.");
    }

    [Fact]
    public async Task AlterarRoleAdminAsync_DeveLancarExcecao_QuandoNaoForAdmin() {
        var context = CriarContexto();
        var adminFakeId = Guid.NewGuid();
        var alvoId = Guid.NewGuid();

        context.Usuarios.Add(new Usuario { Id = adminFakeId, Nome = "Comum", Email = "a@a.com", SenhaHash = "h", Role = ERoleUsuario.Comum });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new UsuarioService(context);

        Func<Task> acao = async () => await service.AlterarRoleAdminAsync(alvoId, adminFakeId, ERoleUsuario.Admin);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Apenas administradores podem alterar permissões.");
    }

    [Fact]
    public async Task ExcluirUsuarioAsync_DeveRetornarTrue_QuandoForOProprioUsuario() {
        var context = CriarContexto();
        var usuarioId = Guid.NewGuid();

        context.Usuarios.Add(new Usuario { Id = usuarioId, Nome = "Eu", Email = "eu@eu.com", SenhaHash = "h" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new UsuarioService(context);
        var resultado = await service.ExcluirUsuarioAsync(usuarioId, usuarioId);

        resultado.Should().BeTrue();
        context.Usuarios.Count().Should().Be(0);
    }
}