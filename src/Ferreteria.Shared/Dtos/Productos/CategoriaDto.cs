using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ferreteria.Shared.Dtos.Productos
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CategoriaRequest
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(60, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;
    }
}
