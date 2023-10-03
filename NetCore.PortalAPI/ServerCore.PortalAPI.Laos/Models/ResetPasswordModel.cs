using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.Models
{
    public class ResetPasswordModel
    {

        public int ServiceID { get; set; }

        public string AccountName { get; set; }

        public byte Type { get; set; } // 1: Lấy lại qua SMS; 2: Lấy lại qua email

        public string Email { get; set; }

        public string Otp { get; set; }

        public int OtpType { get; set; }

        public string OtpToken { get; set; }

        public string NewPassword { get; set; }

        public string Captcha { get; set; }

        public string Token { get; set; }
    }
}
