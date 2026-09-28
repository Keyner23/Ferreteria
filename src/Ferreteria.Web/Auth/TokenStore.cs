using Blazored.LocalStorage;
using Ferreteria.Shared.Dtos.Auth;

namespace Ferreteria.Web.Auth
{

    public interface ITokenStore
    {
        Task<string?> ObtenerTokenAsync();
        Task GuardarAsync(LoginResponse sesion);
        Task LimpiarAsync();
    }

    public class TokenStore : ITokenStore
    {
        private const string Clave = "ferreteria_token";

        private readonly ILocalStorageService _storage;

        public TokenStore(ILocalStorageService storage)
        {
            _storage = storage;
        }

        public async Task<string?> ObtenerTokenAsync()
        {
            try
            {
                return await _storage.GetItemAsStringAsync(Clave);
            }
            catch
            {
                // Navegador en modo privado o con almacenamiento bloqueado.
                return null;
            }
        }

        public async Task GuardarAsync(LoginResponse sesion)
        {
            await _storage.SetItemAsStringAsync(Clave, sesion.Token);
        }

        public async Task LimpiarAsync()
        {
            await _storage.RemoveItemAsync(Clave);
        }
    }
}
