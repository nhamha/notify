using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.Utilities.Models
{
    [Serializable]
    public class AccountInfo
    {
        //[JsonIgnore]
        public long AccountID { get; set; }

 
        public string UserName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long TotalXu { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long TotalCoin { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Avatar { get; set; }

        public string NickName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Level { get; set; }

        [JsonIgnore]
        public bool IsOtp { get; set; }

        [JsonProperty("Token")]
        public string OtpToken { get; set; }
        public bool IsCaptcha { get; set; }
        //  OTP APP
        public string AccessToken { get; set; }
        public string AccessTokenFishing { get; set; }
        public bool IsMobileActived { get; set; }
        public bool IsAllowChat { get; set; }
        public bool IsAgency { get; set; }

        [JsonIgnore]
        public int ErrorCode { get; set; }

        [JsonIgnore]
        public string Mobile { get; set; }
        [JsonIgnore]
        public int LocationID { get; set; }
        //[JsonIgnore]
        public string PreFix { get; set; }
        //[JsonIgnore]
        //public string RefCode { get; set; }

        public AccountInfo(AccountDb accountDb)
        {
            AccountID = accountDb.AccountID;
            UserName = accountDb.UserName;
            TotalCoin = accountDb.TotalCoin;
            TotalXu = accountDb.TotalXu;
            Avatar = accountDb.Avatar;
            NickName = accountDb.UserFullname;
            Level = accountDb.Level;
            IsOtp = accountDb.IsOtp == 1 ? true : false;
            IsMobileActived = accountDb.IsMobileActived;
            IsAgency = accountDb.IsAgency == 1 ? true:false;
            LocationID=accountDb.LocationID;
            PreFix = accountDb.PreFix;
 //           RefCode = accountDb.RefCode;
            
        }

        public AccountInfo()
        {
        }
    }
}
