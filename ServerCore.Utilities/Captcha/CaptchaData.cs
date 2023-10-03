using System;

namespace ServerCore.PortalAPI.CaptchaNew
{
    public class CaptchaData
    {
        private readonly static int TOKEN_EXPIRE = 5;//minute
        public string CaptchaText
        {
            get; set;
        }

        public long CreateTime
        {
            get; set;
        }

        public string AuthKey
        {
            get; set;
        }

        public CaptchaData()
        {
            
        }

        public CaptchaData(string captchaText, long createTime, string authKey)
        {
            CaptchaText = captchaText;
            CreateTime = createTime;
            AuthKey = authKey;
        }

        public bool IsExpired()
        {
            long tickNow = DateTime.Now.Ticks;
            TimeSpan diffTime = new TimeSpan(tickNow - CreateTime);
            if(diffTime.TotalMinutes > TOKEN_EXPIRE)
                return false;
            return true;
        }
    }
}
