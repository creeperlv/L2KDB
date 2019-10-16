using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

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
                int r = 0;
                while ((r=rnd.Next(48, 122))==58)
                {

                }
                num.Append((char)r);
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
        public string Key { get; set; }
        public string IV  { get; set; }
        public byte[] GenerateFromString(string str)
        {
            List<byte> vs = new List<byte>();
            foreach (var item in str)
            {
                vs.Add((byte)item);
            }
            return vs.ToArray();
        }
        public string Encrypt(string content)
        {
            var key= GenerateFromString(Key);
            var iv= GenerateFromString(IV);
            byte[] DATA = Encoding.UTF8.GetBytes(content);
            var encryptor= AESEncrypt(DATA,key, iv);
            if (encryptor == null)
            {
                return null;
            }
            return Convert.ToBase64String(encryptor);
        }
        public string Decrypt(string content)
        {
            var key = GenerateFromString(Key);
            var iv = GenerateFromString(IV);
            byte[] DATA = Convert.FromBase64String(content);
            var result = AESDecrypt(DATA, key, iv);
            if (result == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(result);
        }
        public static byte[] AESEncrypt(byte[] data, byte[] key, byte[] vector)
        {

            byte[] bytes = data;

            byte[] encryptData = null; // encrypted data
            using (Aes Aes = Aes.Create())
            {
                try
                {
                    using (MemoryStream Memory = new MemoryStream())
                    {
                        using (CryptoStream Encryptor = new CryptoStream(Memory,
                         Aes.CreateEncryptor(key, vector),
                         CryptoStreamMode.Write))
                        {
                            Encryptor.Write(bytes, 0, bytes.Length);
                            Encryptor.FlushFinalBlock();

                            encryptData = Memory.ToArray();
                        }
                    }
                }
                catch
                {
                    encryptData = null;
                }
                return encryptData;
            }
        }
            public static byte[] AESDecrypt(byte[] data, byte[] key, byte[] vector)
            {

                byte[] encryptedBytes = data;

                byte[] decryptedData = null; // decrypted data

                using (Aes Aes = Aes.Create())
                {
                    try
                    {
                        using (MemoryStream Memory = new MemoryStream(encryptedBytes))
                        {
                            using (CryptoStream Decryptor = new CryptoStream(Memory, Aes.CreateDecryptor(key, vector), CryptoStreamMode.Read))
                            {
                                using (MemoryStream tempMemory = new MemoryStream())
                                {
                                    byte[] Buffer = new byte[1024];
                                    Int32 readBytes = 0;
                                    while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
                                    {
                                        tempMemory.Write(Buffer, 0, readBytes);
                                    }

                                    decryptedData = tempMemory.ToArray();
                                }
                            }
                        }
                    }
                    catch
                    {
                        decryptedData = null;
                    }

                    return decryptedData;
                }
            }
        
    }
}
