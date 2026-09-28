using System.Net;
using System.Net.Http.Headers;

namespace Ferreteria.Web.Auth
{

    /// <summary>
    /// Se mete en medio de cada petición HTTP: adjunta el token si hay sesión,
    /// y si la API responde 401 limpia la sesión local.
    /// </summary>
    public class AuthorizationHandler : DelegatingHandler
    {
        private readonly ITokenStore _tokenStore;

        public AuthorizationHandler(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenStore.ObtenerTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var respuesta = await base.SendAsync(request, cancellationToken);

            // El servidor rechazó el token: venció o dejó de ser válido.
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                await _tokenStore.LimpiarAsync();

            return respuesta;
        }
    }
}
