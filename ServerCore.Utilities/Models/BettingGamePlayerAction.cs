using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerCore.Utilities.Models
{
    public class BettingGamePlayerAction
    {
        public byte GameId { get; set; }
        public long AccountId { get; set; }
        public byte ActionId { get; set; }
        public string ActionName { get; set; }
        public IEnumerable<int> Cards { get; set; }
    }
}
