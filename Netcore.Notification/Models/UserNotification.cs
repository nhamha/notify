using Newtonsoft.Json;
using System;

namespace Netcore.Notification.Models
{
    public class UserNotification
    {
        public long NotifyID { get; set; }

        [JsonIgnore]
        public long AccountID { get; set; }

        public string UserName { get; set; }

        // public string UserName { get; set; }
        // public int GameID { get; set; }
        public long Amount { get; set; }

        [JsonIgnore]
        public string Message { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }

        [JsonIgnore]
        public string Icon { get; set; }

        // public int Status { get; set; }
        public bool IsRead { get; set; }

        [JsonIgnore]
        public bool IsSend { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GiftCode { get; set; }
        public string ImageUrl { get; set; }
    }

    public class UserQuantityNotification
    {
        public int Quantity { get; set; }
    }
}