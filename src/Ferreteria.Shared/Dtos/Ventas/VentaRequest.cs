using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ferreteria.Shared.Dtos.Ventas
{
    public class VentaRequest
    {
        [Required(ErrorMessage = "La venta debe tener al menos un producto.")]
        [MinLength(1, ErrorMessage = "La venta debe tener al menos un producto.")]
        public List<ItemVentaRequest> Items { get; set; } = new();
    }

    public class ItemVentaRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el producto.")]
        public int ProductoId { get; set; }

        [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000.")]
        public int Cantidad { get; set; }
    }

}
