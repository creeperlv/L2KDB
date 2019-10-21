using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using L2KDB.Server.Core.CommandSets;
using L2KDB.Core;
using System.Threading.Tasks;
using L2KDB.Server.Utils.IO;
using LiteDatabase.CustomedCryptography;

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
                        if (item.realDB.GeneratedID == calculatedID)
                        {
                            if (FindPermission(session.AuthID, "Database->" + para[0]))
                            {
                                session.DatabaseAES = new CustomedAES();
                                session.DatabaseAES.Key = para[1];
                                session.DatabaseAES.IV = para[2];
                                session.operatingBD = item;
                                return "L2KDB:Basic:DatabaseOpen";
                            }
                            else if (FindPermission(session.AuthID, "FullDBAccess"))
                            {
                                session.DatabaseAES = new CustomedAES();
                                session.DatabaseAES.Key = para[1];
                                session.DatabaseAES.IV = para[2];
                                session.operatingBD = item;
                                return "L2KDB:Basic:DatabaseOpen";
                            }
                            else
                            {
                                return "L2KDB:Basic:AccessForbidden";
                            }
                        }
                    }
                    if (Directory.Exists(BasePath + para[0]))
                    {

                        if (FindPermission(session.AuthID, "Database->" + para[0]))
                        {
                            //session.operatingBD = item;if (para[1] != "")
                            CryptographyCredential cc = null;
                            if (para[1] != "")
                            {
                                cc = new CryptographyCredential(CryptographtType.AES, para[1], para[2]);
                            }
                            Database database = Database.OpenDatabase(BasePath + para[0], credential: cc);
                            VariablesPool.Databases.Add(database);
                            database.realDB.GeneratedID = calculatedID;
                            session.operatingBD = database;
                            session.DatabaseAES = new CustomedAES();
                            session.DatabaseAES.Key = para[1];
                            session.DatabaseAES.IV = para[2];

                            return "L2KDB:Basic:DatabaseOpen";
                        }
                        else if (FindPermission(session.AuthID, "FullDBAccess"))
                        {
                            CryptographyCredential cc = null;
                            if (para[1] != "")
                            {
                                cc = new CryptographyCredential(CryptographtType.AES, para[1], para[2]);
                            }
                            Database database = Database.OpenDatabase(BasePath + para[0], credential: cc);
                            VariablesPool.Databases.Add(database);
                            database.realDB.GeneratedID = calculatedID;
                            session.operatingBD = database;
                            session.DatabaseAES = new CustomedAES();
                            session.DatabaseAES.Key = para[1];
                            session.DatabaseAES.IV = para[2];
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
                BasicCommandSet.Functions.Add("OpenForm", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session.AuthID, "FullDBAccess") || FindPermission(session.AuthID, "Database->" + session.operatingBD.realDB.HomeDirectory.Name))
                    {
                        try
                        {
                            session.operatingBD.realDB.OpenForm(para[0]);
                        }
                        catch (Exception)
                        {
                            return "L2KDB:Basic:UnknownError";
                        }
                        return "L2KDB:Basic:FromOpen";
                    }
                    else
                    {

                        return "L2KDB:Basic:AccessForbidden";
                    }
                });
                BasicCommandSet.Functions.Add("CreateDatabase", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session.AuthID, "FullDBAccess") || FindPermission(session.AuthID, "CreateDatabase"))
                    {
                        //L2KDB:Basic:CreateDatabase,Name,Key,IV
                        try
                        {
                            bool willPub=false;
                            try
                            {
                                if (para[3].ToUpper() == "PUBLIC")
                                {
                                    willPub = true;
                                }
                            }
                            catch (Exception)
                            {
                            }
                            Database.CreateDatabase(Path.Combine(BasePath, para[0]), credential: new CryptographyCredential() { Key = para[1], IV = para[2] }, isPublic: willPub);
                            //Directory.CreateDirectory(BasePath + para[0]);
                        }
                        catch (Exception)
                        {
                            return "L2KDB:Basic:ErrorOnDatabaseCreation";
                        }
                        return "L2KDB:Basic:DatabaseCreated";
                    }
                    else
                    {

                        return "L2KDB:Basic:AccessForbidden";
                    }
                });
                BasicCommandSet.Functions.Add("Query", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session.AuthID, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session.AuthID, "FullDBAccess"))
                    {
                        var data = session.operatingBD.realDB.Query(para[0], para[1]);
                        
                        session.SendData("L2KDB:Basic:DatabaseQueryResult" , ""+data);
                    }
                    return "";
                });
                BasicCommandSet.Functions.Add("Save", (List<string> para, string content, Session session) =>
                {
                    //L2KDB:Basic:Save,|[Session]|
                    if (FindPermission(session.AuthID, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session.AuthID, "FullDBAccess"))
                    {
                        session.operatingBD.realDB.Save(para[0], para[1], content);
                        return "L2KDB:Basic:OperationCompleted";
                    }
                    return "L2KDB:Basic:AccessForbidden";
                });
                BasicCommandSet.Functions.Add("DeleteID2", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session.AuthID, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session.AuthID, "FullDBAccess"))
                    {
                        try
                        {

                            session.operatingBD.realDB.Remove(para[0], para[1]);
                            return "L2KDB:Basic:OperationCompleted";
                        }
                        catch (Exception)
                        {
                            return "L2KDB:Basic:OperationError";
                        }
                    }
                    return "L2KDB:Basic:AccessForbidden";
                });
                BasicCommandSet.Functions.Add("DeleteID1", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session.AuthID, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session.AuthID, "FullDBAccess"))
                    {
                        try
                        {

                            session.operatingBD.realDB.RemoveID1(para[0]);
                            return "L2KDB:Basic:OperationCompleted";
                        }
                        catch (Exception)
                        {
                            return "L2KDB:Basic:OperationError";
                        }
                    }
                    return "L2KDB:Basic:AccessForbidden";
                });
                BasicCommandSet.Functions.Add("DeleteForm", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session.AuthID, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session.AuthID, "FullDBAccess"))
                    {
                        try
                        {

                            session.operatingBD.realDB.DeleteForm(para[0]);
                            return "L2KDB:Basic:OperationCompleted";
                        }
                        catch (Exception)
                        {
                            return "L2KDB:Basic:OperationError";
                        }
                    }
                    return "L2KDB:Basic:AccessForbidden";
                });
                BasicCommandSet.Functions.Add("GetDatabaseVersion", (List<string> para, string content, Session session) =>
                {
                    return LiteDatabase.Database.DatabaseVersion.ToString();
                });
            }
        }
        public string BasePath="./Databasee/";
        void InitAdminCommands()
        {
            {
                //Init
                AdminCommandSet.Functions.Add("OpenForm", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session.AuthID, "AdminAccess"))
                    {
                        try
                        {
                            ServerConfig.OpenForm(para[0]);
                        }
                        catch (Exception)
                        {
                            return "L2KDB:Basic:ErrorOnDatabaseCreation";
                        }
                        return "L2KDB:Basic:DatabaseCreated";
                    }
                    else
                    {

                        return "L2KDB:Basic:AccessForbidden";
                    }
                });
                AdminCommandSet.Functions.Add("Query", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session.AuthID, "AdminAccess"))
                    {
                        var data = ServerConfig.Query(para[0], para[1]);
                        session.SendData("L2KDB:Basic:DatabaseQueryResult\r\n" + data);
                    }
                    return "";
                });
                AdminCommandSet.Functions.Add("Save", (List<string> para, string content, Session session) =>
                {
                    //L2KDB:Basic:Save,|[Session]|
                    if (FindPermission(session.AuthID, "AdminAccess"))
                    {
                        ServerConfig.Save(para[0], para[1], content);
                    }
                    return "L2KDB:Basic:AccessForbidden";
                });
            }
        }
        static LiteDatabase.Database ServerConfig = new LiteDatabase.Database("./Server-Config/", LiteDatabase.DatabaseMode.Cache);
        public ServerCore(string BasePath="./Databases/",string SeverConfigPath="./Server-Config/")
        {
            this.BasePath = BasePath;
            ServerConfig = new LiteDatabase.Database(SeverConfigPath, LiteDatabase.DatabaseMode.Cache);
            InitBasicCommands();
            InitAdminCommands();
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
            //ServerConfig.Save("WRM9Sh02BjJBLl4VOjULUQ==", "FullDBAccess", "" + true);
            //ServerConfig.Save("WRM9Sh02BjJBLl4VOjULUQ==", "AdminAccess", "" + true);
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
                    Session session = new Session(request, streamReader, stream, Authentication.ObtainID(req[1], req[2])) { };
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
