using System.Security.Cryptography;
using System.Text;
using System;

namespace SlothFlyingWeb.Utils
{
    public class Md5
    {
        private const string SALT = "Sl0thF1yiNgWe6";
        public static string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                string salt = Environment.GetEnvironmentVariable("SALT_MD5") ?? SALT; 
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input + salt);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}