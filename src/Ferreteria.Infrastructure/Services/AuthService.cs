using Ferreteria.Application.Exceptions;
using Ferreteria.Application.Interfaces;
using Ferreteria.Domain.Entities;
using Ferreteria.Infrastructure.Data;
using Ferreteria.Shared.Dtos.Auth;
using Microsoft.EntityFrameworkCore;


namespace Ferreteria.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly FerreteriaDbContext _context;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokenService;

        public AuthService(
            FerreteriaDbContext context,
            IPasswordHasher hasher,
            ITokenService tokenService)
        {
            _context = context;
            _hasher = hasher;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> RegistrarAsync(RegistroRequest request)
        {
            var correo = request.Correo.Trim().ToLowerInvariant();

            if (await _context.Usuarios.AnyAsync(u => u.Correo == correo))
                throw new ConflictException($"Ya existe una cuenta con el correo {correo}.");

            if (await _context.Clientes.AnyAsync(c => c.Documento == request.Documento))
                throw new ConflictException($"Ya existe un cliente con el documento {request.Documento}.");

            var usuario = new Usuario
            {
                Correo = correo,
                PasswordHash = _hasher.Hash(request.Password),
                Rol = Rol.Cliente,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            var cliente = new Cliente
            {
                Usuario = usuario,
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Documento = request.Documento.Trim(),
                Telefono = request.Telefono?.Trim(),
                Direccion = request.Direccion?.Trim(),
                Activo = true
            };

            _context.Clientes.Add(cliente);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Red de seguridad: dos registros simultáneos pasaron los Any() de arriba
                // y el índice único rechazó el segundo.
                throw new ConflictException("El correo o el documento ya están registrados.");
            }

            return ConstruirRespuesta(usuario, cliente.Id);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var correo = request.Correo.Trim().ToLowerInvariant();

            var usuario = await _context.Usuarios
                                        .Include(u => u.Cliente)
                                        .FirstOrDefaultAsync(u => u.Correo == correo);

            // Mismo mensaje para "no existe" y "contraseña mala": no revelamos
            // qué correos están registrados.
            if (usuario is null || !_hasher.Verificar(request.Password, usuario.PasswordHash))
                throw new UnauthorizedException("Correo o contraseña incorrectos.");

            if (!usuario.Activo)
                throw new UnauthorizedException("La cuenta está desactivada.");

            return ConstruirRespuesta(usuario, usuario.Cliente?.Id);
        }

        private LoginResponse ConstruirRespuesta(Usuario usuario, int? clienteId)
        {
            var (token, expiraEn) = _tokenService.GenerarToken(usuario, clienteId);

            return new LoginResponse
            {
                Token = token,
                ExpiraEn = expiraEn,
                Correo = usuario.Correo,
                Rol = usuario.Rol.ToString(),
                ClienteId = clienteId
            };
        }
    }
}
