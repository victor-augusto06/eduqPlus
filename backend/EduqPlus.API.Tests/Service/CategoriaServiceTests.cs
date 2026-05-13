using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;
using EduqPlus.API.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduqPlus.API.Tests.Services {
    public class CategoriaServiceTests {
        private async Task<EduqPlusContext> ObterContextoEmMemoria() {
            var options = new DbContextOptionsBuilder<EduqPlusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new EduqPlusContext(options);
            return context;
        }

        [Fact]
        public async Task CriarCategoriaAsync_DeveCriar_QuandoNomeNaoExiste() {
            var context = await ObterContextoEmMemoria();
            var service = new CategoriaService(context);
            var dto = new CategoriaCreateDTO { Nome = "Programação" };

            var resultado = await service.CriarCategoriaAsync(dto);

            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Programação");
            context.Categorias.Should().HaveCount(1);
        }

        [Fact]
        public async Task CriarCategoriaAsync_DeveLancarExcecao_QuandoNomeJaExiste() {
            var context = await ObterContextoEmMemoria();
            context.Categorias.Add(new Categoria { Id = Guid.NewGuid(), Nome = "Design" });

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new CategoriaService(context);
            var dto = new CategoriaCreateDTO { Nome = "design" };

            Func<Task> acao = async () => await service.CriarCategoriaAsync(dto);

            await acao.Should().ThrowAsync<Exception>().WithMessage("Já existe uma categoria com este nome.");
        }

        [Fact]
        public async Task ExcluirCategoriaAsync_DeveLancarExcecao_QuandoUsuarioNaoEhAdmin() {
            var context = await ObterContextoEmMemoria();
            var usuarioId = Guid.NewGuid();
            context.Usuarios.Add(new Usuario {
                Id = usuarioId,
                Role = ERoleUsuario.Comum,
                Nome = "Usuario Teste",
                Email = "teste@eduqplus.com",
                SenhaHash = "senha_dummy"
            });

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new CategoriaService(context);

            Func<Task> acao = async () => await service.ExcluirCategoriaAsync(Guid.NewGuid(), usuarioId);

            await acao.Should().ThrowAsync<Exception>().WithMessage("Apenas administradores podem excluir categorias.");
        }

        [Fact]
        public async Task ExcluirCategoriaAsync_DeveLancarExcecao_QuandoExistemCursosVinculados() {
            var context = await ObterContextoEmMemoria();
            var usuarioId = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();

            context.Usuarios.Add(new Usuario {
                Id = usuarioId,
                Role = ERoleUsuario.Admin,
                Nome = "Admin Teste",
                Email = "admin_excluir@eduqplus.com",
                SenhaHash = "senha_dummy"
            });

            var categoria = new Categoria { Id = categoriaId, Nome = "Linguagens" };
            categoria.Cursos = new List<Curso> { new Curso { Id = Guid.NewGuid(), Titulo = "C#" } };
            context.Categorias.Add(categoria);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new CategoriaService(context);

            Func<Task> acao = async () => await service.ExcluirCategoriaAsync(categoriaId, usuarioId);

            await acao.Should().ThrowAsync<Exception>().WithMessage("Não é possível excluir esta categoria pois existem cursos vinculados a ela.");
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoIdNaoExiste() {
            var context = await ObterContextoEmMemoria();
            var service = new CategoriaService(context);

            Func<Task> acao = async () => await service.ObterPorIdAsync(Guid.NewGuid());

            await acao.Should().ThrowAsync<Exception>().WithMessage("Nenhuma categoria foi encontrada");
        }

        [Fact]
        public async Task AtualizarCategoriasAsync_DeveAlterarNome_QuandoSucesso() {
            var context = await ObterContextoEmMemoria();
            var id = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            context.Categorias.Add(new Categoria { Id = id, Nome = "Antigo" });
            context.Usuarios.Add(new Usuario {
                Id = adminId,
                Role = ERoleUsuario.Admin,
                Nome = "Admin Teste",
                Email = "admin@eduqplus.com",
                SenhaHash = "senha_dummy"
            });

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var service = new CategoriaService(context);
            var dto = new CategoriaUpdateDTO { Nome = "Novo Nome" };

            var resultado = await service.AtualizarCategoriasAsync(id, adminId, dto);

            resultado.Nome.Should().Be("Novo Nome");
            var noBanco = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);

            noBanco.Should().NotBeNull();

            noBanco.Nome.Should().Be("Novo Nome");
        }
    }
}