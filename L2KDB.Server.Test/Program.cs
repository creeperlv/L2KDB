using L2KDB.Server.Utils.IO;
using LiteDatabase.CustomedCryptography;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace L2KDB.Server.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 9341);
            var s = client.GetStream();
            StreamWriter streamWriter = new StreamWriter(s);
            {
                AdvancedStream.SendMessage(ref streamWriter, "L2KDB:Basic:OpenSession|Creeper Lv|123456\r\nL2KDB:Basic:EndOfCurrentTransmission");
                //streamWriter.Flush();

            }
            String SessionID = "";
            String Key = "";
            String IV = "";
            CustomedAES aes = new CustomedAES();
            StreamReader streamReader = new StreamReader(s);
            {
                string receive; receive = AdvancedStream.ReadToCurrentEnd(ref streamReader);
                Console.WriteLine(receive);
                var blocks = receive.Split(':');
                if (blocks[2].StartsWith("ConnectionAccept"))
                {
                    var c = blocks[2].Split(',');
                    SessionID = c[1];
                    Key = c[2];
                    IV = c[3];
                    aes.Key = Key;
                    aes.IV = IV;
                    Console.WriteLine($"Obtain:{SessionID}\t{Key}\t{IV}");
                    {

                        AdvancedStream.SendMessage(ref streamWriter, "L2KDB:Basic:GetDatabaseVersion|" + SessionID, aes);
                        Console.WriteLine(AdvancedStream.ReadToCurrentEnd(ref streamReader, aes));
                    }
                    {

                        AdvancedStream.SendMessage(ref streamWriter, "L2KDB:Basic:OpenDatabase,TestDataBase,,|"+SessionID, aes);
                        Console.WriteLine(AdvancedStream.ReadToCurrentEnd(ref streamReader, aes));
                    }
                }
                else
                {

                }
            }
            //Console.WriteLine("Hello World!");
        }
    }
}
