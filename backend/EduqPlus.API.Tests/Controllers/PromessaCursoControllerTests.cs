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

public class PromessaCursoControllerTests {
    private readonly Mock<IPromessaCursoService> _serviceMock;
    private readonly PromessaCursoController _controller;

    public PromessaCursoControllerTests() {
        _serviceMock = new Mock<IPromessaCursoService>();
        _controller = new PromessaCursoController(_serviceMock.Object);
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
        var dto = new PromessaCursoCreateDTO();
        var response = new PromessaCursoResponseDTO { Id = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarPromessaAsync(dto, usuarioId)).ReturnsAsync(response);

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Criar_DeveRetornarBadRequest_QuandoServiceLancarExcecao() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);
        var dto = new PromessaCursoCreateDTO();

        _serviceMock.Setup(s => s.CriarPromessaAsync(dto, usuarioId)).ThrowsAsync(new Exception("Erro"));

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarOk_QuandoSucesso() {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(new PromessaCursoResponseDTO());

        var resultado = await _controller.ObterPorId(id);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObterPorCurso_DeveRetornarNotFound_QuandoServiceLancarExcecao() {
        var cursoId = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterPorCursoAsync(cursoId)).ThrowsAsync(new Exception("Erro"));

        var resultado = await _controller.ObterPorCurso(cursoId);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AtualizarAdmin_DeveRetornarOk_QuandoSucesso() {
        var adminId = Guid.NewGuid();
        var id = Guid.NewGuid();
        MockUsuarioAutenticado(adminId);
        var dto = new PromessaCursoUpdateAdminDTO();

        _serviceMock.Setup(s => s.AlterarPromessaAdminAsync(id, adminId, dto))
            .ReturnsAsync(new PromessaCursoResponseDTO());

        var resultado = await _controller.AtualizarAdmin(id, dto);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarNoContent_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        var id = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirPromessaAsync(id, usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.Excluir(id);

        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarBadRequest_QuandoServiceRetornarFalse() {
        var usuarioId = Guid.NewGuid();
        var id = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirPromessaAsync(id, usuarioId)).ReturnsAsync(false);

        var resultado = await _controller.Excluir(id);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }
}