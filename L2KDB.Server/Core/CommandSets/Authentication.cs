using L2KDB.Server.Utils.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Text;

namespace L2KDB.Server.Core.CommandSets
{
    public class Authentication
    {
        public static string ObtainID(string usr,string key)
        {
            string generatedID = "";
            Matrix matrix1 = new Matrix(16, 16, usr);
            Matrix matrix2 = new Matrix(16, 16, usr);
            var m3 = matrix1 + matrix2;
            var IntID = Matrix.GenerateColumnSum(m3);

            byte[] vs = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                vs[i] = (byte)IntID[i];
            }
            generatedID = Convert.ToBase64String(vs).Replace('/','_');
            return generatedID;
        }
    }
}
