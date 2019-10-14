using System;
using System.IO;

namespace LiteDatabase.Test.NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine((new DirectoryInfo("./").FullName));
            Database liteDatabase = new Database(loadMode: DatabaseMode.OnDemand);
            liteDatabase.OpenForm();
            //Console.WriteLine("Test LiteDatabase");
            //Console.WriteLine("Pressure Test");
            //Console.WriteLine("Please enter id1 length:");
            //int a = int.Parse(Console.ReadLine());
            //Console.WriteLine("Please enter id2 length:");
            //int b = int.Parse(Console.ReadLine());
            //Console.WriteLine("Please enter content length:");
            //int c = int.Parse(Console.ReadLine());
            //string cont = GenerateRandomString(c);
            //for (int i = 0; i < a; i++)
            //{
            //    for (int ii = 0; ii < b; ii++)
            //    {
            //        liteDatabase.Save($"id1.{i}", $"id2.{ii}", cont);
            //    }
            //}
            //foreach (var id1 in liteDatabase.GetID1())
            //{
            //    Console.WriteLine($"Fetch:{id1}");
            //    foreach (var id2 in liteDatabase.GetID2(id1))
            //    {
            //        Console.WriteLine($"Fetch:{id1}.{id2}:{liteDatabase.Query(id1, id2).GetHashCode()}");
            //    }
            //}
        }
        static string GenerateRandomString(int length)
        {
            Random random = new Random();
            string a = "";
            for (int i = 0; i < length; i++)
            {
                a += (char)random.Next(0, 60);
            }
            return a;
        }
    }
}
