using System;
using System.Collections.Generic;
using System.Text;

namespace Ferreteria.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Rol Rol { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; }

        public Cliente? Cliente { get; set; }
    }
}
