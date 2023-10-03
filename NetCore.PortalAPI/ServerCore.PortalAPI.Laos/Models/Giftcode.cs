using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalAPI.Models
{
    public class Giftcode
    {
        //public int accountId { get; set; }
        //public string accountName { get; set; }
        //public string nickName { get; set; }
        public string giftCode { get; set; }
        public string captcha { get; set; }
        public string verifyCaptcha { get; set; }
        public string merchantKey { get; set; }
        public int merchantId { get; set; }
        public int sourceId { get; set; }
    }
}
