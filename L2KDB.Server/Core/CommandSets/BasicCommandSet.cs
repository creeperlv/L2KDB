using System;
using System.Collections.Generic;
using System.Text;

namespace L2KDB.Server.Core.CommandSets
{
    public class BasicCommandSet
    {
        public static Dictionary<string, Func<List<string>,string, Session, string>> Functions = new Dictionary<string, Func<List<string>, string,Session, string>>();
    }
    public class AdminCommandSet
    {
        public static Dictionary<string, Func<List<string>,string, Session, string>> Functions = new Dictionary<string, Func<List<string>, string,Session, string>>();
    }
}
