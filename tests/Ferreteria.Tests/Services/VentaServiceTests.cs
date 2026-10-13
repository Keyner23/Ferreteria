using Ferreteria.Domain.Entities;
using Ferreteria.Infrastructure.Services;
using Ferreteria.Shared.Dtos.Ventas;
using Ferreteria.Tests.Comun;
using Microsoft.EntityFrameworkCore;

namespace Ferreteria.Tests.Services
{
    public class VentaServiceTests : BaseDeDatosTests
    {
        [Fact]
        public async Task Registrar_compra_normal_calcula_el_total_y_descuenta_el_stock()
        {
            // Arrange: una categoría, un cliente y un martillo de $25.000 con 10 en stock
            using (var preparacion = NuevoContexto())
            {
                preparacion.Categorias.Add(new Categoria { Id = 1, Nombre = "Herramientas" });
                preparacion.Clientes.Add(new Cliente { Id = 1, Nombre = "Ana", Apellido = "Perez", Documento = "1234567" });
                preparacion.Productos.Add(new Producto
                {
                    Id = 1,
                    Codigo = "MART-010",
                    Nombre = "Martillo",
                    PrecioUnitario = 25000m,
                    Stock = 10,
                    CategoriaId = 1
                });
                await preparacion.SaveChangesAsync();
            }

            var pedido = new VentaRequest
            {
                Items = { new ItemVentaRequest { ProductoId = 1, Cantidad = 3 } }
            };

            // Act
            VentaDto venta;
            using (var contextoDelServicio = NuevoContexto())
            {
                var servicio = new VentaService(contextoDelServicio);
                venta = await servicio.RegistrarAsync(clienteId: 1, pedido);
            }

            // Assert
            Assert.Equal(75000m, venta.Total);
            Assert.Single(venta.Detalles);

            using var verificacion = NuevoContexto();
            var producto = await verificacion.Productos.SingleAsync();
            Assert.Equal(7, producto.Stock);
        }
    }
}