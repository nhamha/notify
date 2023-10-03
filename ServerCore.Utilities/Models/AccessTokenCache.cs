using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore.Utilities.Models
{
    public class AccessTokenCache
    {
        public string AccessToken { get; set; }
        public string Uiid { get; set; }
        public long AccountID { get; set; }
        public long CreatedTime { get; set; }
        public int ExpireTime { get; set; }

        public AccessTokenCache(string accessToken, string uiid, long accountID, int expireTime)
        {
            AccessToken = accessToken;
            Uiid = uiid;
            AccountID = accountID;
            CreatedTime = DateTime.Now.Ticks;
            ExpireTime = expireTime;
        }

        public bool IsExpired()
        {
            long tickNow = DateTime.Now.Ticks;
            TimeSpan diffTime = new TimeSpan(tickNow - CreatedTime);
            if (diffTime.TotalHours > ExpireTime)
                return false;
            return true;
        }
    }
}
