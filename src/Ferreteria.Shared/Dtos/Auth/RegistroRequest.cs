using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ferreteria.Shared.Dtos.Auth
{
    public class RegistroRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, MinimumLength = 2)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El documento es obligatorio.")]
        [StringLength(20, MinimumLength = 5)]
        public string Documento { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El teléfono no es válido.")]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }
    }
}
