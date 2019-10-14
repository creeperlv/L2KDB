using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
namespace LiteDatabase.CustomedCryptography
{
    public class CustomedAES
    {
        public static string GetRandomString(int length)
        {

            StringBuilder num = new StringBuilder();

            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < length; i++)
            {
                num.Append((char)rnd.Next(32, 126));
            }
            //32~126

            return num.ToString();
        }
        public static string GenerateKey()
        {
            return GetRandomString(32);
        }
        public static string GenerateIV()
        {
            return GetRandomString(16);
        }
        public CustomedAES()
        {

        }

        public string Encrypt(string content)
        {
            Aes aes = Aes.Create();
            return null;
        }
    }
}
