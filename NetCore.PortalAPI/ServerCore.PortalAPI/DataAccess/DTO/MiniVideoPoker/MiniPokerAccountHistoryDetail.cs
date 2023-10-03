using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    public class MiniPokerAccountHistoryDetail
    {
        public DateTime CreatedTime { get; set; }
        public int RoomID { get; set; }
        public long BetValue { get; set; }
        public int CardTypeID { get; set; }
        public long PrizeValue { get; set; }
        public int CardID1 { get; set; }
        public int CardID2 { get; set; }
        public int CardID3 { get; set; }
        public int CardID4 { get; set; }
        public int CardID5 { get; set; }
    }
}
