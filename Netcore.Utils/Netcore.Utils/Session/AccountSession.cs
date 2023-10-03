using Microsoft.AspNetCore.Http;
using NetCore.Utils.Log;
using System;

namespace NetCore.Utils.Sessions
{
    public class AccountSession
    {
        private IHttpContextAccessor _httpContextAccessor;

        public AccountSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public HttpContext Current => _httpContextAccessor.HttpContext;

        public long AccountID
        {
            get
            {
                long accountId = 0;
                if (Current.User.Identity.IsAuthenticated)
                {
                    string val = Current.User.FindFirst("AccountId").Value;
                    accountId = Int64.Parse(val);
                }
                return accountId;
            }
        }

        public string AccountName
        {
            get
            {
                try
                {
                    string val = "";
                    if (Current.User.Identity.IsAuthenticated)
                    {
                        val = Current.User.FindFirst("UserName").Value;
                    }
                    return val;
                }
                catch (Exception)
                {

                    return string.Empty;
                }
            }
        }

        public string NickName
        {
            get
            {
                try
                {
                    if (Current.User.Identity.IsAuthenticated)
                    {
                        string nickName = Current.User.FindFirst("NickName").Value;
                        return nickName;
                    }
                    return string.Empty;
                }
                catch (Exception)
                {

                    return string.Empty;
                }
            }
        }

        public int MerchantID
        {
            get
            {
                int merchantId = 0;
                if (Current.User.Identity.IsAuthenticated)
                {
                    string val = Current.User.FindFirst("MerchantId").Value;
                    merchantId = int.Parse(val);
                }
                return merchantId;
            }
        }

        public int PlatformID
        {
            get
            {
                int platformId = 0;
                if (Current.User.Identity.IsAuthenticated)
                {
                    string val = Current.User.FindFirst("PlatformId").Value;
                    platformId = int.Parse(val);
                }
                return platformId;
            }
        }

        public int SourceID
        {
            get
            {
                int platformId = 0;
                if (Current.User.Identity.IsAuthenticated)
                {
                    string val = Current.User.FindFirst("PlatformId").Value;
                    platformId = int.Parse(val);
                }
                return platformId;
            }
        }

        public bool IsAgency
        {
            get
            {
                if (Current.User.Identity.IsAuthenticated)
                {
                    var claim = Current.User.FindFirst("IsAgency");
                    string val = claim == null ? "0" : claim.Value;
                    return int.Parse(val) == 1;
                }
                return false;
            }
        }

        public string IpAddress
        {
            get
            {
                if (Current.User.Identity.IsAuthenticated)
                {
                    return Current.Connection.RemoteIpAddress.ToString();
                }
                return "";
            }
        }

        public string Language
        {
            get
            {
                if (Current != null && Current.Request.Headers.ContainsKey("Accept-Language"))
                {
                    string lang = Current.Request.Headers["Accept-Language"];
                    return lang.ToLower();
                }
                return "vi";
            }
        }
        public bool IsOTP
        {
            get
            {
                bool isOtp = false;
                if (Current.User.Identity.IsAuthenticated)
                {
                    string val = Current.User.FindFirst("IsOTP").Value;
                    isOtp = bool.Parse(val);
                }
                return isOtp;
            }
        }

        public string RefCode
        {
            get
            {
                //string refCode = '';
                if (Current.User.Identity.IsAuthenticated)
                {
                    string refCode = Current.User.FindFirst("RefCode").Value;
                    return refCode;
                }
                return "";
            }
        }
        public string PreFix
        {
            get
            {
                //string refCode = '';
                if (Current.User.Identity.IsAuthenticated)
                {
                    string preFix = Current.User.FindFirst("PreFix").Value;
                    return preFix;
                }
                return "";
            }
        }
   
        public int LocationID
        {
            get
            {
                int locationID = 0;
                if (Current.User.Identity.IsAuthenticated)
                {
                    string val = Current.User.FindFirst("LocationID").Value;
                    locationID = int.Parse(val);
                }
                return locationID;
            }
        }
    }
}