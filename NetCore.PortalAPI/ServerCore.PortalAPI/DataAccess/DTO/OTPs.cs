using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    public class OTPs
    {
        public class AccountPostData
        {
            public int productId { get; set; }
            public int platformId { get; set; }
            public string browserName { get; set; }
            public string hashKey { get; set; }
            public string userName { get; set; }
            public string serviceKey { get; set; }
        }

        public class AuthentOTPInput
        {
            public string otpToken { get; set; }
            public string otp { get; set; }
            public int otpType { get; set; }
            //public string serviceKey { get; set; }
        }

        public class AuthentOTPInputPost
        {
            public string OtpToken { get; set; }
            public string Otp { get; set; }
            public int OtpType { get; set; }
            public int ServiceId { get; set; }
            public string serviceKey { get; set; }
        }

        /// <summary>
        /// dữ liệu đầu ra sau khi login thành công
        /// </summary>
        public class AccountReturnData
        {
            public int AccountID { get; set; }
            public string UserName { get; set; }
            public long Bon { get; set; }
            public long Bac { get; set; }
            public bool IsOtp { get; set; }
            public string otpToken { get; set; }
            public string OtpCode { get; set; }
            public string Avatar { get; set; }
            public string NickName { get; set; }
            public int Level { get; set; }
            public int ResponCode { get; set; }
            public int IsConfirmMobile { get; set; }
            public string PhoneNumber { get; set; }
            public string AccountToken { get; set; }
            public string Mobile { get; set; }
        }

        /// <summary>
        /// dữ liệu để post GetAccountInfo
        /// </summary>
        public class AccountMobileReturnData
        {
            public string PassPort { get; set; }
            public string Email { get; set; }
            public string Mobile { get; set; }
            public bool IsMobileActived { get; set; }
            public bool IsEmailActived { get; set; }
            public bool IsOTP { get; set; }
            public int ResponCode { get; set; }
        }

        /// <summary>
        /// dữ liệu đầu ra setup app token
        /// </summary>
        public class SetupAppTokenOutput
        {
            public int AccountID { get; set; }
            public string Mobile { get; set; }
            public string TokenKey { get; set; }
            public int Status { get; set; }
            public string Description { get; set; }
            public int otp { get; set; }
            public string VerifyCode { get; set; }
            public string AccountToken { get; set; }    //  check account client với server
        }

        /// <summary>
        /// dữ liệu đầu vào setup app token
        /// </summary>
        public class SetupAppTokenInput
        {
            public string AccountName { get; set; }
            public string TokenKey { get; set; }
            public int Status { get; set; }
            public string Description { get; set; }
            public int otp { get; set; }
            public string VerifyCode { get; set; }
            public string uiid { get; set; }
            public string AccountToken { get; set; }    //  check account client với server
        }

        /// <summary>
        /// dữ liệu đầu vào check SMS
        /// </summary>
        public class AccountVerifySms
        {
            public int AccountID { get; set; }
            public string Username { get; set; }
            public string UserSMS { get; set; }
            public int TYPE { get; set; }
            public int CodeVerifying { get; set; }
            public string AccountToken { get; set; }    //  check account client với server
        }

        /// <summary>
        /// dữ liệu đầu vào đồng bộ client app - server api
        /// </summary>
        public class CheckActiveSyncTime
        {
            public int SystemID { get; set; }
            public long AccountID { get; set; }
            public string SystemUserName { get; set; }
            public string Mobile { get; set; }
            public string TokenKey { get; set; }
            public int ServiceID { get; set; }
            public string VerifyCode { get; set; }
            public long ClientUnixTimestamp { get; set; }

            public string AccountToken { get; set; }    //  check account client với server
        }

        public class CheckActiveSyncTimeInput
        {
            public string AccountName { get; set; }
            public string TokenKey { get; set; }
            public long ClientUnixTimestamp { get; set; }
            public string AccountToken { get; set; }
        }

        public class TokensCheckActiveInput
        {
            public string SystemUserName { get; set; }
            public string AccountName { get; set; }
            public string TokenKey { get; set; }
            public string VerifyCode { get; set; }
            public long ClientUnixTimestamp { get; set; }
            public string AccountToken { get; set; }
        }

        /// <summary>
        /// dữ liệu đầu vào cho vieecjj login
        /// </summary>
        public class AuthentInput
        {
            public string userName { get; set; }
            public string hashKey { get; set; }
        }

        public class AccountUpdateInfoInput
        {
            public string mobile { get; set; }
            public string otp { get; set; }
            public int otpType { get; set; }
            public string AccountToken { get; set; }    //  check account client với server
        }

        /// <summary>
        /// dữ liệu đầu vào của việc update thông tin tk
        /// </summary>
        public class AccountUpdateInfoSendPost
        {
            public int accountId { get; set; }
            public string mobile { get; set; }
            public string otp { get; set; }
            public int otpType { get; set; }
            public string seviceKey { get; set; }

            //public string AccountToken { get; set; }    //  check account client với server
        }

        public class AuthenSession
        {
            public int AccountID { get; set; }
            public string UserName { get; set; }
            public string VerifyCode { get; set; }
            public string Captcha { get; set; }
        }

        public class RegisterSecurityModels
        {
            public string AccountName { get; set; }
            public int AccountId { get; set; }
            public byte Type { get; set; } // 1: Kích hoạt OTP; 2: Huy OTP; 3: Xoa số điện thoại OTP
            public string SecureCode { get; set; } // mã xác thực OTP
            public string Mobile { get; set; }
            public string ServiceKey { get; set; }
        }

        //  Account_VerifyOTPInput
        public class AccountVerifyOTPInput
        {
            public int AccountID { get; set; }
            public string Username { get; set; }
            public string UserSMS { get; set; }
            public int TYPE { get; set; }
            public int CodeVerifying { get; set; }
        }

        public class ResponMes
        {
            public int ResponCode { get; set; }
            public string Mes { get; set; }
            public string OTPKey { get; set; }
        }

        public class UpdateInfo
        {
            public int AccountID { get; set; }
            public string Email { get; set; }
            public string Mobile { get; set; }
            public string CMTND { get; set; }
            public int Avatar { get; set; }
        }

        //PROCEDURE[dbo].[SP_OTPApp_ActiveTwoFactAuthenticator]
        //@_AccountID INT, --User của hệ thống cần sử dụng phương thức bảo mật
        //@_AccountName VARCHAR(50),
        //@_TokenKey VARCHAR(50) = NULL, --Serial cua dien thoai khi active
        //@_SecretKey     VARCHAR(50) = NULL, --Mã để tạo OTP
        //@_Status        BIT,	--0: Hủy; 1: Active

        public class ActiveFactor
        {
            public long AccountID { get; set; }
            public string AccountName { get; set; }
            public string TokenKey { get; set; }
            public string SecretKey { get; set; }
            public int Status { get; set; }

            public ActiveFactor(long accountID, string accountName, string tokenKey, string secretKey, int status)
            {
                this.AccountID = accountID;
                this.AccountName = accountName;
                TokenKey = tokenKey;
                SecretKey = secretKey;
                Status = status;
            }
        }

        public class ActiveFactorOutput
        {
            public string Issuer { get; set; }
            public string Message { get; set; }
            public string SecretKey { get; set; }

            public ActiveFactorOutput(string issuer, string message, string secretKey)
            {
                Issuer = issuer;
                Message = message;
                SecretKey = secretKey;
            }
        }
    }
}
