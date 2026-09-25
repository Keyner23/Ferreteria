using Ferreteria.Shared.Dtos.Productos;


namespace Ferreteria.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> ObtenerTodasAsync();
        Task<CategoriaDto> CrearAsync(CategoriaRequest request);
        Task ActualizarAsync(int id, CategoriaRequest request);
        Task EliminarAsync(int id);
    }
}
