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

public class CursoControllerTests {
    private readonly Mock<ICursoService> _serviceMock;
    private readonly Mock<IRecommendationService> _recServiceMock;
    private readonly CursoController _controller;

    public CursoControllerTests() {
        _serviceMock = new Mock<ICursoService>();
        _recServiceMock = new Mock<IRecommendationService>();
        _controller = new CursoController(_serviceMock.Object, _recServiceMock.Object);
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
    public async Task ObterPorId_DeveRetornarOk_QuandoCursoExiste() {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ReturnsAsync(new CursoResponseDTO { Id = id, Titulo = "Teste" });

        var resultado = await _controller.ObterPorId(id);

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
    public async Task Criar_DeveRetornarCreated_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);
        var dto = new CursoCreateDTO { Titulo = "Novo Curso" };
        var response = new CursoResponseDTO { Id = Guid.NewGuid(), Titulo = "Novo Curso" };

        _serviceMock.Setup(s => s.CriarCursoAsync(It.IsAny<CursoCreateDTO>())).ReturnsAsync(response);

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarNoContent_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirCursoAsync(cursoId, usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.Excluir(cursoId);

        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarBadRequest_QuandoServiceRetornarFalse() {
        var usuarioId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirCursoAsync(cursoId, usuarioId)).ReturnsAsync(false);

        var resultado = await _controller.Excluir(cursoId);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ObterRecomendacoes_DeveLimitarEmTresCursosPorPadrao() {
        var id = Guid.NewGuid();
        var recIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _recServiceMock.Setup(r => r.ObterCursosRecomendadosAsync(id, 3)).ReturnsAsync(recIds);
        foreach (var recId in recIds) {
            _serviceMock.Setup(s => s.ObterPorIdAsync(recId)).ReturnsAsync(new CursoResponseDTO { Id = recId, Titulo = "Rec" });
        }

        var resultado = await _controller.ObterRecomendacoes(id);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var cursos = okResult.Value as List<CursoResponseDTO>;
        cursos.Should().NotBeNull();
        cursos.Should().HaveCount(3);
    }

}
