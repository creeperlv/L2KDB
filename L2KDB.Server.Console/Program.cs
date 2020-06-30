using L2KDB.Server.Core;
using LiteDatabase;
using System;
using System.IO;
using System.Threading.Tasks;

namespace L2KDB.Server.SConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string workingDirectory = "./";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--W"|args[i]=="-WorkingDirectory")
                {
                    workingDirectory = args[i + 1];
                    i++;
                }
            }
            Console.WriteLine("Local 2-Key Database Server");
            Console.WriteLine("Storage Files and Configs to: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(new DirectoryInfo(".").FullName);
            Console.ForegroundColor = ConsoleColor.White;
            ServerCore core = new ServerCore(Path.Combine(workingDirectory,"Databases"),Path.Combine(workingDirectory,"Server-Config"));
            Task.Run(() => core.Start());
            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd.ToUpper() == "STOP")
                {
                    core.StopServer();
                    Environment.Exit(0);
                }
                else if (cmd.ToUpper().StartsWith("SET-ADMIN"))
                {
                    try
                    {

                        var combine = cmd.Substring("Set-Admin".Length).Trim();
                        var auth = combine.Split(' ');
                        core.SetAdmin(auth[0], auth[1]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Set {auth[0]} to administrator.");
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Unable to set admin.\r\bException:{e.Message}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else if (cmd.ToUpper().StartsWith("SET-PORT"))
                {
                    try
                    {

                        var combine = cmd.Substring("Set-Port".Length).Trim();
                        core.SetPort(int.Parse(combine));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Set port to: {combine}.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"It requires a restart to take effect.");
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Unable to set port.\r\bException:{e.Message}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else if (cmd.ToUpper().StartsWith("SET-IP"))
                {
                    try
                    {

                        var combine = cmd.Substring("Set-IP".Length).Trim();
                        core.SetIP((combine));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Set IP to: {combine}.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"It requires a restart to take effect.");
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Unable to set port.\r\bException:{e.Message}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else if (cmd.ToUpper().StartsWith("REMOVE-ADMIN"))
                {
                    try
                    {

                        var combine = cmd.Substring("Remove-Admin".Length);
                        var auth = combine.Split(' ');
                        core.RemoveAdmin(auth[0], auth[1]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Removed administrator permission of {auth[0]}.");
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Unable to set admin.\r\bException:{e.Message}");
                    }
                }
                else if (cmd.ToUpper() == "VERSION")
                {
                    Console.Write("Server:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(core.CoreVersion + "\r\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("L2KDB:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(Database.DatabaseVersion + "\r\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Shell:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("0.1.0.0\r\n");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (cmd.ToUpper().StartsWith("SET-FLAG"))
                {

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
