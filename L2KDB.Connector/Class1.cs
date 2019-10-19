using System;
using System.IO;
using System.Net.Sockets;
using L2KDB.Server.Utils.IO;

namespace L2KDB.Connector
{
    public class DBConnector
    {
        public static DBConnector connector = new DBConnector();
        TcpClient tcpClient;
        string sessionID = "";
        string usrname;
        string password;
        string aesKey;
        string aesIV;
        NetworkStream stream;
        StreamWriter Writer;
        StreamReader Reader;
        bool isConnected = false;
        public bool Connect(string ip, int port, string usr, string pwd)
        {
            tcpClient.Connect(ip, port);
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
                aesKey = Responses[2];
                aesIV = Responses[3];
                isConnected = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        public string OpenDatabase(string name, string key, string iv)
        {
            if (isConnected == true)
            {
                AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:OpenDatabase,{name},{key},{iv}|{sessionID}");
                string content = AdvancedStream.ReadToCurrentEnd(ref Reader);
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
            }
            return "[F]Unconnected";
        }
        public string OpenForm(string name)
        {
            if (isConnected == true)
            {
                AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:OpenForm,{name}|{sessionID}");
                string content = AdvancedStream.ReadToCurrentEnd(ref Reader);
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
        public bool Save(string id1, string id2, string content)
        {
            return false;
        }
    }
}
