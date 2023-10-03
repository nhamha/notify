using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerCore.DataAccess.DTO
{
    /// <summary>
    /// Data trả về cho client
    /// </summary>
    public class MiniPokerSpinResponse
    {
        public int AccountID { get; set; }
        public int BetType { get; set; }
        public long SpinID { get; set; }
        public long BetValue { get; set; }
        public long PrizeValue { get; set; }
        public long Balance { get; set; }
        public long Jackpot { get; set; }
        public int ResponseStatus { get; set; }
        public List<MiniPokerListCardModel> Cards { get; set; }
    }
}