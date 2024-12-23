using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace minio.Domain.Core.Utilities
{
    public class EncryptUtils
    {
        private static readonly ILogger<EncryptUtils> _logger;

        public static string EncryptString(object plainText, string secret)
        {
            try
            {
                var iv = new byte[16];
                using var aes = Aes.Create();
                aes.Key = GetSha256(secret);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                var plainTextBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(plainText));
                var encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        public static string DecryptString(object encryptedText, string secret)
        {
            try
            {
                var iv = new byte[16];
                var buffer = Convert.FromBase64String(encryptedText.ToString());
                using var aes = Aes.Create();
                aes.Key = GetSha256(secret);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var decryptedBytes = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        private static byte[] GetSha256(string secret)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(secret));
        }

        public static string EncryptResponse(object plaintext)
        {
            return EncryptString(plaintext, GetEncryptResponseSecret());
        }

        public static string DecryptResponse(object plaintext)
        {
            return DecryptString(plaintext, GetEncryptResponseSecret());
        }

        private static string GetEncryptResponseSecret()
        {
            var calendar = DateTime.UtcNow;
            calendar = calendar.AddSeconds(-calendar.Second).AddMilliseconds(-calendar.Millisecond).AddMinutes(1);
            return calendar.Ticks.ToString();
        }
    }
}


