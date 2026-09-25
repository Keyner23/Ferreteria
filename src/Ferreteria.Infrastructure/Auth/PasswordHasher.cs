using Ferreteria.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ferreteria.Infrastructure.Auth
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool Verificar(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // El hash guardado está corrupto o no es de BCrypt.
                return false;
            }
        }
    }
}
