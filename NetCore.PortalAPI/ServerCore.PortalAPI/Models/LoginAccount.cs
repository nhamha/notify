using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.Models
{
    public class LoginAccount
    {
        public int PlatformId { get; set; }
        public int MerchantId { get; set; }
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Uiid { get; set; }
        public string CaptchaText { get; set; }
        public string CaptchaToken { get; set; }
        public string NickName { get; set; }
        public string FacebookToken { get; set; }
        public string FacebookId { get; set; }
        public string VipCode { get; set; }
        public string CampaignSource { get; set; }
        public string AppleToken { get; set; }
        public string AppleId { get; set; }
        public string AppleEmail { get; set; }
        public string DeviceName { get; set; }
        public int IsLoginUID { get; set; }
        public int LocationID { get; set; }
        public string PreFix { get; set; }

       
    }
}
