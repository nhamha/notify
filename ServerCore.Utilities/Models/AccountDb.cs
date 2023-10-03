using Newtonsoft.Json;
using ServerCore.Utilities.Utils;
using System;

namespace ServerCore.Utilities.Models
{
    [Serializable]
    public class AccountDb
    {
        //public string Nickname { get; set; } //nickname
        //public int Level { get; set; } //level
        //public long Experiences { get; set; } //diem kinh nghiem
        //public long NextExperiences { get; set; } //diem kinh nghiem de len level tiep theo
        //public string Avatar { get; set; }// avatar url
        public AccountDb()
        {
        }

        public AccountDb(AccountDb account)
        {
            this.AccountID = account.AccountID;
            this.UserName = account.UserName;
            this.TotalXu = account.TotalXu;
            this.TotalCoin = account.TotalCoin;
            this.IsOtp = account.IsOtp;
            this.ClientIP = account.ClientIP;
            this.MerchantID = account.MerchantID;
            this.PlatformID = account.PlatformID;
            //this.IsRedirect = account.IsRedirect;
            this.Token = account.Token;
            //this.Experiences = account.Experiences;
            this.Avatar = account.Avatar;
            this.UserFullname = account.UserFullname;
            this.EmailFull = account.Email;
            this.PreFix = account.PreFix;
            this.LocationID = account.LocationID;
            //if (account.Email.Length > 0 && account.Email.Contains("@"))
            //    this.Email = "@";
            //else
            //    this.Email = account.Email;

            this.Level = account.Level;
        }

        public AccountDb(long accountId, string username, long totalCoin, long totalXu, long star, int vcoin, int eventCoin, int isOTP, int merchantId, string email = "", string userFullName = "", int level = 1, int avatar = 0, String preFix = "", string refCod = "", int locationID=0 )
        {
            this.AccountID = accountId;
            this.UserName = username;
            this.TotalXu = totalXu;
            this.TotalCoin = totalCoin;
            this.IsOtp = isOTP;
            this.MerchantID = merchantId;
            this.PlatformID = 0;
            this.EmailFull = email;
            //if (email.Length > 0 && email.Contains("@"))
            //    this.Email = "@";
            //else
            //    this.Email = email;
            //this.Experiences = experiences;
            this.Avatar = avatar;
            //this.Nickname = nickname;
            this.UserFullname = userFullName;
            this.Level = level;
            this.LocationID = locationID;
            this.PreFix=preFix;
        }

        public long AccountID { get; set; }

        public string UserName { get; set; }

        public long TotalCoin { get; set; }

        public long TotalXu { get; set; }


        public bool isFacebook { get; set; }

        [JsonIgnore]
        public string EmailFull { get; set; }

        //  public long Star { get; set; }

        //    public int EventCoin { get; set; }
        //   public int EventStar { get; set; }

        public int IsOtp { get; set; }
        public string OtpToken { get; set; }

        [JsonIgnore]
        public string ClientIP { get; set; }

        [JsonIgnore]
        public int MerchantID { get; set; }

        [JsonIgnore]
        public int PlatformID { get; set; }

        public int Avatar { get; set; }
        //public int IsViewPayment { get; set; }
        //public int IsViewHotLine { get; set; }
        [JsonIgnore]
        public string Token { get; set; }

        [JsonIgnore]
        public string Email { get; set; }
        public string UserFullname { get; set; }
        
        public int Level { get; set; }


        //  OTP APP
        [JsonIgnore]
        public string AccountToken { get; set; }
        [JsonIgnore]
        public string Mobile { get; set; }
        [JsonIgnore]
        public bool IsMobileActived { get; set; }
        [JsonIgnore]
        public int IsAgency { get; set; }


        //  END OTP APP
        [JsonIgnore]
        public int LocationID { get; set; }
        [JsonIgnore]
        public string PreFix { get; set; }
        

        public bool IsEnoughMoney(long amount, byte moneyType)
        {
            if (amount < 0 || moneyType > 2)
            {
                return false;
            }
            else
            {
                if (moneyType == 0)
                {
                    return this.TotalXu >= amount;
                }
                else if (moneyType == 1)
                {
                    return this.TotalCoin >= amount;
                }
                return false;
            }
        }

        public AccountDb CloneWithMask()
        {
            AccountDb account = new AccountDb(this);
            account.UserName = StringUtil.MaskUserName(account.UserName);
            return account;
        }
    }

    public class AccountLoyalty
    {
        [JsonIgnore]
        public long AccountID { get; set; }
        public int VipPoint { get; set; }
        public int TotalVipPoint { get; set; }
        public int Level { get; set; }
        public string Mobile { get; set; }
    }
}