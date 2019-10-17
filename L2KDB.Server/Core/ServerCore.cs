using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using L2KDB.Server.Core.CommandSets;
using LiteDatabase;
using System.Threading.Tasks;
using L2KDB.Server.Utils.IO;

namespace L2KDB.Server.Core
{
    public class ServerCore
    {
        void InitBasicCommands()
        {
            {
                //Init
                BasicCommandSet.Functions.Add("OpenDatabase", (List<string> para, string content, Session session) =>
                {
                    string calculatedID = "";
                    {
                        calculatedID = Authentication.ObtainDBID(para[0], para[1], para[2]);
                    }
                    foreach (var item in VariablesPool.Databases)
                    {
                        if (item.GeneratedID == calculatedID)
                        {
                            if (FindPermission(session.AuthID, "Database->" + para[0]))
                            {
                                session.operatingBD = item;
                                return "L2KDB:Basic:DatabaseOpen";
                            }
                            else if (FindPermission(session.AuthID, "FullDBAccess"))
                            {
                                session.operatingBD = item;
                                return "L2KDB:Basic:DatabaseOpen";
                            }
                            else
                            {
                                return "L2KDB:Basic:AccessForbidden";
                            }
                        }
                    }
                    if (Directory.Exists("./Databases/" + para[0]))
                    {

                        if (FindPermission(session.AuthID, "Database->" + para[0]))
                        {
                            //session.operatingBD = item;
                            Database database = new Database("./Databases/" + para[0]);
                            VariablesPool.Databases.Add(database);
                            database.GeneratedID = calculatedID;
                            session.operatingBD = database;
                            return "L2KDB:Basic:DatabaseOpen";
                        }
                        else if (FindPermission(session.AuthID, "FullDBAccess"))
                        {
                            Database database = new Database("./Databases/" + para[0]);
                            VariablesPool.Databases.Add(database);
                            database.GeneratedID = calculatedID;
                            session.operatingBD = database;
                            return "L2KDB:Basic:DatabaseOpen";
                        }
                        else
                        {
                            return "L2KDB:Basic:AccessForbidden";
                        }
                    }
                    else
                    {
                        return "L2KDB:Basic:DatabaseNoFound";
                    }
                });
                BasicCommandSet.Functions.Add("OpenForm", (List<string> para, string content, Session session) => { return ""; });
                BasicCommandSet.Functions.Add("Query", (List<string> para, string content, Session session) => { return ""; });
                BasicCommandSet.Functions.Add("Save", (List<string> para, string content, Session session) => { return ""; });
                BasicCommandSet.Functions.Add("DeleteID2", (List<string> para, string content, Session session) => { return ""; });
                BasicCommandSet.Functions.Add("DeleteID1", (List<string> para, string content, Session session) => { return ""; });
                BasicCommandSet.Functions.Add("DeleteForm", (List<string> para, string content, Session session) => { return ""; });
                BasicCommandSet.Functions.Add("GetDatabaseVersion", (List<string> para, string content, Session session) =>
                {
                    return Database.DatabaseVersion.ToString();
                });
            }
        }
        static Database ServerConfig = new Database("./Server-Config/", DatabaseMode.Cache);
        public ServerCore()
        {
            InitBasicCommands();
        }
        public static bool FindPermission(string AuthID, string PermissionID)
        {
            try
            {
                return bool.Parse((string)ServerConfig.Query(AuthID, PermissionID));
            }
            catch (Exception)
            {
            }
            return false;
        }
        public static List<Session> SessionPool = new List<Session>();
        TcpListener Listener;
        public void Start()
        {
            string ipadd = "0.0.0.0";
            int port = 9341;
            {
                //Load Config
                ServerConfig.OpenForm("Server");
                try
                {
                    ipadd = (string)ServerConfig.Query("BasicConfig", "IP");
                }
                catch (Exception)
                {
                }
                try
                {

                    port = int.Parse((string)ServerConfig.Query("BasicConfig", "Port"));

                }
                catch (Exception)
                {
                }
            }
            SessionPool.Clear();
            ServerConfig.OpenForm("Permissions");
            var ip = IPAddress.Parse(ipadd);
            var Endpoint = new IPEndPoint(ip, port);
            Listener = new TcpListener(Endpoint);
            Listener.Start();
            Listen();
        }
        bool stopFlag = false;
        private void ProcessHandshake(TcpClient request)
        {
            var stream = request.GetStream();
            StreamReader streamReader = new StreamReader(stream);
            var str = AdvancedStream.ReadToCurrentEnd(ref streamReader);
            var req = str.Split('|');
            if (req.Length > 2)
            {
                if (req[0] == "L2KDB:Basic:OpenSession")
                {
                    //Example
                    //L2KDB:Basic:OpenSession|CreeperLv|123456
                    Session session = new Session(request, streamReader, stream) { AuthID = Authentication.ObtainID(req[1], req[2]) };
                    SessionPool.Add(session);
                }
            }
            else
            {
                StreamWriter streamWriter = new StreamWriter(stream);
                streamWriter.Write("HTTP/1.1 200 OK\r\nServer: L2KDB\r\nContent-Encoding: gzip\r\nContent-Type: text/html;charset=UTF-8\r\n\r\n<!DOCTYPE html>\r\n<html><head><title>L2KDB</title></head><body><p>Wrong Handshaking Protocol.</p></html>");
                Console.WriteLine("Sending Refuse.");
                streamWriter.Flush();
                streamWriter.Close();
            }
        }
        private void Listen()
        {
            while (stopFlag != true)
            {
                Console.WriteLine("Waiting for shaking hand.");
                var request = Listener.AcceptTcpClient();
                var r = Task.Run(() => { ProcessHandshake(request); });

            }
        }
    }
}
