using Ferreteria.Shared.Dtos.Clientes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ferreteria.Application.Interfaces
{
    public interface IClienteService
    {
        Task<List<ClienteDto>> ObtenerTodosAsync(bool soloActivos = true);
        Task<ClienteDto?> ObtenerPorIdAsync(int id);
        Task<ClienteDto> CrearAsync(ClienteRequest request);
        Task ActualizarAsync(int id, ClienteRequest request);
        Task DesactivarAsync(int id);
    }
}
