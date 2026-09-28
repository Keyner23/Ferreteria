using Ferreteria.Shared.Dtos.Productos;
using System.Net.Http.Json;

namespace Ferreteria.Web.Services
{
    public interface IProductoApi
    {
        Task<List<ProductoDto>> ObtenerTodosAsync(bool incluirInactivos = false);
        Task<ProductoDto?> ObtenerPorIdAsync(int id);
        Task<ProductoDto> CrearAsync(ProductoRequest request);
        Task ActualizarAsync(int id, ProductoRequest request);
        Task DesactivarAsync(int id);
    }

    public class ProductoApi : IProductoApi
    {
        private readonly HttpClient _http;

        public ProductoApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProductoDto>> ObtenerTodosAsync(bool incluirInactivos = false)
        {
            var url = incluirInactivos ? "api/Productos?incluirInactivos=true" : "api/Productos";

            var respuesta = await _http.GetAsync(url);

            return await respuesta.LeerAsync<List<ProductoDto>>();
        }

        public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
        {
            var respuesta = await _http.GetAsync($"api/Productos/{id}");

            if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return await respuesta.LeerAsync<ProductoDto>();
        }

        public async Task<ProductoDto> CrearAsync(ProductoRequest request)
        {
            var respuesta = await _http.PostAsJsonAsync("api/Productos", request);

            return await respuesta.LeerAsync<ProductoDto>();
        }

        public async Task ActualizarAsync(int id, ProductoRequest request)
        {
            var respuesta = await _http.PutAsJsonAsync($"api/Productos/{id}", request);

            await respuesta.AsegurarExitoAsync();
        }

        public async Task DesactivarAsync(int id)
        {
            var respuesta = await _http.DeleteAsync($"api/Productos/{id}");

            await respuesta.AsegurarExitoAsync();
        }
    }
}
