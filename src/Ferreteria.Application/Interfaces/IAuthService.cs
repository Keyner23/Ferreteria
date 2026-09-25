using Ferreteria.Shared.Dtos.Auth;


namespace Ferreteria.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> RegistrarAsync(RegistroRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}

