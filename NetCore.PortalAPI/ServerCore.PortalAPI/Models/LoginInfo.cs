using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalAPI.Models
{
    public class LoginInfo
    {
        public int PlatformId { get; set; }
        public int MerchantId { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; } 
        public string UIID { get; set; } 
        public string CaptchaText { get; set; } 
        public string CaptchaToken { get; set; } 
    }
}