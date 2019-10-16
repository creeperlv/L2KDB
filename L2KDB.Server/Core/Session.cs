using L2KDB.Server.Core.CommandSets;
using L2KDB.Server.Utils.IO;
using LiteDatabase;
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
        Unknown, Banned, AccessForbidden, ShutdownServer
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
        public Session(TcpClient client, StreamReader reader, Stream originalStream)
        {
            Client = client;
            Reader = reader;
            OriginalStream = originalStream;
            Writer = new StreamWriter(OriginalStream);
            Task.Run(async ()=> { SessionWorker(); });
        }
        Guid SessionID;
        CustomedAES CustomedAES = new CustomedAES();
        public async Task SendDataAsync(string Data)
        {
            await Writer.WriteAsync(CustomedAES.Encrypt(Data));
            await Writer.FlushAsync();

        }
        public void SendData(String Data)
        {
            Writer.Write(CustomedAES.Encrypt(Data));
            Writer.Flush();
        }
        public async void SessionWorker()
        {

            SessionKey = CustomedAES.GenerateKey();
            SessionIV = CustomedAES.GenerateIV();
            CustomedAES.Key = SessionKey;
            CustomedAES.IV = SessionIV;
            SessionID = Guid.NewGuid();
            AdvancedStream.SendMessage(ref Writer,"L2KDB:Basic:ConnectionAccept,"+SessionID.ToString()+$",{SessionKey},{SessionIV}");
            while (true)
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
                    Console.WriteLine("Command:"+Command);
                    var cmd=Command.Split('|');
                    var result = CmdletProcesser(cmd, data);
                    Console.WriteLine(result);
                    AdvancedStream.SendMessage(ref Writer, $"L2KDB:Basic:CommandComplete|{SessionID}{Environment.NewLine}{result}", CustomedAES);
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

                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
        String CmdletProcesser(string[] cmdlet,string content)
        {
            var cmd = cmdlet[0].Split(':');
            Console.WriteLine("Command:" + cmdlet[0]+$",{cmd[0]},{cmd[1]},{cmd[2]}");
            switch (cmd[0])
            {
                case "L2KDB":
                    if (cmd[1] == "Basic")
                    {
                        var cmd3 = cmd[2].Split(',').ToList();
                        Console.WriteLine($"Command:{cmd3[0]}");
                        var cmdc = cmd3[0];
                        cmd3.RemoveAt(0);
                        Console.WriteLine($"Calling...");
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
