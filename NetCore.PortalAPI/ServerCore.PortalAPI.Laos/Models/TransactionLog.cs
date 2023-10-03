using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.Models
{
    public class TransactionLog
    {
        public string serviceName { get; set; }
        public int amount { get; set; }
        public string description { get; set; }
        public string createdTime { get; set; }
        public int inOut { get; set; }
    }
}
