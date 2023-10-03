using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netcore.Chat.Models
{
    public class BlockAccount
    {
        public int id { get; set; }
        public string name { get; set; }
        public int accountid { get; set; }
        public int reasonblock { get; set; }
        public string namereasonblock { get; set; }
        public int typeblock { get; set; }
        public DateTime endtimeblock { get; set; }
        public DateTime createDate { get; set; }
    }
}
