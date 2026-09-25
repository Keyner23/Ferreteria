using Ferreteria.Shared.Dtos.Ventas;


namespace Ferreteria.Application.Interfaces
{

    public interface IVentaService
    {
        /// <summary>Registra una venta. El clienteId viene del token, no del cuerpo.</summary>
        Task<VentaDto> RegistrarAsync(int clienteId, VentaRequest request);

        /// <summary>Todas las ventas. Solo para el admin.</summary>
        Task<List<VentaDto>> ObtenerTodasAsync();

        /// <summary>Las ventas de un cliente. Para su historial de compras.</summary>
        Task<List<VentaDto>> ObtenerPorClienteAsync(int clienteId);

        Task<VentaDto?> ObtenerPorIdAsync(int id);
    }
}
