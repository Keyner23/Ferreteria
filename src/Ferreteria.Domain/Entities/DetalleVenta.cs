

namespace Ferreteria.Domain.Entities
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        // Copia del producto al momento de vender: no cambia si el producto cambia después
        public string NombreProducto { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }

        public int Cantidad { get; set; }

        public decimal Subtotal => PrecioUnitario * Cantidad;
    }
}
