using Ferreteria.Application.Interfaces;
using Ferreteria.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ferreteria.Infrastructure.Auth
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario, int? clienteId)
        {
            var expiraEn = DateTime.UtcNow.AddMinutes(_settings.MinutosExpiracion);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol.ToString())
            };

            if (clienteId is not null)
                claims.Add(new Claim("clienteId", clienteId.Value.ToString()));

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiraEn,
                signingCredentials: credenciales);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
        }
    }
}
