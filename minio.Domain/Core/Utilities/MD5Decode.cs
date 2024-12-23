using System;
using System.Security.Cryptography;
using System.Text;

namespace minio.Domain.Core.Utilities
{
    public static class MD5Decode
    {
        public static string Md5Decode(object source)
        {
            if (source == null || string.IsNullOrEmpty(source.ToString()))
                throw new ArgumentException("Object is empty");

            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(source.ToString());
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var t in hashBytes)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}


