using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netcore.Chat.Models
{
    public class Admin
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string NickName { get; set; }
        public bool IsActive { get; set; }
    }
}
