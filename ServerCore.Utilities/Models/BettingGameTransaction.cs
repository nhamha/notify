using System;
using System.Linq;

namespace ServerCore.Utilities.Models
{
    public class BettingGameTransaction
    {
        public long AccountId { get; set; }
        public string Username { get; set; }
        public long Amount { get; set; }
        public byte MoneyType { get; set; }
        public byte Action { get; set; }
        public string Description { get; set; }
        public long RefAcccountId { get; set; }
    }
}