using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore.Utilities.Utils
{
    public class Utils
    {
        public static string GetLanguage(HttpContext httpContext)
        {
            if (httpContext != null && httpContext.Request.Headers.ContainsKey("Accept-Language"))
            {
                string lang = httpContext.Request.Headers["Accept-Language"];
                return lang.ToLower();
            }

            return "vi";
        }

        public static string FormatMobile(string mobile)
        {
            if (mobile == null || mobile.Length < 10)
                return "";

            //mobile = mobile.Replace("+", "");
            //if (mobile.StartsWith("84"))
            //    mobile = "0" + mobile.Substring(2, mobile.Length - 2);

            if (mobile.StartsWith("0"))
                mobile = mobile.Substring(1, mobile.Length - 1);
            return mobile;
        }

        public static bool IsNumber(string pValue)
        {
            foreach (Char c in pValue)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return true;
        }
    }
}
