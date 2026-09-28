using System.Text;
using Ferreteria.Application.Interfaces;
using Ferreteria.Infrastructure.Auth;
using Ferreteria.Infrastructure.Data;
using Ferreteria.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
namespace Ferreteria.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<FerreteriaDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<IVentaService, VentaService>();



            var jwt = configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");

            if (string.IsNullOrWhiteSpace(jwt.Secret))
                throw new InvalidOperationException("Falta 'Jwt:Secret'. Configúralo con dotnet user-secrets.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwt.Issuer,
                            ValidAudience = jwt.Audience,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwt.Secret)),
                            ClockSkew = TimeSpan.Zero
                        };
                    });

            services.AddAuthorization();

            return services;
        }
    }
}
