using SkyLabIdP.Application.Common.Interfaces;
using System.Security.Cryptography;

namespace SkyLabIdP.Shared.Services
{
    public class SaltGenerator : ISaltGenerator
    {
        public string GenerateSecureSalt(int size = 16)
        {
            var randomBytes = new byte[size]; // Default to 128-bit buffer size
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes); // Fill the buffer with cryptographically secure random bytes
            }

            return Convert.ToBase64String(randomBytes); // Convert to base64 string for easy storage and handling
        }
    }

}

