using System.Security.Claims;
using EduqPlus.API.Controllers;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduqPlus.API.Tests.Controllers;

public class DenunciaControllerTests {
    private readonly Mock<IDenunciaService> _serviceMock;
    private readonly DenunciaController _controller;

    public DenunciaControllerTests() {
        _serviceMock = new Mock<IDenunciaService>();
        _controller = new DenunciaController(_serviceMock.Object);
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
        var dto = new DenunciaCreateDTO();
        var response = new DenunciaResponseDTO { Id = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarDenunciaAsync(dto, usuarioId)).ReturnsAsync(response);

        var resultado = await _controller.Criar(dto);

        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarNotFound_QuandoServiceLancarExcecao() {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ObterPorIdAsync(id)).ThrowsAsync(new Exception("Erro"));

        var resultado = await _controller.ObterPorId(id);

        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ObterDenunciasUsuario_DeveRetornarOk() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ObterDenunciaUsuarioAsync(usuarioId))
            .ReturnsAsync(new List<DenunciaResponseDTO>());

        var resultado = await _controller.ObterDenunciasUsuario();

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AtualizarAdmin_DeveRetornarBadRequest_QuandoErroDePermissao() {
        var adminId = Guid.NewGuid();
        var denunciaId = Guid.NewGuid();
        MockUsuarioAutenticado(adminId);
        var dto = new DenunciaUpdateAdminDTO();

        _serviceMock.Setup(s => s.AlterarDenunciaAdminAsync(denunciaId, adminId, dto))
            .ThrowsAsync(new Exception("Sem permissão"));

        var resultado = await _controller.AtualizarAdmin(denunciaId, dto);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarNoContent_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        var denunciaId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirDenunciaAsync(denunciaId, usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.Excluir(denunciaId);

        resultado.Should().BeOfType<NoContentResult>();
    }
}