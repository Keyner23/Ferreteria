using Ferreteria.Application.Interfaces;
using Ferreteria.Shared.Dtos.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferreteria.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ClientesController : ControllerBase
    {

        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ClienteDto>>> GetTodos([FromQuery] bool incluirInactivos = false)
        {
            return Ok(await _clienteService.ObtenerTodosAsync(!incluirInactivos));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteDto>> GetPorId(int id)
        {
            var cliente = await _clienteService.ObtenerPorIdAsync(id);

            if (cliente is null)
                return NotFound();

            return Ok(cliente);
        }

        /// <summary>Registra un cliente de mostrador, sin cuenta de acceso.</summary>
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> Crear([FromBody] ClienteRequest request)
        {
            var creado = await _clienteService.CrearAsync(request);
            return CreatedAtAction(nameof(GetPorId), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ClienteRequest request)
        {
            await _clienteService.ActualizarAsync(id, request);
            return NoContent();
        }

        /// <summary>Desactiva el cliente y bloquea su acceso. No se borra: tiene ventas.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            await _clienteService.DesactivarAsync(id);
            return NoContent();
        }
    }
}

