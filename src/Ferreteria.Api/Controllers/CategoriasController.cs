using Ferreteria.Application.Interfaces;
using Ferreteria.Shared.Dtos.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ferreteria.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {

        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        /// <summary>Lista las categorías. Público: el catálogo se ve sin iniciar sesión.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<CategoriaDto>>> GetTodas()
        {
            return Ok(await _categoriaService.ObtenerTodasAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoriaDto>> Crear([FromBody] CategoriaRequest request)
        {
            var creada = await _categoriaService.CrearAsync(request);
            return CreatedAtAction(nameof(GetTodas), new { id = creada.Id }, creada);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] CategoriaRequest request)
        {
            await _categoriaService.ActualizarAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _categoriaService.EliminarAsync(id);
            return NoContent();
        }

    }
}
