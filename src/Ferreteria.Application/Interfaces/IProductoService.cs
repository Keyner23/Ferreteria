using Ferreteria.Shared.Dtos.Productos;

namespace Ferreteria.Application.Interfaces
{
    public interface IProductoService
    {
        Task<List<ProductoDto>> ObtenerTodosAsync(bool soloActivos = true);
        Task<ProductoDto?> ObtenerPorIdAsync(int id);
        Task<ProductoDto> CrearAsync(ProductoRequest request);
        Task ActualizarAsync(int id, ProductoRequest request);
        Task DesactivarAsync(int id);
    }
}

