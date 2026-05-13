using System.Security.Claims;
using EduqPlus.API.Controllers;
using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduqPlus.API.Tests.Controllers;

public class AuditoriaControllerTests {
    private readonly Mock<IAuditoriaService> _serviceMock;
    private readonly AuditoriaController _controller;

    public AuditoriaControllerTests() {
        _serviceMock = new Mock<IAuditoriaService>();
        _controller = new AuditoriaController(_serviceMock.Object);
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
    public async Task ObterPorCurso_DeveRetornarOk() {
        var cursoId = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterAuditoriasCursoAsync(cursoId))
            .ReturnsAsync(new List<AuditoriaResponseDTO>());

        var resultado = await _controller.ObterPorCurso(cursoId);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObterPorCurso_DeveRetornarBadRequest_QuandoLancarExcecao() {
        var cursoId = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterAuditoriasCursoAsync(cursoId))
            .ThrowsAsync(new Exception("Erro"));

        var resultado = await _controller.ObterPorCurso(cursoId);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ObterPendentes_DeveRetornarOk() {
        _serviceMock.Setup(s => s.ObterAuditoriasPendentesAsync())
            .ReturnsAsync(new List<AuditoriaResponseDTO>());

        var resultado = await _controller.ObterPendentes();

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ObterConcluidas_DeveRetornarOk() {
        _serviceMock.Setup(s => s.ObterAuditoriasConcluidasAsync(null))
            .ReturnsAsync(new List<AuditoriaResponseDTO>());

        var resultado = await _controller.ObterConcluidas(null);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Criar_DeveRetornarOk_QuandoSucesso() {
        var auditorId = Guid.NewGuid();
        MockUsuarioAutenticado(auditorId);
        var dto = new AuditoriaCreateDTO { CursoId = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarAuditoriaAsync(It.IsAny<AuditoriaCreateDTO>()))
            .ReturnsAsync(new AuditoriaResponseDTO { Id = Guid.NewGuid() });

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Criar_DeveRetornarBadRequest_QuandoServiceLancarExcecao() {
        var auditorId = Guid.NewGuid();
        MockUsuarioAutenticado(auditorId);
        var dto = new AuditoriaCreateDTO();

        _serviceMock.Setup(s => s.CriarAuditoriaAsync(It.IsAny<AuditoriaCreateDTO>()))
            .ThrowsAsync(new Exception("Sem permissão"));

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Atualizar_DeveRetornarOk_QuandoSucesso() {
        var auditorId = Guid.NewGuid();
        var auditoriaId = Guid.NewGuid();
        MockUsuarioAutenticado(auditorId);
        var dto = new AuditoriaUpdateDTO { Resultado = EStatusAuditoria.Aprovado };

        _serviceMock.Setup(s => s.AtualizarAuditoriaAsync(auditoriaId, auditorId, dto))
            .ReturnsAsync(new AuditoriaResponseDTO { Id = auditoriaId });

        var resultado = await _controller.Atualizar(auditoriaId, dto);

        resultado.Should().BeOfType<OkObjectResult>();
    }
}