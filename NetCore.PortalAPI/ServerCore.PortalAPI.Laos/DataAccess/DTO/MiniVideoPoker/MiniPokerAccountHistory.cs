using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    public class MiniPokerAccountHistory
    {
        public long SpinID { get; set; }
        public DateTime CreatedTime { get; set; }
        public int RoomID { get; set; }
        public long BetValue { get; set; }
        public int CardTypeID { get; set; }
        public long PrizeValue { get; set; }
        public string CardResult { get; set; }
    }
}
