using L2KDB.Server.Core.CommandSets;
using L2KDB.Server.Utils.IO;
using L2KDB.Core;
using LiteDatabase.CustomedCryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace L2KDB.Server.Core
{
    public enum StopReason
    {
        Unknown, Banned, AccessForbidden, ShutdownServer,SessionIDChanged
    }
    public class Session
    {
        TcpClient Client;
        StreamReader Reader;
        StreamWriter Writer;
        Stream OriginalStream;
        String SessionKey = "";
        String SessionIV = "";
        public String AuthID="";
        public Database operatingBD;
        public Session(TcpClient client, StreamReader reader, Stream originalStream,string AuthID)
        {
            this.AuthID = AuthID;
            Client = client;
            Reader = reader;
            OriginalStream = originalStream;
            Writer = new StreamWriter(OriginalStream);
            Task.Run(async ()=> { SessionWorker(); });
        }
        Guid SessionID;
        CustomedAES CustomedAES = new CustomedAES();
        public CustomedAES DatabaseAES = new CustomedAES();
        public async Task SendDataAsync(string Data)
        {
            AdvancedStream.SendMessage(ref Writer, Data, CustomedAES);
        }
        public void SendData(String Data)
        {
            AdvancedStream.SendMessage(ref Writer, Data, CustomedAES);
        }
        public void SendData(string title,String Data)
        {
            AdvancedStream.SendMessage(ref Writer,title, Data, CustomedAES);
        }
        bool willStop = false;
        public async void SessionWorker()
        {
            SessionID = Guid.NewGuid();
            Console.WriteLine("Session Opened:"+SessionID+", AuthID="+AuthID);
            SessionKey = CustomedAES.GenerateKey();
            SessionIV = CustomedAES.GenerateIV();
            CustomedAES.Key = SessionKey;
            CustomedAES.IV = SessionIV;
            int ExceptionOccurred = 0;
            AdvancedStream.SendMessage(ref Writer,"L2KDB:Basic:ConnectionAccept,"+SessionID.ToString()+$",{SessionKey},{SessionIV}");
            while (willStop==false)
            {
                try
                {
                    var Command= CustomedAES.Decrypt(await Reader.ReadLineAsync());
                    var data = AdvancedStream.ReadToCurrentEnd(ref Reader, CustomedAES);
                    /**
                     * Data Structure:
                     * [Command]|[SessionID]|[Key(Optional)]|[IV(Optional)]
                     * [Cont...
                     * en...
                     * t]
                     **/
                    Console.WriteLine($"Command from {SessionID}:"+Command);
                    var cmd=Command.Split('|');
                    if (cmd[1] != SessionID.ToString())
                    {
                        Stop(StopReason.Unknown);
                    }
                    
                    var result = CmdletProcesser(cmd, data);
                    if (result != "-1")
                    {
                        AdvancedStream.SendMessage(ref Writer, $"{result}", CustomedAES);
                    }
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(System.Net.Sockets.SocketException))
                    {
                        Console.WriteLine($"Shutting {SessionID} Down.");
                        Stop();
                        break;
                    }
                    else if (e.GetType() == typeof(System.IO.IOException))
                    {
                        Console.WriteLine($"Shutting {SessionID} Down.");
                        Stop();
                        break;
                    }
                    else
                    {
                        ExceptionOccurred++;
                        Console.WriteLine("Captured:"+e.Message);
                        if (ExceptionOccurred >= 100)
                        {
                            Console.WriteLine($"Session:{SessionID} occurred too many errors, force to shutdown.");
                            Stop();
                        }
                    }
                }
            }
        }
        String CmdletProcesser(string[] cmdlet,string content)
        {
            var cmd = cmdlet[0].Split(':');
            switch (cmd[0])
            {
                case "L2KDB":
                    if (cmd[1] == "Basic")
                    {
                        var cmd3 = cmd[2].Split(',').ToList();
                        var cmdc = cmd3[0];
                        cmd3.RemoveAt(0);
                        return BasicCommandSet.Functions[cmdc](cmd3,content,this);
                    }
                    break;
                default:
                    SendData("Unknown Command Set!");
                    throw new Exception("Unknown Command Set!");
            }
            return null;
        }
        public void Stop(StopReason stopReason = StopReason.ShutdownServer)
        {
            willStop = true;
            try
            {

                AdvancedStream.SendMessage(ref Writer, "L2KDB:Basic:Stop," + stopReason.ToString(), CustomedAES);
            }
            catch (Exception)
            {

            }
            try
            {
                Reader.Dispose();
            }
            catch (Exception)
            {
            }
            try
            {
                Writer.Dispose();
            }
            catch (Exception)
            {
            }
            try
            {
                OriginalStream.Dispose();
            }
            catch (Exception)
            {
            }
            try
            {
                Client.Dispose();
            }
            catch (Exception)
            {
            }
            try
            {
                ServerCore.SessionPool.Remove(this);
            }
            catch (Exception)
            {
            }
            System.GC.Collect();
        }
    }
}
