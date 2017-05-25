using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace 约车抢购
{
    public class Static
    {
        public static HttpClient client;
        public static HttpClientHandler handler;
        public static CookieContainer cookie;

        public static string UserName = "";
        public static string TeachName = "";
        internal static string UseTime;
        internal static string LeftTime;
    }
}
