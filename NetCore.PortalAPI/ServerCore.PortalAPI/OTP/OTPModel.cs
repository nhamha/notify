using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.OTP
{
    public class OtpModel
    {
        public string Otp
        {
            get; set;
        }

        public string OtpToken
        {
            get; set;
        }
        /// <summary>
        /// OtpType: 1: SMS, 2: APP (telegram)
        /// </summary>
        public int OtpType
        {
            get; set;
        }

        public string Captcha
        {
            get; set;
        }

        public string CaptchaToken
        {
            get; set;
        }

        public int ServiceId
        {
            get; set;
        }

        public string ServiceKey
        {
            get; set;
        }
    }
}
