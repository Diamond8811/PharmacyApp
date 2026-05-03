using System;
using System.Security.Cryptography;

namespace PharmacyApp.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 24;       
        private const int HashSize = 32;      
        private const int Iterations = 10_000;

        public static string ComputeHash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return string.Empty;

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            try
            {
                string[] parts = storedHash.Split('.');
                if (parts.Length != 3) return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] expectedHash = Convert.FromBase64String(parts[2]);

                byte[] actualHash;
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
                {
                    actualHash = pbkdf2.GetBytes(expectedHash.Length);
                }

                return SlowEquals(expectedHash, actualHash);
            }
            catch
            {
                return false;
            }
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }
    }
}