using System;
using System.Collections.Generic;
using System.Text;

namespace Ferreteria.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verificar(string password, string hash);
    }
}
