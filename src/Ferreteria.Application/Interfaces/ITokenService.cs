using Ferreteria.Domain.Entities;



namespace Ferreteria.Application.Interfaces
{

    public interface ITokenService
    {
        (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario, int? clienteId);
    }
}
