using System.Net.Http.Json;
using Ferreteria.Shared.Dtos.Auth;

namespace Ferreteria.Web.Services
{
    public interface IAuthApi
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RegistrarAsync(RegistroRequest request);
    }

    public class AuthApi : IAuthApi
    {
        private readonly HttpClient _http;

        public AuthApi(HttpClient http)
        {
            _http = http;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var respuesta = await _http.PostAsJsonAsync("api/Auth/login", request);

            return await respuesta.LeerAsync<LoginResponse>();
        }

        public async Task<LoginResponse> RegistrarAsync(RegistroRequest request)
        {
            var respuesta = await _http.PostAsJsonAsync("api/Auth/registro", request);

            return await respuesta.LeerAsync<LoginResponse>();
        }
    }
}