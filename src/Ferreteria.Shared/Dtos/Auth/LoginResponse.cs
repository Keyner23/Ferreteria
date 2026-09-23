using System;
using System.Collections.Generic;
using System.Text;

namespace Ferreteria.Shared.Dtos.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? ClienteId { get; set; }
    }
}
