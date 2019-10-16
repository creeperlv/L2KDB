using System;
using System.Collections.Generic;
using System.Text;

namespace L2KDB.Server.Utils.LinearAlgebra
{
    public class Matrix
    {
        public int this[int x, int y]
        {
            get => data[$"{x},{y}"]; set { if (data.ContainsKey($"{x},{y}"))
                {
                    data[$"{x},{y}"] = value;
                }
                else
                {
                    if (x < Columns & y < Rows & x >= 0 & y >= 0)
                    {
                        data.Add($"{x},{y}", value);
                    }
                    else
                    {
                        throw new Exception("Accessing Illegal Position of the Matrix!");
                    }
                }
            }
        }
        public int Rows;
        public int Columns;
        public Dictionary<string, int> data = new Dictionary<string, int>();
        public Matrix(int m, int n, string data)
        {
            Rows = m;
            Columns = n;
            int index = 0;
            if (data != null & data != "")
            {

                for (int x = 0; x < m; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        this.data[$"{x},{y}"] = data[index % data.Length];
                        index++;
                    }
                }
            }
            else
            {

                for (int x = 0; x < m; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        this.data[$"{x},{y}"] = 0;
                        index++;
                    }
                }
            }
        }
        public static Matrix operator *(Matrix m1,int Multiplier)
        {
            Matrix matrix = new Matrix(m1.Rows, m1.Columns, "");
            for (int x = 0; x < m1.Rows; x++)
            {
                for (int y = 0; y < m1.Columns; y++)
                {
                    matrix[x, y] = m1[x, y] * Multiplier;
                }
            }
            return matrix;
        }
        public static List<int> GenerateColumnSum(Matrix matrix)
        {
            List<int> vs = new List<int>();
            for (int x = 0; x < matrix.Rows; x++)
            {
                int tmp = 0;
                for (int y = 0; y < matrix.Columns; y++)
                {
                    tmp += matrix[x, y];
                }
                vs.Add(tmp);
            }
            return vs;
        }public static List<int> GenerateRowSum(Matrix matrix)
        {
            List<int> vs = new List<int>();
            for (int x = 0; x < matrix.Columns; x++)
            {
                int tmp = 0;
                for (int y = 0; y < matrix.Rows; y++)
                {
                    tmp += matrix[x, y];
                }
                vs.Add(tmp);
            }
            return vs;
        }
        public static Matrix operator /(Matrix m1,int Multiplier)
        {
            Matrix matrix = new Matrix(m1.Rows, m1.Columns, "");
            for (int x = 0; x < m1.Rows; x++)
            {
                for (int y = 0; y < m1.Columns; y++)
                {
                    matrix[x, y] = m1[x, y] / Multiplier;
                }
            }
            return matrix;
        }
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            Matrix result = new Matrix(m1.Rows, m2.Columns,"");
            for (int x = 0; x < m1.Rows; x++)
            {
                for (int y = 0; y < m2.Columns; y++)
                {
                    int tmp = 0;
                    try
                    {
                        tmp = m1[x, y];
                        try
                        {
                            tmp += m2[x, y];
                        }
                        catch (Exception)
                        {
                        }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            tmp += m2[x, y];
                        }
                        catch (Exception)
                        {
                        }
                    }
                    result[x, y] = tmp;
                }
            }
            return result;
        }
    }
}
