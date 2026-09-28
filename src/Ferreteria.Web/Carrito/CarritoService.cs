using Blazored.LocalStorage;
using Ferreteria.Shared.Dtos.Productos;
using Ferreteria.Shared.Dtos.Ventas;

namespace Ferreteria.Web.Carrito
{

    public interface ICarritoService
    {
        /// <summary>Se dispara cuando el carrito cambia, para que la interfaz se repinte.</summary>
        event Func<Task>? Cambio;

        Task<List<ItemCarrito>> ObtenerAsync();
        Task AgregarAsync(ProductoDto producto, int cantidad = 1);
        Task CambiarCantidadAsync(int productoId, int cantidad);
        Task QuitarAsync(int productoId);
        Task VaciarAsync();
        Task<int> ContarUnidadesAsync();
        Task<VentaRequest> ConstruirPedidoAsync();
    }

    public class CarritoService : ICarritoService
    {
        private const string Clave = "ferreteria_carrito";

        private readonly ILocalStorageService _storage;

        public CarritoService(ILocalStorageService storage)
        {
            _storage = storage;
        }

        public event Func<Task>? Cambio;

        public async Task<List<ItemCarrito>> ObtenerAsync()
        {
            try
            {
                return await _storage.GetItemAsync<List<ItemCarrito>>(Clave) ?? new();
            }
            catch
            {
                // Almacenamiento bloqueado o contenido corrupto.
                return new();
            }
        }

        public async Task AgregarAsync(ProductoDto producto, int cantidad = 1)
        {
            if (cantidad < 1)
                return;

            var items = await ObtenerAsync();
            var item = items.FirstOrDefault(i => i.ProductoId == producto.Id);

            if (item is null)
            {
                items.Add(new ItemCarrito
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Codigo = producto.Codigo,
                    PrecioUnitario = producto.PrecioUnitario,
                    Cantidad = Math.Min(cantidad, producto.Stock),
                    StockDisponible = producto.Stock
                });
            }
            else
            {
                // Se actualiza el precio y el stock por si cambiaron desde la última vez.
                item.PrecioUnitario = producto.PrecioUnitario;
                item.StockDisponible = producto.Stock;
                item.Cantidad = Math.Min(item.Cantidad + cantidad, producto.Stock);
            }

            await GuardarAsync(items);
        }

        public async Task CambiarCantidadAsync(int productoId, int cantidad)
        {
            var items = await ObtenerAsync();
            var item = items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item is null)
                return;

            if (cantidad < 1)
            {
                items.Remove(item);
            }
            else
            {
                item.Cantidad = item.StockDisponible > 0
                    ? Math.Min(cantidad, item.StockDisponible)
                    : cantidad;
            }

            await GuardarAsync(items);
        }

        public async Task QuitarAsync(int productoId)
        {
            var items = await ObtenerAsync();

            items.RemoveAll(i => i.ProductoId == productoId);

            await GuardarAsync(items);
        }

        public async Task VaciarAsync()
        {
            await _storage.RemoveItemAsync(Clave);

            await NotificarAsync();
        }

        public async Task<int> ContarUnidadesAsync()
        {
            var items = await ObtenerAsync();

            return items.Sum(i => i.Cantidad);
        }

        /// <summary>
        /// Arma lo que se le manda a la API: SOLO producto y cantidad.
        /// Los precios los pone el servidor.
        /// </summary>
        public async Task<VentaRequest> ConstruirPedidoAsync()
        {
            var items = await ObtenerAsync();

            return new VentaRequest
            {
                Items = items.Select(i => new ItemVentaRequest
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad
                }).ToList()
            };
        }

        private async Task GuardarAsync(List<ItemCarrito> items)
        {
            await _storage.SetItemAsync(Clave, items);

            await NotificarAsync();
        }

        private async Task NotificarAsync()
        {
            if (Cambio is not null)
                await Cambio.Invoke();
        }
    }
}
