using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalAPI.Models
{
    public class BulkSendSmsTranferAgency
    {
        public string FromNickName{ get; set; }
        public string ToNickName{ get; set; }
        public string Amount{ get; set; }
        public string ToMobile{ get; set; }
        public string Balance{ get; set; }
        public string AmountType{ get; set; }

        public BulkSendSmsTranferAgency()
        {

        }
    }
}