using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace L2KDB.Server.Core
{
    public class ServerCore
    {
        public ServerCore()
        {

        }
        List<Session> SessionPool = new List<Session>();
        TcpListener client;
        public void Start()
        {
            SessionPool.Clear();
            var ip = IPAddress.Parse(("0.0.0.0"));
            int port = 9341;
            var Endpoint = new IPEndPoint(ip, port);
            client = new TcpListener(Endpoint);
            client.Start();
            Listen();
        }
        bool stopFlag = false;
        private void Listen()
        {
            while (stopFlag == true)
            {
                var request = client.AcceptTcpClient();
                var stream = request.GetStream();
                StreamReader streamReader = new StreamReader(stream);
                var str = streamReader.ReadToEnd();
                var req = str.Split('|');
                /**
                 * Data Structure:
                 * [Request]
                 * [Session ID]
                 * [Key(Optional)]
                 * [IV(Vector)(Optional)]
                 * [Content]
                 * 
                 **/
                if (req.Length > 2)
                {
                    if (req[0] == "L2KDB:Basic:OpenSession")
                    {
                        Session session = new Session(request, streamReader, stream);
                        SessionPool.Add(session);
                    }
                }
            }
        }
    }
}
