using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.Models
{
    public class OTPDataReturn
    {
        public string OtpToken { get; set; }
        public bool IsOtp { get; set; }
        public string Mobile { get; set; }

        public OTPDataReturn(string otpToken, string mobile = "")
        {
            OtpToken = otpToken;
            Mobile = mobile;
            IsOtp = true;
        }

        public OTPDataReturn()
        {
            IsOtp = false;
        }
    }
}
