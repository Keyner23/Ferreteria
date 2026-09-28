using System.Net.Http.Json;

namespace Ferreteria.Web.Services
{
    /// <summary>Error de la API ya traducido a un mensaje mostrable.</summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(string message, int statusCode = 0) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>Espejo mínimo del ProblemDetails que devuelve ASP.NET.</summary>
    public class ProblemDetailsDto
    {
        public string? Title { get; set; }
        public int? Status { get; set; }
        public Dictionary<string, List<string>>? Errors { get; set; }
    }

    public static class ApiResponseExtensions
    {
        /// <summary>Devuelve el contenido, o lanza ApiException con el mensaje de la API.</summary>
        public static async Task<T> LeerAsync<T>(this HttpResponseMessage respuesta)
        {
            await AsegurarExitoAsync(respuesta);

            return (await respuesta.Content.ReadFromJsonAsync<T>())!;
        }

        /// <summary>Para respuestas sin cuerpo (204). Lanza si no fue exitosa.</summary>
        public static async Task AsegurarExitoAsync(this HttpResponseMessage respuesta)
        {
            if (respuesta.IsSuccessStatusCode)
                return;

            throw new ApiException(await LeerMensajeAsync(respuesta), (int)respuesta.StatusCode);
        }

        private static async Task<string> LeerMensajeAsync(HttpResponseMessage respuesta)
        {
            try
            {
                var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetailsDto>();

                // Los 400 de validación traen el detalle por campo.
                if (problema?.Errors is { Count: > 0 })
                    return string.Join(" ", problema.Errors.SelectMany(e => e.Value));

                if (!string.IsNullOrWhiteSpace(problema?.Title))
                    return problema.Title;
            }
            catch
            {
                // La respuesta no era ProblemDetails.
            }

            return respuesta.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Tu sesión expiró. Vuelve a iniciar sesión.",
                System.Net.HttpStatusCode.Forbidden => "No tienes permiso para hacer esto.",
                _ => "Ocurrió un error inesperado. Intenta de nuevo."
            };
        }
    }   
}
        