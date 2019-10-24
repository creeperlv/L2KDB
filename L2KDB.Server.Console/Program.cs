using L2KDB.Server.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace L2KDB.Server.SConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Local 2-Key Database Server");
            ServerCore core = new ServerCore();
            Task.Run(() => core.Start());
            while (true)
            {
                var cmd= Console.ReadLine();
                if (cmd.ToUpper() == "STOP")
                {
                    core.StopServer();
                    Environment.Exit(0);
                }else if (cmd.ToUpper().StartsWith("Set-Admin"))
                {
                    try
                    {

                        var combine = cmd.Substring("Set-Admin".Length);
                        var auth = combine.Split(' ');
                        core.SetAdmin(auth[0], auth[1]);

                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Unable to set admin.\r\bException:{e.Message}");
                    }
                }
                else
                {
                    if (cmd.Trim() == "")
                    {

                    }
                    else
                    {
                        Console.WriteLine($"Command: {cmd} not found!");
                    }
                }
            }
        }
    }
}
