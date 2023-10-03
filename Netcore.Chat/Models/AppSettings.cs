using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netcore.Chat.Models
{
    public class AppSettings
    {
        public string ConnectionString { get; set; }
        public int TimeInterval { get; set; }
        public int MaxIdleUserOnline { get; set; }
        public int MaxMessageInChanel { get; set; }
        public int MaxMessageLength { get; set; }
        public int MinUserInactiveInChanel { get; set; }
        public int TwoMessageDuration { get; set; }
        public int TenMessageDuration { get; set; }
        public int DuplicateMessageDuration { get; set; }
        public int GlobalTenSameMessageDuration { get; set; }
        public int NotificationDelay { get; set; }
        public string ApiAuthenAccount { get; set; }
        public string KeyApi { get; set; }
        public double SecondsRequest { get; set; }
        public string Secret { get; set; }
        public string CloseChatServer { get; set; }
        public int MinMessageTimeSecond { get; set; }
        public string VippointURL { get; set; }
    }
}
