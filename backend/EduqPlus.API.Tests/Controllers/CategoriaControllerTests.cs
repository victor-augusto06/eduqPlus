using System.Security.Claims;
using EduqPlus.API.Controllers;
using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduqPlus.API.Tests.Controllers {
    public class CategoriaControllerTests {
        private readonly Mock<ICategoriaService> _categoriaServiceMock;
        private readonly CategoriaController _controller;
        private readonly Guid _usuarioLogadoId;

        public CategoriaControllerTests() {
            _categoriaServiceMock = new Mock<ICategoriaService>();
            _controller = new CategoriaController(_categoriaServiceMock.Object);
            _usuarioLogadoId = Guid.NewGuid();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, _usuarioLogadoId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext() {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarOk_QuandoCategoriaExiste() {
            var id = Guid.NewGuid();
            _categoriaServiceMock.Setup(s => s.ObterPorIdAsync(id))
                .ReturnsAsync(new CategoriaResponseDTO { Id = id, Nome = "Tecnologia" });

            var resultado = await _controller.ObterPorId(id);

            resultado.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoEncontrada() {
            _categoriaServiceMock.Setup(s => s.ObterPorIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Nenhuma categoria foi encontrada"));

            var resultado = await _controller.ObterPorId(Guid.NewGuid());

            resultado.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Criar_DeveRetornarCreated_QuandoSucesso() {
            var dto = new CategoriaCreateDTO { Nome = "Nova" };
            _categoriaServiceMock.Setup(s => s.CriarCategoriaAsync(dto))
                .ReturnsAsync(new CategoriaResponseDTO { Id = Guid.NewGuid(), Nome = "Nova" });

            var resultado = await _controller.Criar(dto);

            resultado.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Excluir_DeveRetornarNoContent_QuandoSucesso() {
            _categoriaServiceMock.Setup(s => s.ExcluirCategoriaAsync(It.IsAny<Guid>(), _usuarioLogadoId))
                .ReturnsAsync(true);

            var resultado = await _controller.Excluir(Guid.NewGuid());

            resultado.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Atualizar_DeveRetornarBadRequest_QuandoOcorrerErro() {
            var dto = new CategoriaUpdateDTO { Nome = "Atualizada" };
            _categoriaServiceMock.Setup(s => s.AtualizarCategoriasAsync(It.IsAny<Guid>(), _usuarioLogadoId, dto))
                .ThrowsAsync(new Exception("Erro de validação"));

            var resultado = await _controller.Atualizar(Guid.NewGuid(), dto);

            resultado.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async Task ObterTodas_DeveRetornarOk_ComLista() {
            var lista = new List<CategoriaResponseDTO> { new CategoriaResponseDTO { Nome = "TI" } };
            _categoriaServiceMock.Setup(s => s.ObterTodosAsync()).ReturnsAsync(lista);

            var resultado = await _controller.ObterTodas();

            resultado.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Atualizar_DeveRetornarOk_QuandoSucesso() {
            var dto = new CategoriaUpdateDTO { Nome = "Nova Categoria" };
            _categoriaServiceMock.Setup(s => s.AtualizarCategoriasAsync(It.IsAny<Guid>(), _usuarioLogadoId, dto))
                .ReturnsAsync(new CategoriaResponseDTO { Id = Guid.NewGuid(), Nome = "Nova Categoria" });

            var resultado = await _controller.Atualizar(Guid.NewGuid(), dto);

            resultado.Should().BeOfType<OkObjectResult>();
        }
    }
}