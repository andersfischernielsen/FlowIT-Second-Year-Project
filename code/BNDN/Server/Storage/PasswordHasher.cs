using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Server.Storage
{
    public static class PasswordHasher
    {
        private static readonly int SaltValueSize = 4;
        private static readonly UnicodeEncoding Unicode = new UnicodeEncoding();
        private static readonly HashAlgorithm Hash = new SHA256Managed();
        public static string GenerateSaltValue()
        {
            // Create a random number object seeded from the value
            // of the last random seed value. This is done
            // interlocked because it is a static value and we want
            // it to roll forward safely.
            var random = new Random(unchecked((int)DateTime.Now.Ticks));

            // Create an array of random values.
            var saltValue = new byte[SaltValueSize * UnicodeEncoding.CharSize];

            random.NextBytes(saltValue);

            // Convert the salt value to a string. Note that the resulting string
            // will still be an array of binary values and not a printable string. 
            // Also it does not convert each byte to a double byte.
            var saltValueString = Unicode.GetString(saltValue);

            // Return the salt value as a string.
            return saltValueString;
        }

        public static string HashPassword(string clearData, string saltValue = null)
        {
            if (clearData == null || Hash == null) return null;
            // If the salt string is null or the length is invalid then
            // create a new valid salt value.

            if (saltValue == null)
            {
                // Generate a salt string.
                saltValue = GenerateSaltValue();
            }

            // Convert the salt string and the password string to a single
            // array of bytes. Note that the password string is Unicode and
            // therefore may or may not have a zero in every other byte.

            // var binarySaltValue = new byte[SaltValueSize];

            var binarySaltValue = Unicode.GetBytes(saltValue);

            /*for (var i = 0; i < SaltValueSize; i++)
            {
                binarySaltValue[i] = byte.Parse(saltValue.Substring(i*2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat);
            }*/

            //var valueToHash = new byte[SaltValueSize + Unicode.GetByteCount(clearData)];
            var binaryPassword = Unicode.GetBytes(clearData);

            // Copy the salt value and the password to the hash buffer.

            var valueToHash = binarySaltValue.Concat(binaryPassword).ToArray();

            var hashValue = Hash.ComputeHash(valueToHash);

            // The hashed password is the salt plus the hash value (as a string).

            var hashedPassword = new StringBuilder(saltValue);

            foreach (var hexdigit in hashValue)
            {
                hashedPassword.Append(hexdigit.ToString("X2", CultureInfo.InvariantCulture.NumberFormat));
            }

            // Return the hashed password as a string.

            return hashedPassword.ToString();
        }

        public static bool VerifyHashedPassword(string password, string profilePassword)
        {
            var saltLength = SaltValueSize;

            if (string.IsNullOrEmpty(profilePassword) ||
                string.IsNullOrEmpty(password) ||
                profilePassword.Length < saltLength)
            {
                return false;
            }

            // Strip the salt value off the front of the stored password.
            var saltValue = profilePassword.Substring(0, saltLength);

            var hashedPassword = HashPassword(password, saltValue);
            
            // If the hashedPassword matches the profilePassword return true.
            // Otherwise the password could not be verified..
            return profilePassword.Equals(hashedPassword, StringComparison.Ordinal);
        }
    }
}