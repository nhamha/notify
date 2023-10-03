using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netcore.Chat.Models
{

    public class ChatDB
    {
        public int AccountID { get; set; }
        public string UserName { get; set; }
        public string ChannelID { get; set; }
        public string NickName { get; set; }
        public string Message { get; set; }
    }
}
