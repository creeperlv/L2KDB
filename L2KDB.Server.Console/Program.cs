using L2KDB.Server.Core;
using System;
using System.IO;

namespace L2KDB.Server.SConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Local 2-Key Database Server");
            ServerCore core = new ServerCore();
            core.Start();

            Console.ReadLine();

        }
    }
}
