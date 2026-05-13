using System.Security.Claims;
using EduqPlus.API.Controllers;
using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduqPlus.API.Tests.Controllers;

public class ProdutorControllerTests {
    private readonly Mock<IProdutorService> _serviceMock;
    private readonly ProdutorController _controller;

    public ProdutorControllerTests() {
        _serviceMock = new Mock<IProdutorService>();
        _controller = new ProdutorController(_serviceMock.Object);
    }

    private void MockUsuarioAutenticado(Guid usuarioId) {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, usuarioId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Criar_DeveRetornarCreated_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);
        var dto = new ProdutorCreateDTO();
        var response = new ProdutorResponseDTO { Id = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarProdutorAsync(It.IsAny<ProdutorCreateDTO>()))
            .ReturnsAsync(response);

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Criar_DeveRetornarBadRequest_QuandoLancarExcecao() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);
        var dto = new ProdutorCreateDTO();

        _serviceMock.Setup(s => s.CriarProdutorAsync(It.IsAny<ProdutorCreateDTO>()))
            .ThrowsAsync(new Exception("Erro de validação"));

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarOk() {
        _serviceMock.Setup(s => s.ObterTodosAsync())
            .ReturnsAsync(new List<ProdutorResponseDTO>());

        var resultado = await _controller.ObterTodos();

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoServiceLancarExcecao() {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ThrowsAsync(new Exception("Não encontrado"));

        var resultado = await _controller.ObterPorId(id);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ObterProdutoresCurso_DeveRetornarOk_QuandoSucesso() {
        var cursoId = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterProdutoresCursoAsync(cursoId))
            .ReturnsAsync(new ProdutorResponseDTO());

        var resultado = await _controller.ObterProdutoresCurso(cursoId);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Atualizar_DeveRetornarBadRequest_QuandoErroDePermissao() {
        var usuarioId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);
        var dto = new ProdutorUpdateDTO();

        _serviceMock.Setup(s => s.AlterarProdutorAsync(produtorId, usuarioId, dto))
            .ThrowsAsync(new Exception("Sem permissão"));

        var resultado = await _controller.Atualizar(produtorId, dto);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AtualizarAdmin_DeveRetornarOk_QuandoSucesso() {
        var adminId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();
        MockUsuarioAutenticado(adminId);
        var dto = new ProdutorUpdateDTO();

        _serviceMock.Setup(s => s.AlterarProdutorAdminAsync(produtorId, adminId, dto))
            .ReturnsAsync(new ProdutorResponseDTO());

        var resultado = await _controller.AtualizarAdmin(produtorId, dto);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarNoContent_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirProdutorAsync(produtorId, usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.Excluir(produtorId);

        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarBadRequest_QuandoRetornarFalse() {
        var usuarioId = Guid.NewGuid();
        var produtorId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirProdutorAsync(produtorId, usuarioId)).ReturnsAsync(false);

        var resultado = await _controller.Excluir(produtorId);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }
}