using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netcore.Chat.Models
{
    public class AccountBlockList
    {
        public int AccountID { get; set; }
        public string UserName { get; set; }
        public string PeopleExecute { get; set; }
        public string Reason { get; set; }
        public int Status { get; set; }
        public DateTime DatetimeBlock { get; set; }
    }
}
