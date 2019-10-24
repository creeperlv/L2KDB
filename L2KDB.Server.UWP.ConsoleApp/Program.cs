using L2KDB.Server.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace L2KDB.Server.UWP.ConsoleApp
{
    class Program
    {
        static List<string> GetCommands(string originalCMD)
        {
            List<string> vs = new List<string>();
            var tmp = "";
            bool isT = false;
            bool isStarted= true;
            foreach (var item in originalCMD)
            {
                if (isT == false)
                {
                    if (item != ' ' || item != '\"')
                    {
                        tmp += item;
                    }
                    else if (item == '\\')
                    {
                        isT = true;
                    }else
                    {
                        vs.Add(tmp);
                        tmp = "";
                    }
                }
                else
                {
                    if (item == 'n')
                    {
                        tmp += "\n";
                    }
                    else
                    {
                        tmp += item;
                    }
                    isT = false;
                }
            }
            return vs;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("AppData:" +
            Windows.Storage.ApplicationData.Current.LocalFolder.Path);
            ServerCore core = new ServerCore(
            Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Databases\\",
            Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Server-Config\\");
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
                else if (cmd.ToUpper().StartsWith("REMOVE-ADMIN"))
                {
                    try
                    {

                        var combine = cmd.Substring("Remove-Admin".Length);
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
