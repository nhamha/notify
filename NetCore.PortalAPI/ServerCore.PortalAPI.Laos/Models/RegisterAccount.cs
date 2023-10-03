using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalAPI.Models
{
    public class RegisterAccount
    {
        public string UserName{ get;set;}
        public string PassWord { get;set;}
        public string NickName { get;set;}
        public string CaptchaVerify{ get;set;}
        public string CaptchaText{ get;set;}
        public int PlatformId { get;set;}
        public int MerchantId{ get;set;}
        public string Email{ get;set;}
        public int ServiceID{ get;set;}
    }
}