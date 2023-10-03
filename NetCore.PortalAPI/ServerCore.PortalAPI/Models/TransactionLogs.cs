using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.Models
{
    public class TransactionLogs
    {
        public int code { get; set; }
        public string description { get; set; }
        public List<TransactionLog> data { get; set; }
    }
}
