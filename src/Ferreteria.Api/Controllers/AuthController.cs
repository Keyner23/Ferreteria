using Ferreteria.Application.Interfaces;
using Ferreteria.Shared.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ferreteria.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Crea una cuenta de cliente y devuelve su token.</summary>
        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Registrar([FromBody] RegistroRequest request)
        {
            var respuesta = await _authService.RegistrarAsync(request);
            return Ok(respuesta);
        }

        /// <summary>Valida credenciales y devuelve un token.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var respuesta = await _authService.LoginAsync(request);
            return Ok(respuesta);
        }

        /// <summary>Devuelve los datos del token actual. Sirve para comprobar que la auth funciona.</summary>
        [HttpGet("yo")]
        [Authorize]
        public ActionResult<object> Yo()
        {
            return Ok(new
            {
                UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Correo = User.FindFirstValue(ClaimTypes.Email),
                Rol = User.FindFirstValue(ClaimTypes.Role),
                ClienteId = User.FindFirstValue("clienteId")
            });
        }
    }
}

