using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netcore.Notification.Models
{
    public class ShareProfit
    {
        public string AccountName { get; set; }
        public long TotalValue { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
