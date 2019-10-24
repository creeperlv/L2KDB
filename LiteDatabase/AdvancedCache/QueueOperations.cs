using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LiteDatabase.AdvancedCache
{
    public class DatabaseOperation
    {
        //Once it is added to queue, target id1 will be marked to modify.
        public string id1;
        public string id2;
    }

}
