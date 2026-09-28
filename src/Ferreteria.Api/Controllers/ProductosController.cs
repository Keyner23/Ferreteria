using Ferreteria.Application.Interfaces;
using Ferreteria.Shared.Dtos.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferreteria.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {

        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        /// <summary>Catálogo. Solo el admin puede pedir los inactivos.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductoDto>>> GetTodos([FromQuery] bool incluirInactivos = false)
        {
            var esAdmin = User.IsInRole("Admin");
            var soloActivos = !(incluirInactivos && esAdmin);

            return Ok(await _productoService.ObtenerTodosAsync(soloActivos));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductoDto>> GetPorId(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);

            if (producto is null)
                return NotFound();

            return Ok(producto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductoDto>> Crear([FromBody] ProductoRequest request)
        {
            var creado = await _productoService.CrearAsync(request);
            return CreatedAtAction(nameof(GetPorId), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ProductoRequest request)
        {
            await _productoService.ActualizarAsync(id, request);
            return NoContent();
        }

        /// <summary>Desactiva el producto. No se borra: tiene ventas asociadas.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Desactivar(int id)
        {
            await _productoService.DesactivarAsync(id);
            return NoContent();
        }
    }
}
