using System;

namespace Netcore.Notification.Models
{
    public class PopupNotification
    {
        public long NotifyID { get; set; }
        public long AccountID { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public int BValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ReceiveDate { get; set; }
        public bool IsRead { get; set; }
        public bool nStatus { get; set; }
    }
}