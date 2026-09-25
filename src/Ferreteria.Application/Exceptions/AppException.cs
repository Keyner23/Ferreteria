using System;
using System.Collections.Generic;
using System.Text;

namespace Ferreteria.Application.Exceptions
{

    /// <summary>
    /// Base de los errores de negocio que el servicio sabe describir.
    /// Lleva el código HTTP que le corresponde, pero no conoce ASP.NET:
    /// es solo un número. La capa Api lo traduce a una respuesta.
    /// </summary>
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }

        protected AppException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>No existe el recurso pedido o uno al que se hace referencia. → 404</summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(404, message) { }
    }

    /// <summary>Choca con el estado actual: duplicado, stock insuficiente. → 409</summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(409, message) { }
    }

    /// <summary>Credenciales inválidas o cuenta inactiva. → 401</summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(401, message) { }
    }

    /// <summary>La petición es válida pero la regla de negocio la rechaza. → 400</summary>
    public class BusinessException : AppException
    {
        public BusinessException(string message) : base(400, message) { }
    }
}
