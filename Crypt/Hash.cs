using System;
using System.Security.Cryptography;
using System.Text;

namespace penguin_new.Crypt
{
    public static class Hash
    {
        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string GetNumericHash(string inputString)
        {
            return BitConverter.ToInt32(GetHash(inputString), 0).ToString();
        }
    }
}
