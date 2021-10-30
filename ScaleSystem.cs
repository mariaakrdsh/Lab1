using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabCalculator
{
    public struct Index
    {
        public int row;
        public int column;
    }
    static public class BasedSystem26
    {
        public static string To26System(int x)
        {
            x++;
            int mod;
            string colName = "";
            if (x == 0) return ((char)64).ToString();
            while (x > 0)
            {
                mod = (x - 1) % 26;
                colName = ((char)(65 + mod)).ToString() + colName;
                x = (x - mod) / 26;
            }
            return colName;
        }

        public static Index From26System(string x)
        {
            Index ans = new LabCalculator.Index();
            ans.column = 0;
            ans.row = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] >= 64 && x[i] < 91)
                {
                    ans.column *= 26;
                    ans.column += (x[i]) - 64;
                }
                else if (x[i] > 47 && x[i] < 58)
                {
                    ans.row *= 10;
                    ans.row += x[i] - 48;
                }
            }
            ans.column--;
            return ans;
        }
    }

}