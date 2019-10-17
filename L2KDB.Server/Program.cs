using L2KDB.Server.Core;
using L2KDB.Server.Utils.LinearAlgebra;
using System;

namespace L2KDB.Server
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
