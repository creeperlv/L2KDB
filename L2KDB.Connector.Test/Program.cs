using System;

namespace L2KDB.Connector.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            DBConnector connector = new DBConnector();
            Console.WriteLine(connector.Connect("127.0.0.1", 9341, "", ""));
        }
    }
}
