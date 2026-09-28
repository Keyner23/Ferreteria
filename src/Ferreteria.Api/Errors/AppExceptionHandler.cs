using Ferreteria.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ferreteria.Api.Errors
{
    public class AppExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<AppExceptionHandler> _logger;

        public AppExceptionHandler(ILogger<AppExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not AppException appException)
            {
                _logger.LogError(exception, "Error no controlado en {Ruta}", httpContext.Request.Path);
                return false;
            }

            var problem = new ProblemDetails
            {
                Status = appException.StatusCode,
                Title = appException.Message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = appException.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }
    }
}
