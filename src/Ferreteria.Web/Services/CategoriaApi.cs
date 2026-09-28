using Ferreteria.Shared.Dtos.Productos;
using System.Net.Http.Json;

namespace Ferreteria.Web.Services
{
    public interface ICategoriaApi
    {
        Task<List<CategoriaDto>> ObtenerTodasAsync();
        Task<CategoriaDto> CrearAsync(CategoriaRequest request);
        Task ActualizarAsync(int id, CategoriaRequest request);
        Task EliminarAsync(int id);
    }

    public class CategoriaApi : ICategoriaApi
    {
        private readonly HttpClient _http;

        public CategoriaApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CategoriaDto>> ObtenerTodasAsync()
        {
            var respuesta = await _http.GetAsync("api/Categorias");

            return await respuesta.LeerAsync<List<CategoriaDto>>();
        }

        public async Task<CategoriaDto> CrearAsync(CategoriaRequest request)
        {
            var respuesta = await _http.PostAsJsonAsync("api/Categorias", request);

            return await respuesta.LeerAsync<CategoriaDto>();
        }

        public async Task ActualizarAsync(int id, CategoriaRequest request)
        {
            var respuesta = await _http.PutAsJsonAsync($"api/Categorias/{id}", request);

            await respuesta.AsegurarExitoAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var respuesta = await _http.DeleteAsync($"api/Categorias/{id}");

            await respuesta.AsegurarExitoAsync();
        }
    }
}
