using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
// ===============================
// AUTHOR     : QuangPm
// SPECIAL NOTES: get IP client
// ===============================
namespace ServerCore.Utilities.Utils
{
    public class IPAddressHelper
    {
        public static string GetRemoteIPAddress(HttpContext context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                string header = (context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault());
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                    return ip.ToString();
                }
            }
            return context.Connection.RemoteIpAddress.ToString();
        }


        public static string GetClientIP(HttpContext context)
        {
            string IP = context.Connection.RemoteIpAddress.ToString();

            try
            {
                //if (HttpContext.Current.Request.ServerVariables["HTTP_CITRIX"] != null)
                //{ IP = HttpContext.Current.Request.ServerVariables["HTTP_CITRIX"]; }

                //if (string.IsNullOrEmpty(IP) && HttpContext.Current.Request.Headers["CITRIX_CLIENT_HEADER"] != null)
                //{
                //    IP = HttpContext.Current.Request.Headers["CITRIX_CLIENT_HEADER"];
                //}

                //if (string.IsNullOrEmpty(IP))
                //{
                //    if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                //    {
                //        IP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                //    }
                //    if (IP == "")
                //    {
                //        IP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                //    }

                //}
            }
            catch
            {
            }

            var ips = IP.Split(',', ':', ';');
            if (ips.Length > 1)
                IP = ips[0];

            return IP.Replace('|', '#').Trim();
        }

        private static string[] range = new string[] { "125.234", "125.235", "203.113", "220.231", "203.41", "203.162", "203.210", "222.255", "210.245" };
        public static string GetClientIp(string username)
        {
            try
            {
                string md5Name = Security.Security.MD5Encrypt(username);
                byte[] array = Encoding.ASCII.GetBytes(md5Name);

                if (BitConverter.IsLittleEndian) Array.Reverse(array);
                int length = array.Length;

                int val1 = BitConverter.ToInt32(array, 0);
                int val2 = BitConverter.ToInt32(array, 1);
                int val3 = BitConverter.ToInt32(array, 2);

                val1 = val1 % range.Length;
                val2 = val2 % 254 + 1;
                val3 = val3 % 254 + 1;

                return string.Format("{0}.{1}.{2}", range[val1], val2, val3);
            }
            catch (Exception ex)
            {

            }
            return "";//GetClientIP();
        }
    }
}