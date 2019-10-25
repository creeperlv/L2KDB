using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using L2KDB.Server.Utils.IO;
using LiteDatabase.CustomedCryptography;

namespace L2KDB.Connector
{
    public class DBConnector
    {
        public static readonly Version ConnectorVersion = new Version(1, 0, 0, 0);
        public static DBConnector connector = new DBConnector();
        TcpClient tcpClient = new TcpClient();
        string sessionID = "";
        string usrname;
        string password;
        string aesKey;
        string aesIV;
        NetworkStream stream;
        StreamWriter Writer;
        CustomedAES aes;
        StreamReader Reader;
        bool isConnected = false;
        public void Disconnect()
        {
            try
            {
                stream.Dispose();
            }
            catch (Exception)
            { }
            try
            {
                Reader.Dispose();
            }
            catch (Exception)
            { }
            try
            {
                Writer.Dispose();
            }
            catch (Exception)
            { }
            try
            {
                tcpClient.Dispose();
            }
            catch (Exception)
            { }
        }
        public string Connect(string ip, int port, string usr, string pwd)
        {
            var ips = IPAddress.Parse(ip);
            var Endpoint = new IPEndPoint(ips, port);
            //tcpClient = new TcpClient(Endpoint);
            tcpClient.Connect(Endpoint);
            //tcpClient.Connect(ip, port);
            usrname = usr;
            password = pwd;
            stream = tcpClient.GetStream();
            Writer = new StreamWriter(stream);
            Reader = new StreamReader(stream);
            AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:OpenSession|{usr}|{pwd}");
            string content = AdvancedStream.ReadToCurrentEnd(ref Reader);
            var Responses = content.Split(',');
            if (Responses[0] == "L2KDB:Basic:ConnectionAccept")
            {
                sessionID = Responses[1];
                aes = new CustomedAES();
                
                aesKey = Responses[2];
                aesIV = Responses[3];
                aes.Key = aesKey;
                aes.IV = aesIV;
                isConnected = true;
                return "[S]";
            }
            else
            {
                return "[F]" + Responses[0];
            }
        }
        public string OpenDatabase(string name, string key, string iv)
        {
            if (isConnected == true)
            {
                AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:OpenDatabase,{name},{key},{iv}|{sessionID}",aes);
                string content = AdvancedStream.ReadToCurrentEnd(ref Reader,aes);
                if (content == "L2KDB:Basic:DatabaseOpen")
                {
                    return "[S]";
                }
                else if (content == "L2KDB:Basic:AccessForbidden")
                {
                    return "[F]Access Forbidden";
                }
                else if (content == "L2KDB:Basic:DatabaseNoFound")
                {
                    return "[F]NotFound";
                }
                else return content;
            }
            return "[F]Unconnected";
        }
        public string CreateDatabase(string name, string key, string iv)
        {
            if (isConnected == true)
            {
                AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:CreateDatabase,{name},{key},{iv}|{sessionID}",aes);
                string content = AdvancedStream.ReadToCurrentEnd(ref Reader,aes);
                if (content == "L2KDB:Basic:DatabaseCreated")
                {
                    return "[S]";
                }
                else if (content == "L2KDB:Basic:AccessForbidden")
                {
                    return "[F]Access Forbidden";
                }
                else if (content == "L2KDB:Basic:DatabaseNoFound")
                {
                    return "[F]NotFound";
                }
                else return content;
            }
            return "[F]Unconnected";
        }
        public string OpenForm(string name)
        {
            if (isConnected == true)
            {
                AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:OpenForm,{name}|{sessionID}",aes);
                string content = AdvancedStream.ReadToCurrentEnd(ref Reader,aes);
                if (content == "L2KDB:Basic:FromOpen")
                {
                    return "[S]";
                }
                else if (content == "L2KDB:Basic:UnknownError")
                {
                    return "[F]An Error Has Occurred On Server";
                }
                else
                if (content == "L2KDB:Basic:AccessForbidden")
                {
                    return "[F]Access Forbidden";
                }
            }
            return "[F]Unconnected";
        }
        public string  Save(string id1, string id2, string content)
        {
            if (isConnected == true)
            {
                {

                }
                AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:Save,{id1},{id2}|{sessionID}",content,aes);
                string result = AdvancedStream.ReadToCurrentEnd(ref Reader,aes);
                if (result == "L2KDB:Basic:OperationCompleted")
                {
                    return "[S]";
                }
                else if (result == "L2KDB:Basic:UnknownError")
                {
                    return "[F]An Error Has Occurred On Server";
                }
                else
                if (result == "L2KDB:Basic:AccessForbidden")
                {
                    return "[F]Access Forbidden";
                }
            }
            return "[F]Unconnected";
        }
        public List<string> GetForms()
        {
            List<string> vs = new List<string>();

            if (isConnected == true)
            {
                {
                    AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:GetForms|{sessionID}", aes);
                    var Command = aes.Decrypt(Reader.ReadLine());
                    var data = AdvancedStream.ReadToCurrentEnd(ref Reader, aes,false);
                    if (Command == "L2KDB:Basic:DatabaseGetFormsResult")
                    {
                        StringReader stringReader = new StringReader(data);
                        var temp = "";
                        while ((temp= stringReader.ReadLine())!=null)
                        {
                            vs.Add(temp);
                        }
                    }
                    else
                    {
                        throw new Exception("" + Command);
                    }
                }
                return vs;
            }
            throw new Exception("Unconnected");
        }
        public string Query(string id1, string id2, string content)
        {
            if (isConnected == true)
            {
                {
                    AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:Query,{id1},{id2}|{sessionID}", aes);
                    var Command = aes.Decrypt(Reader.ReadLine());
                    var data = AdvancedStream.ReadToCurrentEnd(ref Reader, aes, false);
                    if (Command == "L2KDB:Basic:DatabaseQueryResult")
                    {
                        return data;
                    }
                    else
                    {
                        throw new Exception(""+Command);
                    }
                }
            }
            throw new Exception("Unconnected");
        }
    }
}
