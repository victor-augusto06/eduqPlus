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

public class AvaliacaoControllerTests {
    private readonly Mock<IAvaliacaoService> _serviceMock;
    private readonly AvaliacaoController _controller;

    public AvaliacaoControllerTests() {
        _serviceMock = new Mock<IAvaliacaoService>();
        _controller = new AvaliacaoController(_serviceMock.Object);
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
    public async Task Criar_DeveRetornarCreated_QuandoDadosValidos() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);
        var dto = new AvaliacaoCreateDTO { CursoId = Guid.NewGuid(), NotaEntrega = 5.0 };
        var response = new AvaliacaoResponseDTO { Id = Guid.NewGuid(), UsuarioId = usuarioId };

        _serviceMock.Setup(s => s.CriarAvaliacaoAsync(It.IsAny<AvaliacaoCreateDTO>())).ReturnsAsync(response);

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoEncontrado() {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ThrowsAsync(new Exception("Não encontrado"));

        var resultado = await _controller.ObterPorId(id);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ValidarComprovante_DeveRetornarOk_QuandoAdminValida() {
        var adminId = Guid.NewGuid();
        var avalId = Guid.NewGuid();
        MockUsuarioAutenticado(adminId);

        _serviceMock.Setup(s => s.ValidarComprovanteAsync(avalId, adminId, EStatusComprovante.Aprovado))
            .ReturnsAsync(new AvaliacaoResponseDTO { Id = avalId });

        var resultado = await _controller.ValidarComprovante(avalId, EStatusComprovante.Aprovado);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarNoContent_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        var avalId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirAvaliacaoAsync(avalId, usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.Excluir(avalId);

        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Atualizar_DeveRetornarBadRequest_QuandoErroDePermissao() {
        var usuarioId = Guid.NewGuid();
        var avalId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);
        var dto = new AvaliacaoUpdateDTO();

        _serviceMock.Setup(s => s.AtualizarAvaliacaoAsync(avalId, usuarioId, dto))
            .ThrowsAsync(new Exception("Sem permissão"));

        var resultado = await _controller.Atualizar(avalId, dto);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }
}