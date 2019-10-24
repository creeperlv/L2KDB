using System;
using System.Collections.Generic;
using System.Text;

namespace L2KDB.Server.Diagnostic
{
    public class Diagnotor
    {
        public static IDiagnotor CurrentDiagnotor=new DefaultDiagnotor();
    }
    public class DefaultDiagnotor : IDiagnotor
    {
        public void Log(string str)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogError(string str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogSuccess(string str)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogWarning(string str)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    public interface IDiagnotor
    {
        void Log(string str);
        void LogError(string str);
        void LogWarning(string str);
        void LogSuccess(string str);
    }
}
