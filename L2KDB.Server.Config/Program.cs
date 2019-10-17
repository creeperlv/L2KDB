using L2KDB.Server.Core.CommandSets;
using LiteDatabase;
using System;

namespace L2KDB.Server.Config
{
    class Program
    {
        static void SetAdmin()
        {
            Console.Clear();
            Console.WriteLine("L2KDB - Local 2-Key Database");
            Console.WriteLine("Set An Administrator");
            Console.WriteLine("Enter user name:");
            var usr = Console.ReadLine();
            Console.WriteLine("Enter password:");
            var pwd = Console.ReadLine();
            database.OpenForm("Permissions");
            database.Save(Authentication.ObtainID(usr, pwd), "AdminAccess", "" + true);
            scene = 0;
        }
        static void SetPermission()
        {
            Console.Clear();
            Console.WriteLine("L2KDB - Local 2-Key Database");
            Console.WriteLine("Set An Administrator");
            Console.WriteLine("Enter user name:");
            var usr = Console.ReadLine();
            Console.WriteLine("Enter password:");
            var pwd = Console.ReadLine();
            Console.WriteLine("Enter Permission Name:");
            var p = Console.ReadLine();
            database.OpenForm("Permissions");
            database.Save(Authentication.ObtainID(usr, pwd), p, "" + true); 
            scene = 0;
        }
        static Database database = new Database("./Server-Config");
        static void MainScene()
        {
            Console.Clear();
            Console.WriteLine("L2KDB - Local 2-Key Database");
            Console.WriteLine("Server Configure");
            Console.WriteLine("1 - Add An Administrator");
            Console.WriteLine("2 - Add An Permission to An AuthID");
            Console.WriteLine("3 - Add Permission 'FullDBAccess' to An AuthID");
            Console.WriteLine("4 - Set IP address will be used in next launch");
            var option = Console.ReadLine();
            try
            {
                var targetScene = int.Parse(option);
                if (targetScene < 4 & targetScene > 0)
                {
                    scene = targetScene;
                }
            }
            catch (Exception)
            {

            }
        }
        static int scene = 0;
        static void Main(string[] args)
        {
            while (true)
            {
                switch (scene)
                {
                    case 0:
                        {
                            MainScene();
                        }
                        break;
                    case 1:
                        {
                            SetAdmin();
                        }
                        break;
                    case 2:
                        {
                            SetPermission();
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Undefined Scene!");
                        Console.ForegroundColor = ConsoleColor.White;
                        scene = 0;
                        break;
                }

            }
        }
    }
}
