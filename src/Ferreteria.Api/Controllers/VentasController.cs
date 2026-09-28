using Ferreteria.Application.Exceptions;
using Ferreteria.Application.Interfaces;
using Ferreteria.Shared.Dtos.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ferreteria.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        //<summary>Registra una compra. El cliente sale del token, no del cuerpo.</summary>
        [HttpPost]
        public async Task<ActionResult<VentaDto>> Comprar([FromBody] VentaRequest request)
        {
            var clienteId = ObtenerClienteId();

            var venta = await _ventaService.RegistrarAsync(clienteId, request);

            return CreatedAtAction(nameof(GetPorId), new { id = venta.Id }, venta);
        }

        /// <summary>Historial de compras del usuario autenticado.</summary>
        [HttpGet("mis-compras")]
        public async Task<ActionResult<List<VentaDto>>> MisCompras()
        {
            var clienteId = ObtenerClienteId();

            return Ok(await _ventaService.ObtenerPorClienteAsync(clienteId));
        }

        /// <summary>Todas las ventas. Solo el admin.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<VentaDto>>> GetTodas()
        {
            return Ok(await _ventaService.ObtenerTodasAsync());
        }

        /// <summary>Una venta. El cliente solo puede ver las suyas; el admin, cualquiera.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VentaDto>> GetPorId(int id)
        {
            var venta = await _ventaService.ObtenerPorIdAsync(id);

            if (venta is null)
                return NotFound();

            if (!User.IsInRole("Admin") && venta.ClienteId != ObtenerClienteId())
                return Forbid();

            return Ok(venta);
        }

        /// <summary>Lee el claim 'clienteId' del token. Un admin no tiene ficha de cliente.</summary>
        private int ObtenerClienteId()
        {
            var valor = User.FindFirstValue("clienteId");

            if (!int.TryParse(valor, out var clienteId))
                throw new BusinessException(
                    "Esta cuenta no tiene una ficha de cliente asociada y no puede comprar.");

            return clienteId;
        }
    }
}
