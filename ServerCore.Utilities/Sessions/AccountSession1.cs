using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerCore.Utilities.Interfaces;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;
using System;
using System.Net;
using System.Security.Claims;
using System.Web;

namespace ServerCore.Utilities.Sessions
{
    public class AccountSession1 : IAccountSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountSession1(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public AccountInfo GetAccountInfo()
        {
            AccountInfo accountInfo = new AccountInfo();
            accountInfo.AccountID = GetAccountID();
            accountInfo.NickName = GetNickName();
            accountInfo.UserName = GetAccountName();
            return accountInfo;
        }

        public long GetAccountID()
        {
            long accountId = 0;
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string val = _httpContextAccessor.HttpContext.User.FindFirst("AccountId").Value;
                accountId = Int64.Parse(val);
            }
            return accountId;
        }

        public string GetAccountName()
        {
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string accountName = _httpContextAccessor.HttpContext.User.FindFirst("UserName").Value;
                return accountName;
            }
            return "";
        }

        public string GetNickName()
        {
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string nickName = _httpContextAccessor.HttpContext.User.FindFirst("NickName").Value;
                return nickName;
            }
            return "";
        }

        public int GetMerchantID()
        {
            int merchantId = 0;
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string val = _httpContextAccessor.HttpContext.User.FindFirst("MerchantId").Value;
                merchantId = Int32.Parse(val);
            }
            return merchantId;
        }

        public int GetPlatformID()
        {
            int platformId = 0;
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string val = _httpContextAccessor.HttpContext.User.FindFirst("PlatformId").Value;
                platformId = Int32.Parse(val);
            }
            return platformId;
        }

        public string GetIpAddress()
        {
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            return "";
        }

        public string GetToken()
        {
            return "";
        }
    }
}