using Newtonsoft.Json;
using ServerCore.DataAccess;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.OTP
{
    /// <summary>
    /// OtpData
    /// </summary>
    /// <param name="OtpType">
    /// 1 = OTP SMS
    /// 2 = OTP App
    /// </param>
    /// /// <param name="OtpToken">
    /// Chuỗi server trả về client cho mỗi dịch vụ cần OTP, client phải gửi lên server cùng với mã OTP
    /// </param>
    public class OtpData
    {
        private readonly static int TOKEN_EXPIRE = 15;//minute
        public string AccountName
        {
            get; set;
        }

        public long AccountId
        {
            get; set;
        }

        public long CreateTime
        {
            get; set;
        }

        public int ServiceId
        {
            get; set;
        }

        public int PlatformId
        {
            get; set;
        }

        public string ServiceData
        {
            get; set;
        }
        public OtpData(string accountName, long accountId, long createTime, int serviceId, int platformId, string serviceData)
        {
            AccountId = accountId;
            AccountName = accountName;
            CreateTime = createTime;
            ServiceId = serviceId;
            PlatformId = platformId;
            ServiceData = serviceData;
        }

        public bool IsExpired()
        {
            long tickNow = DateTime.Now.Ticks;
            TimeSpan diffTime = new TimeSpan(tickNow - CreateTime);
            if(diffTime.TotalMinutes > TOKEN_EXPIRE)
                return false;
            return true;
        }

        public bool IsServiceOK(int serviceId)
        {
            if(ServiceId != serviceId)
                return false;

            return true;
        }
    }
}
