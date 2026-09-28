using Ferreteria.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ferreteria.Application.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Ferreteria.Infrastructure.Data
{

    public static class DbSeeder
    {
        public static async Task SembrarAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<FerreteriaDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                              .CreateLogger(nameof(DbSeeder));

            await context.Database.MigrateAsync();

            await SembrarAdminAsync(context, hasher, config, logger);
            await SembrarCategoriasAsync(context, logger);
        }

        private static async Task SembrarAdminAsync(
            FerreteriaDbContext context,
            IPasswordHasher hasher,
            IConfiguration config,
            ILogger logger)
        {
            if (await context.Usuarios.AnyAsync(u => u.Rol == Rol.Admin))
                return;

            var correo = config["Admin:Correo"];
            var password = config["Admin:Password"];

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning(
                    "No se sembró el administrador: faltan 'Admin:Correo' o 'Admin:Password'. " +
                    "Configúralos con dotnet user-secrets.");
                return;
            }

            context.Usuarios.Add(new Usuario
            {
                Correo = correo.Trim().ToLowerInvariant(),
                PasswordHash = hasher.Hash(password),
                Rol = Rol.Admin,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            logger.LogInformation("Administrador sembrado: {Correo}", correo);
        }

        private static async Task SembrarCategoriasAsync(FerreteriaDbContext context, ILogger logger)
        {
            if (await context.Categorias.AnyAsync())
                return;

            context.Categorias.AddRange(
                new Categoria { Nombre = "Herramientas manuales" },
                new Categoria { Nombre = "Herramientas eléctricas" },
                new Categoria { Nombre = "Pinturas y solventes" },
                new Categoria { Nombre = "Tornillería" },
                new Categoria { Nombre = "Plomería" },
                new Categoria { Nombre = "Electricidad" });

            await context.SaveChangesAsync();

            logger.LogInformation("Categorías iniciales sembradas.");
        }
    }
}
