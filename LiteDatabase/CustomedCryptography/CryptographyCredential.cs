using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDatabase.CustomedCryptography
{
    public enum CryptographtType 
    {
        None,AES
    }
    public class CryptographyCredential
    {
        public CryptographtType CryptographtType { get; set; } = CryptographtType.None;
        public string Key { get; set; } = "";
        public string IV { get; set; } = "";

        public CryptographyCredential()
        {

        }
        public CryptographyCredential(CryptographtType type,string key,string iv)
        {
            CryptographtType = type;
            Key = key;
            IV = iv;
        }
    }
}
