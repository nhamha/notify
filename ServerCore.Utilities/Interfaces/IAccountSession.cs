using Microsoft.AspNetCore.Http;
using ServerCore.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore.Utilities.Interfaces
{
    public interface IAccountSession
    {
        //AccountInfo GetAccountInfo(HttpContext httpContext);
        //long GetAccountID(HttpContext httpContext);
        //string GetAccountName(HttpContext httpContext);
        //string GetNickName(HttpContext httpContext);
        //int GetMerchantID(HttpContext httpContext);
        //int GetPlatformID(HttpContext httpContext);
        //string GetIpAddress(HttpContext httpContext);
        //string GetToken(HttpContext httpContext);

        AccountInfo GetAccountInfo();
        long GetAccountID();
        string GetAccountName();
        string GetNickName();
        int GetMerchantID();
        int GetPlatformID();
        string GetIpAddress();
        string GetToken();
    }
}
