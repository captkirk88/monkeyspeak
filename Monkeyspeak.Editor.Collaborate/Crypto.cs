using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Collaborate
{
    internal static class Crypto
    {
        private static byte[] defaultIV = null;

        public static string EncryptString(string plainText, string key, out string iv)
        {
            if (key.Length > 24) throw new InvalidOperationException("Key length must be 24 characters or less");

            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Key = Convert.FromBase64String(key);
            rijndaelCipher.GenerateIV();
            iv = Convert.ToBase64String(rijndaelCipher.IV);

            MemoryStream memoryStream = new MemoryStream();
            using (ICryptoTransform rijndaelEncryptor = rijndaelCipher.CreateEncryptor())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelEncryptor, CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                cryptoStream.Write(plainBytes, 0, plainBytes.Length);

                cryptoStream.FlushFinalBlock();
            }

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        public static string EncryptString(string plainText, string key)
        {
            if (key.Length > 24) throw new InvalidOperationException("Key length must be 24 characters or less");

            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Key = Convert.FromBase64String(key);

            if (defaultIV == null)
            {
                rijndaelCipher.GenerateIV();
                defaultIV = rijndaelCipher.IV;
            }

            MemoryStream memoryStream = new MemoryStream();
            using (ICryptoTransform rijndaelEncryptor = rijndaelCipher.CreateEncryptor())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelEncryptor, CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                cryptoStream.Write(plainBytes, 0, plainBytes.Length);

                cryptoStream.FlushFinalBlock();
            }

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        public static string DecryptString(string cipherText, string key, string iv = null)
        {
            if (key.Length > 24) throw new InvalidOperationException("Key length must be 24 characters or less");

            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Key = Convert.FromBase64String(key);
            if (string.IsNullOrEmpty(iv) && defaultIV != null)
            {
                rijndaelCipher.IV = defaultIV;
            }
            else rijndaelCipher.IV = Convert.FromBase64String(iv);

            using (MemoryStream memoryStream = new MemoryStream())
            using (ICryptoTransform rijndaelDecryptor = rijndaelCipher.CreateDecryptor())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelDecryptor, CryptoStreamMode.Write))
            {
                string plainText = String.Empty;

                try
                {
                    byte[] cipherBytes = Encoding.UTF8.GetBytes(cipherText);

                    cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                    cryptoStream.FlushFinalBlock();

                    return Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                finally
                {
                }
            }
        }

        public static string Xor(string text, string key)
        {
            var result = new StringBuilder();

            for (int c = 0; c < text.Length; c++)
                result.Append((char)((uint)text[c] ^ (uint)key[c % key.Length]));

            return result.ToString();
        }
    }
}