using System;
using System.Security.Cryptography;

namespace BudgetTracker.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            // Create a random salt (16 bytes)
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt); // Fill salt array with random bytes

                // Generate a PBKDF2 hash using SHA256 (32 bytes) with 100,000 iterations
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(32); // 32 bytes hash

                // Create an array to store both salt (16 bytes) and hash (32 bytes) = 48 bytes total
                byte[] hashBytes = new byte[48];

                Array.Copy(salt, 0, hashBytes, 0, 16); // Copy the salt into the first 16 bytes
                Array.Copy(hash, 0, hashBytes, 16, 32); // Copy the hash into the next 32 bytes

                // Return the combined salt+hash as a base64 string
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Convert the stored base64 string to a byte array
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // The salt is the first 16 bytes of the hashBytes
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // The salt is the first 16 bytes of the hashBytes
            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32); // Use the same size as your stored hash (20 bytes)

            // Compare the computed hash (from enteredPassword) with the stored hash (in hashBytes)
            for (int i = 0; i < 32; i++) // Length of the hash is 20 bytes
            {
                if (hashBytes[i + 16] != hash[i]) // Compare corresponding bytes
                {
                    return false; // Hashes don't match
                }
            }

            return true;
        }
    }
}