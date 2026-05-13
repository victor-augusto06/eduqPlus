using System.Security.Claims;
using EduqPlus.API.Controllers;
using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace EduqPlus.API.Tests.Controllers;

public class UsuarioControllerTests {
    private readonly Mock<IUsuarioService> _serviceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly UsuarioController _controller;

    public UsuarioControllerTests() {
        _serviceMock = new Mock<IUsuarioService>();
        _configMock = new Mock<IConfiguration>();

        // Simula o appsettings.json para a geração do Token
        var mockConfSection = new Mock<IConfigurationSection>();
        mockConfSection.SetupGet(m => m.Value).Returns("SuperSecretKeyQuePrecisaTerMaisDe32CaracteresParaFuncionar");
        _configMock.Setup(c => c.GetSection("JwtSettings:Secret")).Returns(mockConfSection.Object);

        _controller = new UsuarioController(_serviceMock.Object, _configMock.Object);
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
    public async Task Registrar_DeveRetornarCreated_QuandoSucesso() {
        var dto = new UsuarioCreateDTO();
        var response = new UsuarioResponseDTO { Id = Guid.NewGuid() };

        _serviceMock.Setup(s => s.RegistrarAsync(dto)).ReturnsAsync(response);

        var resultado = await _controller.Registrar(dto);

        resultado.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Registrar_DeveRetornarBadRequest_QuandoServiceLancarExcecao() {
        var dto = new UsuarioCreateDTO();
        _serviceMock.Setup(s => s.RegistrarAsync(dto)).ThrowsAsync(new Exception("E-mail em uso"));

        var resultado = await _controller.Registrar(dto);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_DeveRetornarOkComToken_QuandoCredenciaisCorretas() {
        var dto = new UsuarioLoginDTO { Email = "teste@teste.com", Senha = "123" };
        var response = new UsuarioResponseDTO { Id = Guid.NewGuid(), Email = dto.Email, Nome = "Teste" };

        _serviceMock.Setup(s => s.LoginAsync(dto.Email, dto.Senha)).ReturnsAsync(response);

        var resultado = await _controller.Login(dto);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_DeveRetornarUnauthorized_QuandoCredenciaisIncorretas() {
        var dto = new UsuarioLoginDTO();
        _serviceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Inválidos"));

        var resultado = await _controller.Login(dto);

        resultado.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task AlterarPerfil_DeveRetornarOk_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);
        var dto = new UsuarioUpdateDTO();

        _serviceMock.Setup(s => s.AlterarPerfilAsync(usuarioId, usuarioId, dto))
            .ReturnsAsync(new UsuarioResponseDTO());

        var resultado = await _controller.AlterarPerfil(dto);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarNoContent_QuandoSucesso() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirUsuarioAsync(usuarioId, usuarioId)).ReturnsAsync(true);

        var resultado = await _controller.Excluir(usuarioId);

        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Excluir_DeveRetornarBadRequest_QuandoFalhar() {
        var usuarioId = Guid.NewGuid();
        MockUsuarioAutenticado(usuarioId);

        _serviceMock.Setup(s => s.ExcluirUsuarioAsync(usuarioId, usuarioId)).ReturnsAsync(false);

        var resultado = await _controller.Excluir(usuarioId);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }
}