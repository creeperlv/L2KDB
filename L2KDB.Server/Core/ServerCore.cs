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
using L2KDB.Server.Diagnostic;

namespace L2KDB.Server.Core
{
    public class ServerCore
    {
        public readonly Version CoreVersion = new Version(1, 0, 1, 0);
        public bool CheckName(string n)
        {
            if(n == "" ||n == "/" || n == "\\" || n == "." || n == ".."|| n.IndexOf("/")>=0|| n.IndexOf("\\")>=0|| n.IndexOf(".")==0)
            {
                return false;
            }
            return true;
        }
        public void SetAdmin(string usr,string pwd)
        {
            ServerConfig.Save(Authentication.ObtainID(usr,pwd),"AdminAccess",""+true);
        }
        public void SetPermission(string AuthID,string PermissionName,string content)
        {
            ServerConfig.Save(AuthID, PermissionName, ""+content);
        }
        public void RemoveAdmin(string usr,string pwd)
        {
            ServerConfig.Save(Authentication.ObtainID(usr,pwd),"AdminAccess",""+false);
        }
        void InitBasicCommands()
        {
            {
                //Init
                BasicCommandSet.Functions.Add("OpenDatabase", (List<string> para, string content, Session session) =>
                {
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    string calculatedID = "";
                    {
                        calculatedID = Authentication.ObtainDBID(para[0], para[1], para[2]);
                    }
                    foreach (var item in VariablesPool.Databases)
                    {
                        if (item.realDB.GeneratedID == calculatedID)
                        {
                            if (FindPermission(session, "Database->" + para[0]))
                            {
                                session.DatabaseAES = new CustomedAES();
                                session.DatabaseAES.Key = para[1];
                                session.DatabaseAES.IV = para[2];
                                session.operatingBD = item;
                                return "L2KDB:Basic:DatabaseOpen";
                            }
                            else if (FindPermission(session, "FullDBAccess"))
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

                        if (FindPermission(session, "Database->" + para[0]))
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
                        else if (FindPermission(session, "FullDBAccess"))
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
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    if (FindPermission(session, "FullDBAccess") || FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name))
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
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    if (FindPermission(session, "FullDBAccess") || FindPermission(session, "CreateDatabase"))
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
                            SetPermission(session.AuthID, "Database->" + para[0], "" + true);
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
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    if (FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session, "FullDBAccess"))
                    {
                        var data = session.operatingBD.realDB.Query(para[0], para[1]);
                        
                        session.SendData("L2KDB:Basic:DatabaseQueryResult" , ""+data);
                        return "-1";
                    }
                    return "L2KDN:Basic:UnknownError";
                });
                BasicCommandSet.Functions.Add("GetForms", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session, "FullDBAccess"))
                    {
                        var data = session.operatingBD.realDB.GetForms();
                        var combineD = "";
                        foreach (var item in data)
                        {
                            //Diagnotor.CurrentDiagnotor.Log("Form:"+combineD);
                            if (combineD == "")
                            {
                                combineD = item;
                            }
                            else
                            {
                                combineD += Environment.NewLine + item;
                            }
                        }
                        if (combineD == "")
                        {
                            combineD = "L2KDB:Basic:NoForms";
                        }
                        session.SendData("L2KDB:Basic:DatabaseGetFormsResult" , ""+ combineD);
                        return "-1";
                    }else
                    return "L2KDB:Basic:AccessForbidden";
                });
                BasicCommandSet.Functions.Add("GetID1", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session, "FullDBAccess"))
                    {
                        var data = session.operatingBD.realDB.GetID1();
                        var combineD = "";
                        foreach (var item in data)
                        {
                            //Diagnotor.CurrentDiagnotor.Log("Form:"+combineD);
                            if (combineD == "")
                            {
                                combineD = item;
                            }
                            else
                            {
                                combineD += Environment.NewLine + item;
                            }
                        }
                        if (combineD == "")
                        {
                            combineD = "L2KDB:Basic:NoID1";
                        }
                        session.SendData("L2KDB:Basic:DatabaseGetID1Result" , ""+ combineD);
                        return "-1";
                    }else
                    return "L2KDB:Basic:AccessForbidden";
                });
                BasicCommandSet.Functions.Add("GetID2", (List<string> para, string content, Session session) =>
                {
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    if (FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session, "FullDBAccess"))
                    {
                        var data = session.operatingBD.realDB.GetID2(para[0]);
                        var combineD = "";
                        foreach (var item in data)
                        {
                            //Diagnotor.CurrentDiagnotor.Log("Form:"+combineD);
                            if (combineD == "")
                            {
                                combineD = item;
                            }
                            else
                            {
                                combineD += Environment.NewLine + item;
                            }
                        }
                        if (combineD == "")
                        {
                            combineD = "L2KDB:Basic:NoID2";
                        }
                        session.SendData("L2KDB:Basic:DatabaseGetID2Result" , ""+ combineD);
                        return "-1";
                    }else
                    return "L2KDB:Basic:AccessForbidden";
                });
                BasicCommandSet.Functions.Add("Save", (List<string> para, string content, Session session) =>
                {
                    //L2KDB:Basic:Save,|[Session]|
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    if (FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session, "FullDBAccess"))
                    {
                        session.operatingBD.realDB.Save(para[0], para[1], content);
                        return "L2KDB:Basic:OperationCompleted";
                    }
                    return "L2KDB:Basic:AccessForbidden";
                });
                BasicCommandSet.Functions.Add("DeleteID2", (List<string> para, string content, Session session) =>
                {
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    if (FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session, "FullDBAccess"))
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
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    if (FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session, "FullDBAccess"))
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
                    if (CheckName(para[0]) == false)
                    {
                        return "L2KDB:Basic:IllegalName";
                    }
                    if (FindPermission(session, "Database->" + session.operatingBD.realDB.HomeDirectory.Name) || FindPermission(session, "FullDBAccess"))
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

        public void SetPort(int v)
        {
            ServerConfig.OpenForm("Server");
            ServerConfig.Save("BasicConfig","Port",""+v);
        }
        public void SetIP(string s)
        {
            ServerConfig.OpenForm("Server");
            ServerConfig.Save("BasicConfig","IP",s);
        }

        public string BasePath="./Databasee/";
        void InitAdminCommands()
        {
            {
                //Init
                AdminCommandSet.Functions.Add("OpenForm", (List<string> para, string content, Session session) =>
                {
                    if (FindPermission(session, "AdminAccess"))
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
                    if (FindPermission(session, "AdminAccess"))
                    {
                        var data = ServerConfig.Query(para[0], para[1]);
                        session.SendData("L2KDB:Basic:DatabaseQueryResult\r\n" + data);
                    }
                    return "";
                });
                AdminCommandSet.Functions.Add("Save", (List<string> para, string content, Session session) =>
                {
                    //L2KDB:Basic:Save,|[Session]|
                    if (FindPermission(session, "AdminAccess"))
                    {
                        ServerConfig.Save(para[0], para[1], content);
                    }
                    return "L2KDB:Basic:AccessForbidden";
                });
            }
        }
        static LiteDatabase.Database ServerConfig = new LiteDatabase.Database("./Server-Config/", LiteDatabase.DatabaseMode.OnDemand);
        public void StopServer()
        {
            Diagnotor.CurrentDiagnotor.Log("Force All Databases to Save Data.");
            foreach (var item in VariablesPool.Databases)
            {
                try
                {

                    item.realDB.ForceProcessPueueAndEndCycle();

                }
                catch (Exception e)
                {
                    Diagnotor.CurrentDiagnotor.LogError($"Unable to save data in datase:{item.realDB.givenHome}\r\nReason:{e.Message}");
                }
            }
            Diagnotor.CurrentDiagnotor.LogSuccess("Completed.");

        }
        public void InitializeFeatureFlags()
        {
            ServerConfig.OpenForm("Features");
            var Names=ServerConfig.GetID2("Flags");
            foreach (var item in Names)
            {
                try
                {
                    var value = ServerConfig.Query("Flags", item);
                    VariablesPool.FeatureFlags.Add(item, int.Parse(value.ToString()));
                }
                catch (Exception)
                {
                }
            }
        }
        public void InitializeCore()
        {
            InitBasicCommands();
            InitAdminCommands();
            InitializeFeatureFlags();
        }
        public ServerCore(string BasePath="./Databases/",string SeverConfigPath="./Server-Config/")
        {
            this.BasePath = BasePath;
            ServerConfig = new LiteDatabase.Database(SeverConfigPath, LiteDatabase.DatabaseMode.OnDemand);
            InitializeCore();
        }
        public static bool FindPermission(Session session, string PermissionID)
        {
            try
            {
                return bool.Parse((string)ServerConfig.Query(session.AuthID, PermissionID));
            }
            catch (Exception)
            {
                if (session.GetIPAddress() == "127.0.0.1")
                {
                    return true;
                }
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
            Diagnotor.CurrentDiagnotor.LogSuccess("Server started on: "+ipadd+":"+port);
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
                Diagnotor.CurrentDiagnotor.LogWarning("[WRONG CLIENT]Sending Refuse.");
                streamWriter.Flush();
                streamWriter.Close();
            }
        }
        private void Listen()
        {
            while (stopFlag != true)
            {
                Diagnotor.CurrentDiagnotor.Log("Waiting for shaking hand.");
                var request = Listener.AcceptTcpClient();
                var r = Task.Run(() => { ProcessHandshake(request); });

            }
        }
    }
}
