using Ferreteria.Application.Exceptions;
using Ferreteria.Application.Interfaces;
using Ferreteria.Domain.Entities;
using Ferreteria.Infrastructure.Data;
using Ferreteria.Shared.Dtos.Clientes;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Ferreteria.Infrastructure.Services
{
    public class ClienteService : IClienteService
    {
        private readonly FerreteriaDbContext _context;

        public ClienteService(FerreteriaDbContext context)
        {
            _context = context;
        }

        private static readonly Expression<Func<Cliente, ClienteDto>> ADto =
            c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Documento = c.Documento,
                Telefono = c.Telefono,
                Direccion = c.Direccion,
                Activo = c.Activo,
                Correo = c.Usuario != null ? c.Usuario.Correo : null,
                TieneCuenta = c.UsuarioId != null
            };

        public async Task<List<ClienteDto>> ObtenerTodosAsync(bool soloActivos = true)
        {
            var consulta = _context.Clientes.AsNoTracking();

            if (soloActivos)
                consulta = consulta.Where(c => c.Activo);

            return await consulta.OrderBy(c => c.Apellido)
                                 .ThenBy(c => c.Nombre)
                                 .Select(ADto)
                                 .ToListAsync();
        }

        public async Task<ClienteDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Clientes
                                 .AsNoTracking()
                                 .Where(c => c.Id == id)
                                 .Select(ADto)
                                 .FirstOrDefaultAsync();
        }

        /// <summary>Cliente de mostrador: sin cuenta de usuario, lo registra el admin.</summary>
        public async Task<ClienteDto> CrearAsync(ClienteRequest request)
        {
            var documento = request.Documento.Trim();

            if (await _context.Clientes.AnyAsync(c => c.Documento == documento))
                throw new ConflictException($"Ya existe un cliente con el documento {documento}.");

            var cliente = new Cliente
            {
                UsuarioId = null,
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Documento = documento,
                Telefono = request.Telefono?.Trim(),
                Direccion = request.Direccion?.Trim(),
                Activo = request.Activo
            };

            _context.Clientes.Add(cliente);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException($"Ya existe un cliente con el documento {documento}.");
            }

            return (await ObtenerPorIdAsync(cliente.Id))!;
        }

        public async Task ActualizarAsync(int id, ClienteRequest request)
        {
            var cliente = await _context.Clientes.FindAsync(id)
                ?? throw new NotFoundException($"No existe un cliente con Id {id}.");

            var documento = request.Documento.Trim();

            if (await _context.Clientes.AnyAsync(c => c.Documento == documento && c.Id != id))
                throw new ConflictException($"Ya existe otro cliente con el documento {documento}.");

            cliente.Nombre = request.Nombre.Trim();
            cliente.Apellido = request.Apellido.Trim();
            cliente.Documento = documento;
            cliente.Telefono = request.Telefono?.Trim();
            cliente.Direccion = request.Direccion?.Trim();
            cliente.Activo = request.Activo;

            await _context.SaveChangesAsync();
        }

        public async Task DesactivarAsync(int id)
        {
            var cliente = await _context.Clientes
                                        .Include(c => c.Usuario)
                                        .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException($"No existe un cliente con Id {id}.");

            cliente.Activo = false;

            // Si tiene cuenta, también se le bloquea el acceso.
            if (cliente.Usuario is not null)
                cliente.Usuario.Activo = false;

            await _context.SaveChangesAsync();
        }
    }
}
