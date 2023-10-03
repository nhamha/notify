using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ServerCore.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ServerCore.Utilities.Sessions
{
    public class AccountSession
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;

        public AccountSession(IHttpContextAccessor httpContextAccessor, IDistributedCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
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
                string val = "";
                if (Current.User.Identity.IsAuthenticated)
                {
                    val = Current.User.FindFirst("UserName").Value;
                }
                return val;
            }
        }

        //public string AccountFullName
        //{
        //    get
        //    {
        //        string val = "";
        //        if (Current.User.Identity.IsAuthenticated)
        //        {
        //            val = Current.User.FindFirst("AccountFullName").Value;
        //        }
        //        return val;
        //    }
        //}

        public string NickName
        {
            get
            {
                if (Current.User.Identity.IsAuthenticated)
                {
                    string nickName = Current.User.FindFirst("NickName").Value;
                    return nickName;
                }
                return "";
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
                    merchantId = Int32.Parse(val);
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
                    platformId = Int32.Parse(val);
                }
                return platformId;
            }
        }

        public string IpAddress
        {
            get
            {
                //if (Current.User.Identity.IsAuthenticated)
                //{
                //    return Current.Connection.RemoteIpAddress.ToString();
                //}
                //return "";

                string header = (Current.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? Current.Request.Headers["X-Forwarded-For"].FirstOrDefault());
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                    return ip.ToString();
                }
                return Current.Connection.RemoteIpAddress.ToString();
            }
        }

        public string Language
        {
            get
            {
                if (Current != null && Current.Request.Headers.ContainsKey("Accept-Language"))
                {
                    string lang = Current.Request.Headers["Accept-Language"];
                    if (lang == null || lang.Length <= 0)
                        return "vi";
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
                    isOtp = Boolean.Parse(val);
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
                    locationID = Int32.Parse(val);
                }
                return locationID;
            }
        }


        private bool isValid()
        {
            if (Current.User.Identity.IsAuthenticated)
            {
                string val = Current.User.FindFirst("AccountId").Value;
                string authHeader = Current.Request.Headers["Authorization"];
                string accountCache = _cache.GetString(val);
                if (!string.IsNullOrEmpty(accountCache))
                {
                    List<AccessTokenCache> list = JsonConvert.DeserializeObject<List<AccessTokenCache>>(accountCache);
                    foreach (var it in list)
                    {
                        if (it.AccessToken.Equals(authHeader.Substring("Bearer ".Length).Trim()) && it.IsExpired())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
