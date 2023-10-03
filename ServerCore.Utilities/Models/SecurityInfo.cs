using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.Utilities.Models
{
    [Serializable]
    public class SecurityInfo
    {

        public string PassPort
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public string Mobile
        {
            get;
            set;
        }

        public bool IsMobileActived
        {
            get;
            set;
        }

        public bool IsEmailActived
        {
            get;
            set;
        }

        public bool IsLoginOTP
        {
            get;
            set;
        }

        public SecurityInfo(string email, string mobile, string passport, bool isEmailActived, bool isMobileActived, bool isLoginOtp)
        {
            Email = email;
            Mobile = mobile;
            PassPort = passport;
            IsEmailActived = isEmailActived;
            IsMobileActived = isMobileActived;
            IsLoginOTP = isLoginOtp;
        }
    }

    public class OTPInfor
    {
        public string Mobile { get;set;}
        public long AccountID { get; set; }
        public int UserConfirmMobile { get; set; }
        public int IsOtp { get; set; }
        public string TeleChatID { get; set; }
        public bool IsMonitor { get; set; }
        public int Status { get; set; }
    }


}
