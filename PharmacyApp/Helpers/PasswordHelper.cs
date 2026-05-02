using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Helpers
{
    public static class PasswordHelper
    {
        public static string ComputeHash(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return string.Empty;
            return "hash_" + password.Trim();
        }
    }
}
