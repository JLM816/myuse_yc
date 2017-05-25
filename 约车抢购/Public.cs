using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace 约车抢购
{
    public class Public
    { 
        /// <summary>
        /// 去掉换行,去掉html中标签的空白
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string myTrim(string str)
        {
            string s = str.Replace("\r", "").Replace("\n", "");
            return Regex.Replace(s, ">[\\s]{1,}<", "><");
        }
    }
}
