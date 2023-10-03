using ServerCore.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalAPI.Models
{
    public class LoginData
    {
        public int PlatformId { get; set; }
        public int ProductId { get; set; }
        public string SecretKey { get; set; }
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string AuthKey { get; set; } 
        public string UIID { get; set; } 
    }

    //string provider = data.provider;//VTT, VNP, VMS
    //int type = data.type;//type = 0: mua the,type = 1: trả trước, 2: trả sau
    /// <summary>
    /// 
    /// </summary>
    public class BuyCardData
    {
        public string UserName { get; set; }
        public string Provider { get; set; }//VTT, VNP, VMS
        public int Type { get; set; } ////type = 0: mua the,type = 1: trả trước, 2: trả sau
        public int Money { get; set; }
        public string PhoneNumber { get; set; }
        public int AccountId { get; set; } 
        public int MerchantId { get; set; } 
        public int SourceId { get; set; } 
        public int Quantity { get; set; } 
    }

    public class OTPDataReturn
    {
        public string OtpToken { get; set; }
        //public string Message { get; set; }
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

    public class TransferData
    {
        public int AccountIdTrans { get; set; }
        public string NickNameTrans { get; set; }
        public int TransferValue { get; set;}
        public int AccountIdRecv { get; set; }
        public string NickNameRecv { get; set;}
        public int TransferType { get; set;}
        public string IpAddress { get; set;}
        public int SourceId { get; set;}
        public string Reason{get;set;}
    }

    public class ChangePassWordData
    {
        public long AccountID { get; set; }
        public string AccountName { get; set; }
        public string OldPassWord { get; set; }
        public string NewPassWord {get; set;}
    }

    public class AccountDataReturn
    {
        public long AccountID { get; set; }
        public string UserName { get; set; }
        public long TotalXu { get; set; }
        public long TotalCoin { get; set; }
        public int Avatar { get; set; }
        public string NickName { get; set; }
        public int Level { get; set; }
        public bool IsOtp { get; set; }
        public string OtpToken { get; set; }
        public bool IsCaptcha { get; set; }
        //  OTP APP
        public string AccountToken { get; set; }
        public string Mobile { get; set; }
        public bool IsMobileActived { get; set; }
        public bool IsAllowChat { get; set; }
        //  END OTP APP

        public AccountDataReturn(long accountId, string userName, long bac, long bon, int avatar, string nickName, int level, int isOtp, string otpToken)
        {
            AccountID = accountId;
            UserName = "";// userName;
            TotalXu = bac;
            TotalCoin = bon;
            Avatar = avatar;
            NickName = nickName;
            Level = level;
            IsOtp = isOtp == 0 ? false : true;
            OtpToken = otpToken;
            IsCaptcha = false;
        }

        public AccountDataReturn(AccountDb accountDb)
        {
            AccountID = accountDb.AccountID;
            UserName = accountDb.UserName;
            TotalXu = accountDb.TotalXu;
            TotalCoin = accountDb.TotalCoin;
            Avatar = accountDb.Avatar;
            NickName = accountDb.UserFullname;
            Level = accountDb.Level;
            IsOtp = accountDb.IsOtp == 0 ? false : true;
            OtpToken = null;//accountDb.OtpToken;
            IsCaptcha = false;

            AccountToken = accountDb.AccountToken;
            Mobile = accountDb.Mobile;
            IsMobileActived = accountDb.IsMobileActived;
        }

        public AccountDataReturn()
        {

        }
    }

    public class AccountDataReturnReport : AccountDataReturn
    {
        public string Mobile { get; set; }

        public AccountDataReturnReport(AccountDb accountDb, string mobile)
        {
            AccountID = accountDb.AccountID;
            UserName = accountDb.UserName;
            TotalXu = accountDb.TotalXu;
            TotalCoin = accountDb.TotalCoin;
            Avatar = accountDb.Avatar;
            NickName = accountDb.UserFullname;
            Level = accountDb.Level;
            IsOtp = accountDb.IsOtp == 0 ? false : true;
            OtpToken = accountDb.OtpToken;
            Mobile = mobile;
            IsCaptcha = false;
        }

        public AccountDataReturnReport()
        {

        }

        public AccountDataReturnReport(AccountInfo accountInfo)
        {
            AccountID = accountInfo.AccountID;
            UserName = accountInfo.UserName;
            TotalXu = accountInfo.TotalXu;
            TotalCoin = accountInfo.TotalCoin;
            Avatar = accountInfo.Avatar;
            NickName = accountInfo.NickName;
            Level = accountInfo.Level;
            IsOtp = accountInfo.IsOtp;
            OtpToken = accountInfo.OtpToken;
            Mobile = accountInfo.Mobile;
            IsCaptcha = false;
        }
    }

    public class FrozenData
    {
        public long Balance { get; set; }
        public long FrozenValue { get; set; }
    }

    public class VipPointTradeData
    {
        public long Balance { get; set; }
        public int VipPointCurrent { get; set; }
    }
}
