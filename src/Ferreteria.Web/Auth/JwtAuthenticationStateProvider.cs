using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ferreteria.Shared.Dtos.Auth;
using Microsoft.AspNetCore.Components.Authorization;


namespace Ferreteria.Web.Auth
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly AuthenticationState Anonimo =
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        private readonly ITokenStore _tokenStore;

        public JwtAuthenticationStateProvider(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        /// <summary>Blazor llama a esto para saber quién está conectado.</summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _tokenStore.ObtenerTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
                return Anonimo;

            var jwt = LeerToken(token);

            if (jwt is null || jwt.ValidTo <= DateTime.UtcNow)
            {
                // Token corrupto o vencido: se descarta.
                await _tokenStore.LimpiarAsync();
                return Anonimo;
            }

            var identidad = new ClaimsIdentity(jwt.Claims, "jwt", ClaimTypes.Email, ClaimTypes.Role);

            return new AuthenticationState(new ClaimsPrincipal(identidad));
        }

        /// <summary>Tras iniciar sesión: guarda el token y avisa a toda la interfaz.</summary>
        public async Task MarcarComoAutenticadoAsync(LoginResponse sesion)
        {
            await _tokenStore.GuardarAsync(sesion);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>Cerrar sesión.</summary>
        public async Task CerrarSesionAsync()
        {
            await _tokenStore.LimpiarAsync();

            NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
        }

        private static JwtSecurityToken? LeerToken(string token)
        {
            try
            {
                return new JwtSecurityTokenHandler().ReadJwtToken(token);
            }
            catch
            {
                return null;
            }
        }
    }
}
