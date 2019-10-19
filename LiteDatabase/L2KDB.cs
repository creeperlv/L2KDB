using LiteDatabase;
using LiteDatabase.CustomedCryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace L2KDB.Core
{
    public struct DatabaseDescription
    {
        public bool isPublic;
    }
    public class Database
    {
        public LiteDatabase.Database realDB;
        public DatabaseDescription Description=new DatabaseDescription();
        public static Database CreateDatabase(string name, DatabaseMode loadMode = DatabaseMode.OnDemand, CryptographyCredential credential = null, bool isPublic = false)
        {
            Database database = new Database();
            DatabaseDescription description = new DatabaseDescription();
            description.isPublic = isPublic;
            if (!Directory.Exists(name))
            {
                Directory.CreateDirectory(name);
            }
            SaveDescription(description, Path.Combine(name, "L2KDB-Description"), credential);
            database.realDB = new LiteDatabase.Database(name, loadMode, credential);
            return database;
        }
        public static Database OpenDatabase(string name, DatabaseMode loadMode = DatabaseMode.OnDemand, CryptographyCredential credential = null)
        {
            Database database = new Database();

            try
            {
                var descption = OpenDescription(Path.Combine(name, "L2KDB-Description"), credential);
                database.Description = descption;
            }
            catch (Exception)
            {
                throw new Exception("WRONG-AES");
            }
            database.realDB = new LiteDatabase.Database(name, loadMode, credential);
            return database;
        }
        private static DatabaseDescription OpenDescription(string location, CryptographyCredential credential)
        {
            DatabaseDescription description = new DatabaseDescription();
            if (credential == null)
            {
                var c = File.ReadAllText(location);
                var l=c.Split(':');
                description.isPublic = bool.Parse(l[1]);
            }
            else if (credential.Key == "")
            {

                var c = File.ReadAllText(location);
                var l=c.Split(':');
                description.isPublic = bool.Parse(l[1]);
            }
            else
            {

                var c = File.ReadAllText(location);
                CustomedAES aes = new CustomedAES();
                aes.Key = credential.Key;
                aes.IV = credential.IV;
                var l = (aes.Decrypt(c)).Split(':');
                description.isPublic = bool.Parse(l[1]);
            }
            return new DatabaseDescription();
        }
        private static void SaveDescription(DatabaseDescription description,string location, CryptographyCredential credential)
        {


            if (credential == null)
            {
                File.WriteAllText(location,"PUBLIC:"+description.isPublic);
            }
            else if (credential.Key == "")
            {
                File.WriteAllText(location, "PUBLIC:" + description.isPublic);
            }
            else
            {
                CustomedAES aes = new CustomedAES();
                aes.Key = credential.Key;
                aes.IV = credential.IV;
                File.WriteAllText(location, aes.Encrypt("PUBLIC:" + description.isPublic));
            }
        }
    }
}
