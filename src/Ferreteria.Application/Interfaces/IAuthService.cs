using Ferreteria.Shared.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ferreteria.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> RegistrarAsync(RegistroRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}

