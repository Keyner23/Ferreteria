using Ferreteria.Application.Exceptions;
using Ferreteria.Application.Interfaces;
using Ferreteria.Domain.Entities;
using Ferreteria.Infrastructure.Data;
using Ferreteria.Shared.Dtos.Ventas;
using Microsoft.EntityFrameworkCore;


namespace Ferreteria.Infrastructure.Services
{

    public class VentaService : IVentaService
    {
        private readonly FerreteriaDbContext _context;

        public VentaService(FerreteriaDbContext context)
        {
            _context = context;
        }

        public async Task<VentaDto> RegistrarAsync(int clienteId, VentaRequest request)
        {
            // 1. Agrupar: si el carrito trae el mismo producto dos veces, se suman.
            var pedido = request.Items
                                .GroupBy(i => i.ProductoId)
                                .ToDictionary(g => g.Key, g => g.Sum(i => i.Cantidad));

            var cliente = await _context.Clientes.FindAsync(clienteId)
                ?? throw new NotFoundException($"No existe un cliente con Id {clienteId}.");

            if (!cliente.Activo)
                throw new BusinessException("El cliente está inactivo y no puede comprar.");

            // 2. Traer los productos RASTREADOS: vamos a descontarles stock.
            var ids = pedido.Keys.ToList();

            var productos = await _context.Productos
                                          .Where(p => ids.Contains(p.Id))
                                          .ToListAsync();

            // 3. Validar todo ANTES de tocar nada.
            foreach (var (productoId, cantidad) in pedido)
            {
                var producto = productos.FirstOrDefault(p => p.Id == productoId)
                    ?? throw new NotFoundException($"No existe un producto con Id {productoId}.");

                if (!producto.Activo)
                    throw new BusinessException($"El producto '{producto.Nombre}' no está disponible.");

                if (producto.Stock < cantidad)
                    throw new ConflictException(
                        $"Stock insuficiente de '{producto.Nombre}': quedan {producto.Stock}, pediste {cantidad}.");
            }

            // 4. Construir la venta. Los precios salen de la base, NO del cliente.
            var venta = new Venta
            {
                ClienteId = clienteId,
                Fecha = DateTime.UtcNow
            };

            foreach (var (productoId, cantidad) in pedido)
            {
                var producto = productos.First(p => p.Id == productoId);

                venta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = producto.Id,
                    NombreProducto = producto.Nombre,     // copia histórica
                    PrecioUnitario = producto.PrecioUnitario, // copia histórica
                    Cantidad = cantidad
                });

                producto.Stock -= cantidad;
            }

            venta.Total = venta.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad);

            _context.Ventas.Add(venta);

            // 5. Una sola llamada: la venta, sus detalles y el descuento de stock
            //    entran juntos o no entra nada.
            await _context.SaveChangesAsync();

            return (await ObtenerPorIdAsync(venta.Id))!;
        }

        public async Task<List<VentaDto>> ObtenerTodasAsync()
        {
            return await ConsultaBase()
                         .OrderByDescending(v => v.Fecha)
                         .ToListAsync();
        }

        public async Task<List<VentaDto>> ObtenerPorClienteAsync(int clienteId)
        {
            return await ConsultaBase()
                         .Where(v => v.ClienteId == clienteId)
                         .OrderByDescending(v => v.Fecha)
                         .ToListAsync();
        }

        public async Task<VentaDto?> ObtenerPorIdAsync(int id)
        {
            return await ConsultaBase()
                         .FirstOrDefaultAsync(v => v.Id == id);
        }

        // Proyección a DTO directamente en SQL: no trae entidades ni necesita Include.
        private IQueryable<VentaDto> ConsultaBase()
        {
            return _context.Ventas
                           .AsNoTracking()
                           .Select(v => new VentaDto
                           {
                               Id = v.Id,
                               ClienteId = v.ClienteId,
                               ClienteNombre = v.Cliente!.Nombre + " " + v.Cliente.Apellido,
                               Fecha = v.Fecha,
                               Total = v.Total,
                               Detalles = v.Detalles.Select(d => new DetalleVentaDto
                               {
                                   ProductoId = d.ProductoId,
                                   NombreProducto = d.NombreProducto,
                                   PrecioUnitario = d.PrecioUnitario,
                                   Cantidad = d.Cantidad,
                                   Subtotal = d.PrecioUnitario * d.Cantidad
                               }).ToList()
                           });
        }
    }

}
