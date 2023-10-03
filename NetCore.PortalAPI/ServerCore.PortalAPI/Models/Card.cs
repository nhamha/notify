using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalAPI.Models
{
    public class ChargingCard
    {
        public int merchantId
        { get; set; }
        public string merchantKey
        { get; set; }
        public string cardType { get; set; }
        public string cardSeri { get; set; }
        public string cardPin
        {
            get; set;
        }
        public string token
        {
            get; set;
        }
        public string userName
        {
            get; set;
        }
        public long accountId
        {
            get;
            set;
        }
        public int sourceId
        {
            get; set;
        }
        public string clientIp
        {
            get; set;
        }
    }
}
