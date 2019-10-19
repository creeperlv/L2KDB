using L2KDB.Server.Core;
using System;

namespace L2KDB.Server.UWP.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AppData:" +
            Windows.Storage.ApplicationData.Current.LocalFolder.Path);
            ServerCore core = new ServerCore(
            Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Databases\\",
            Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Server-Config\\");
            core.Start();

        }
    }
}
