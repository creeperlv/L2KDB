using System;
using System.Collections.Generic;
using System.IO;
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
        public Session(TcpClient client, StreamReader reader, Stream originalStream)
        {
            Client = client;
            Reader = reader;
            OriginalStream = originalStream;
            Writer = new StreamWriter(OriginalStream);
            Task.Run(async ()=> { SessionWorker(); });
        }
        Guid SessionID;
        public async void SessionWorker()
        {
            SessionID = Guid.NewGuid();
            await Writer.WriteAsync("L2KDB:Basic:ConnectionAccept,"+SessionID.ToString());
            await Writer.FlushAsync();
            while (true)
            {
                try
                {
                    var Command=await Reader.ReadLineAsync();
                    var data = await Reader.ReadToEndAsync();
                    /**
                     * Data Structure:
                     * [Command]|[SessionID]|[Key(Optional)]|[IV(Optional)]
                     * [Cont...
                     * en...
                     * t]
                     **/
                    var cmd=Command.Split('|');
                    var result=CmdletProcesser(cmd, data);
                    Writer.Write($"L2KDB:Basic:CommandComplete|{SessionID}{Environment.NewLine}{result}");
                }
                catch (Exception)
                {
                }
            }
        }
        String CmdletProcesser(string[] cmdlet,string content)
        {
            var cmd = cmdlet[0].Split(':');
            switch (cmd[0])
            {
                case "L2KDB":
                    break;
                default:
                    Writer.Write("Unknown Command Set!");
                    Writer.Flush();
                    throw new Exception("Unknown Command Set!");
            }
            return null;
        }
        public void Stop(StopReason stopReason = StopReason.ShutdownServer)
        {
            Writer.Write("L2KDB:Basic:Stop," + stopReason.ToString());
            Writer.Flush();
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
        }
    }
}
