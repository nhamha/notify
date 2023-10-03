using Newtonsoft.Json;
using System;

namespace Netcore.Notification.Models
{
    public class SystemNotification
    {
        public string Name { get; set; }
        public string Message { get; set; }
        [JsonIgnore]
        public string Icon { get; set; }
        [JsonIgnore]
        public DateTime EffectDate { get; set; }
        [JsonIgnore]
        public DateTime ExpireDate { get; set; }
        [JsonIgnore]
        public int Type { get; set; }
        public int NType { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
        public int GameID { get; set; }
        public string Link { get; set; }
        public long Amount { get; set; }
        public string UserName { get; set; }
        [JsonIgnore]
        public string CardTypeName { get; set; }
    }
    //public class UserNotification
    //{
    //    public int AccountID { get; set; }
    //    public string UserName { get; set; }
    //    public string Message { get; set; }
    //    public int Type { get; set; }
    //    public string Icon { get; set; }
    //    public bool IsRead { get; set; }
    //    public bool IsSend { get; set; }
    //    public string Title { get; set; }
    //    public string Content { get; set; }
    //}
}