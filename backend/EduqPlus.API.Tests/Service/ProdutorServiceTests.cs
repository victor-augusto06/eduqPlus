using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;
using EduqPlus.API.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduqPlus.API.Tests.Services;

public class ProdutorServiceTests {
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
            SenhaHash = "hash",
            Role = role
        };
    }

    [Fact]
    public async Task CriarProdutorAsync_DeveCriarComSucesso() {
        var context = CriarContexto();
        var service = new ProdutorService(context);
        var dto = new ProdutorCreateDTO {
            UsuarioId = Guid.NewGuid(),
            Nome = "Produtor Teste",
            NichoPrincipal = "Tecnologia",
            LinksSociais = "link.com"
        };

        var resultado = await service.CriarProdutorAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(dto.Nome);
        context.Produtors.Count().Should().Be(1);
    }

    [Fact]
    public async Task AlterarProdutorAsync_DeveLancarExcecao_QuandoUsuarioNaoForDono() {
        var context = CriarContexto();
        var donoId = Guid.NewGuid();
        var invasorId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();

        context.Produtors.Add(new Produtor {
            Id = produtorId,
            UsuarioId = donoId,
            Nome = "Original"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new ProdutorService(context);
        var dto = new ProdutorUpdateDTO { Nome = "Alterado" };

        Func<Task> acao = async () => await service.AlterarProdutorAsync(produtorId, invasorId, dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para alterar este produtor.");
    }

    [Fact]
    public async Task AlterarProdutorAdminAsync_DeveLancarExcecao_QuandoNaoForAdmin() {
        var context = CriarContexto();
        var usuarioId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(usuarioId, ERoleUsuario.Comum));
        context.Produtors.Add(new Produtor {
            Id = produtorId,
            UsuarioId = Guid.NewGuid(),
            Nome = "Original"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new ProdutorService(context);
        var dto = new ProdutorUpdateDTO { Nome = "Alterado" };

        Func<Task> acao = async () => await service.AlterarProdutorAdminAsync(produtorId, usuarioId, dto);

        await acao.Should().ThrowAsync<Exception>().WithMessage("Você não tem permissão para alterar este produtor.");
    }

    [Fact]
    public async Task AlterarProdutorAdminAsync_DeveAtualizar_QuandoUsuarioForAdmin() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));
        context.Produtors.Add(new Produtor {
            Id = produtorId,
            UsuarioId = Guid.NewGuid(),
            Nome = "Original"
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new ProdutorService(context);
        var dto = new ProdutorUpdateDTO { Nome = "Alterado Admin" };

        var resultado = await service.AlterarProdutorAdminAsync(produtorId, adminId, dto);

        resultado.Nome.Should().Be("Alterado Admin");
    }

    [Fact]
    public async Task ExcluirProdutorAsync_DeveRetornarFalse_QuandoNaoTiverPermissao() {
        var context = CriarContexto();
        var donoId = Guid.NewGuid();
        var invasorId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(invasorId, ERoleUsuario.Comum));

        context.Produtors.Add(new Produtor { Id = produtorId, UsuarioId = donoId, Nome = "Produtor Original" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new ProdutorService(context);
        var resultado = await service.ExcluirProdutorAsync(produtorId, invasorId);

        resultado.Should().BeFalse();
        context.Produtors.Count().Should().Be(1);
    }

    [Fact]
    public async Task ExcluirProdutorAsync_DeveRetornarTrue_QuandoForAdmin() {
        var context = CriarContexto();
        var adminId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();

        context.Usuarios.Add(CriarUsuarioFake(adminId, ERoleUsuario.Admin));

        context.Produtors.Add(new Produtor { Id = produtorId, UsuarioId = Guid.NewGuid(), Nome = "Produtor Original" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new ProdutorService(context);
        var resultado = await service.ExcluirProdutorAsync(produtorId, adminId);

        resultado.Should().BeTrue();
        context.Produtors.Count().Should().Be(0);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoNaoEncontrado() {
        var context = CriarContexto();
        var service = new ProdutorService(context);

        Func<Task> acao = async () => await service.ObterPorIdAsync(Guid.NewGuid());

        await acao.Should().ThrowAsync<Exception>().WithMessage("Produtor não foi encontrado.");
    }

    [Fact]
    public async Task ObterProdutoresCursoAsync_DeveRetornarProdutor_QuandoAssociadoAoCurso() {
        var context = CriarContexto();
        var produtorId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        var produtor = new Produtor { Id = produtorId, UsuarioId = Guid.NewGuid(), Nome = "Produtor 1" };
        var curso = new Curso { Id = cursoId, Titulo = "Curso Teste", UsuarioId = Guid.NewGuid(), ProdutorId = produtorId };

        context.Produtors.Add(produtor);
        context.Cursos.Add(curso);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new ProdutorService(context);
        var resultado = await service.ObterProdutoresCursoAsync(cursoId);

        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(produtorId);
    }

    [Fact]
    public async Task ObterProdutoresCursoAsync_DeveLancarExcecao_QuandoNaoEncontrar() {
        var context = CriarContexto();
        var service = new ProdutorService(context);

        Func<Task> acao = async () => await service.ObterProdutoresCursoAsync(Guid.NewGuid());

        await acao.Should().ThrowAsync<Exception>().WithMessage("Nenhum produtor foi encontrado para esse curso.");
    }
}