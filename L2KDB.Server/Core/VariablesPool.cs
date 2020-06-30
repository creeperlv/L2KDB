using L2KDB.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace L2KDB.Server.Core
{
    public class VariablesPool
    {
        public static List<Database> Databases = new List<Database>();
        public static Dictionary<string, int> FeatureFlags = new Dictionary<string, int>();
    }
}
