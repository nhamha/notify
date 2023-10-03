using System;

namespace ServerCore.DataAccess.DTO
{
    /// <summary>
    /// Lớp mô tả thông tin tài khoản Ebank
    /// </summary>
    [Serializable]
    public class AccountEbank
    {
        public long AccountID { get; set; }
        public int PaygateID { get; set; }
        public int UserSN { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public DateTime JoinedTime { get; set; }
        public string Email { get; set; }
        public DateTime Birthday { get; set; }
        public string Passport { get; set; }
        public int Gender { get; set; }
        public int LocationID { get; set; }
        public int Status { get; set; }
        public int ConfirmCode { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public int VcoinPayment { get; set; }
        public int Vcoin { get; set; }
        public int NationalID { get; set; }
        public int QuestionID { get; set; }
        public string Answer { get; set; }
        public DateTime ChangedTime { get; set; }
        public DateTime LastLoginedTime { get; set; }
        public int Error { get; set; }
        public string Captcha { get; set; }
        public bool ReceiveEmail { get; set; }

        //NhatND add 1/11/2012 - add for update ebank 2.1
        public byte AccountTypeID { get; set; }    // Loại tài khoản - đồng bộ với DB khi có mô tả
        public int JoinFrom { get; set; }          // Đăng ký từ đâu, ebank, wap mặc định = 1
        public string GameService { get; set; }//danh sach game yeu thich

        // tuanhd add 07/08/2013 
        public string SmsPlusMobile { get; set; } // số điện thoại sử dụng nhận SMS Plus
        public long PartnerAccountID { get; set; }

        public int TotalVcoin { get; set; }
        public int TotalVcoinPayment { get; set; }
        public int FreezeVcoin { get; set; }

        public bool IsUpdateSecurityInfo { get; set; }
    }

    /// <summary>
    /// Tiện ích SMS Banking 
    /// </summary>
    [Serializable]
    public class SMSBanking
    {
        public long AccountID { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public int AccountStatus { get; set; }
        public string SecurityCode { get; set; }
        public int SMSServiceID { get; set; }
        public string ServiceName { get; set; }
        public int ServiceStatus { get; set; }
        public DateTime CreatedTime { get; set; }
        public string OTP { get; set; }
        public int Error { get; set; }
        public int MinAmount { get; set; }
        public bool BalanceInc { get; set; }
        public bool BalanceDesc { get; set; }
    }
}