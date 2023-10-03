using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalAPI.Models
{
    public class EventX2
    {
        public int TotalCharge { get; set; }
        public long TotalBetMin { get; set; }
        public long TotalBet { get; set; }
        public int  Type { get; set; }          
        public long PrizeValue { get; set; }
        public string CreateTime { get; set; }
    }
}
