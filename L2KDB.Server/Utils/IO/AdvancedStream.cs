using LiteDatabase.CustomedCryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace L2KDB.Server.Utils.IO
{
    public class AdvancedStream
    {
        public static string ReadToCurrentEnd(ref StreamReader streamReader)
        {
            string content = "";
            string tmp;
            while (true)
            {
                try
                {
                    tmp = streamReader.ReadLine();
                    if (tmp == "")
                    {
                        break;
                    }
                    if (tmp == "L2KDB:Basic:EndOfCurrentTransmission") break;
                    if (content == "")
                    {
                        content += tmp;
                    }
                    else
                    {
                        content += Environment.NewLine + tmp;
                    }
                    if (content.StartsWith("L2KDB:"))
                    {
                    }
                    else
                    {
                        return "WRONG HEADER";
                    }

                }
                catch (Exception)
                {
                    break;
                }
            }
            Console.WriteLine("End.");
            return content;
        }
        public static string ReadToCurrentEnd(ref StreamReader streamReader,CustomedAES aes)
        {
            string content = "";
            string tmp;
            while (true)
            {
                try
                {
                    tmp = aes.Decrypt(streamReader.ReadLine());
                    if (tmp == "")
                    {
                        break;
                    }
                    if (tmp == "L2KDB:Basic:EndOfCurrentTransmission") break;
                    if (content == "")
                    {
                        content += tmp;
                    }
                    else
                    {
                        content += Environment.NewLine + tmp;
                    }
                    if (content.StartsWith("L2KDB:"))
                    {
                    }
                    else
                    {
                        return "WRONG HEADER";
                    }

                }
                catch (Exception)
                {
                    break;
                }
            }
            return content;
        }
        public static void SendMessage(ref StreamWriter streamWriter,string content)
        {
            streamWriter.WriteLine(content + streamWriter.NewLine + "L2KDB:Basic:EndOfCurrentTransmission");
            streamWriter.Flush();
        }public static void SendMessage(ref StreamWriter streamWriter,string content,CustomedAES aes)
        {
            string res = aes.Encrypt(content);

            streamWriter.WriteLine(res+ streamWriter.NewLine + aes.Encrypt("L2KDB:Basic:EndOfCurrentTransmission"));
            streamWriter.Flush();
        }
    }
}
