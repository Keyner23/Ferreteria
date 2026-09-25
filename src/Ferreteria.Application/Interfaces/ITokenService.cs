using Ferreteria.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ferreteria.Application.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario, int? clienteId);
    }
}
