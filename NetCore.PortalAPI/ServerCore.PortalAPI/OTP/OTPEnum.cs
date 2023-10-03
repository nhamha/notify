using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.OTP
{
    public enum OTPCode
    {
        OTP_NEED_INPUT = -100,
        UNKNOW = -1,
        NOT_OTP = 0,
        OTP_NOT_VERIFY = 1,
        OTP_VERIFY_ERROR = 2,
        OTP_VERIFY_SUCCESS = 3
    }

    public enum OtpServiceCode
    {
        // Thay doi thong tin tai khoan
        OTP_SERVICE_LOGIN = 1000,
        OTP_SERVICE_RESET_PASSWORD = 1001,
        OTP_SERVICE_CHANGE_PASSWORD = 1002,
        OTP_SERVICE_CHANGE_MOBILE = 1003,

        // Thay doi so du tai khoan
        OTP_SERVICE_BUY_CARD = 2001,
        OTP_SERVICE_TRANSFER = 2002,
        OTP_SERVICE_FREEZE = 2003, // Đóng băng tài khoản
        OTP_SERVICE_UNFREEZE = 2004, // Mở băng tài khoản

        // 
        OTP_SERVICE_REGISTER_OTP = 3001,
        OTP_SERVICE_UNREGISTER_OTP = 3002,
        OTP_SERVICE_REGISTER_LOGIN_OTP = 3003,
        OTP_SERVICE_UNREGISTER_LOGIN_OTP = 3004,
        OTP_SERVICE_LOCK_GAME = 3005,
        OTP_SERVICE_UNLOCK_GAME = 3006,
    }

    public enum OtpType
    {
        OTP_REGISTER = 1,
        OTP_UNREGISTER = 9,
        OTP_NORMAL = 0,
        OTP_TRANSFER = 3
    }

    public enum RegisterOTPType
    {
        REGISTER_OTP = 1,
        UNREGISTER_OTP = 0
    }
}
