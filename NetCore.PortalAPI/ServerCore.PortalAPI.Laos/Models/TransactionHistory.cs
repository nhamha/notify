using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PortalAPI.Models
{
    public class TransactionHistory
    {
        [JsonProperty("accountID")]
        public int AccountID {get; set;}
        [JsonProperty("amount")]
        public int Amount {get; set;}
        [JsonProperty("inOut")]
        public int InOut { get; set; }
        [JsonProperty("createdTime")]
        public string CreatedTime {get; set;}
        [JsonProperty("description")]
        public string Description {get; set;}
        [JsonProperty("serviceID")]
        public string ServiceID {get; set;}
    }
}
