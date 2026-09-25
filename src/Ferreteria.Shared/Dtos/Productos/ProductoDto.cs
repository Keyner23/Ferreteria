using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ferreteria.Shared.Dtos.Productos
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
    }

    public class ProductoRequest
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30, MinimumLength = 2)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal PrecioUnitario { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la categoría.")]
        public int CategoriaId { get; set; }

        public bool Activo { get; set; } = true;
    }

}
