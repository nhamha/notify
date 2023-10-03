using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.Models
{
    public class TransactionLog
    {
        public string ServiceName { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public int InOut { get; set; }
    }
}
