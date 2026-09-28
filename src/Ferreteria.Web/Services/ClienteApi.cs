using Ferreteria.Shared.Dtos.Clientes;
using System.Net.Http.Json;

namespace Ferreteria.Web.Services
{
    public interface IClienteApi
    {
        Task<List<ClienteDto>> ObtenerTodosAsync(bool incluirInactivos = false);
        Task<ClienteDto?> ObtenerPorIdAsync(int id);
        Task<ClienteDto> CrearAsync(ClienteRequest request);
        Task ActualizarAsync(int id, ClienteRequest request);
        Task DesactivarAsync(int id);
    }

    public class ClienteApi : IClienteApi
    {
        private readonly HttpClient _http;

        public ClienteApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ClienteDto>> ObtenerTodosAsync(bool incluirInactivos = false)
        {
            var url = incluirInactivos ? "api/Clientes?incluirInactivos=true" : "api/Clientes";

            var respuesta = await _http.GetAsync(url);

            return await respuesta.LeerAsync<List<ClienteDto>>();
        }

        public async Task<ClienteDto?> ObtenerPorIdAsync(int id)
        {
            var respuesta = await _http.GetAsync($"api/Clientes/{id}");

            if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            return await respuesta.LeerAsync<ClienteDto>();
        }

        public async Task<ClienteDto> CrearAsync(ClienteRequest request)
        {
            var respuesta = await _http.PostAsJsonAsync("api/Clientes", request);

            return await respuesta.LeerAsync<ClienteDto>();
        }

        public async Task ActualizarAsync(int id, ClienteRequest request)
        {
            var respuesta = await _http.PutAsJsonAsync($"api/Clientes/{id}", request);

            await respuesta.AsegurarExitoAsync();
        }

        public async Task DesactivarAsync(int id)
        {
            var respuesta = await _http.DeleteAsync($"api/Clientes/{id}");

            await respuesta.AsegurarExitoAsync();
        }
    }
}
