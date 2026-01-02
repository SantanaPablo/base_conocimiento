using BaseConocimiento.Application.Interfaces.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Infrastructure.Services.Auth
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public string HashPassword(string password)
        {
            // Generar salt aleatorio
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Generar hash con PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // salt + hash
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
               
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize);

                    for (int i = 0; i < HashSize; i++)
                    {
                        if (hashBytes[i + SaltSize] != hash[i])
                            return false;
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
