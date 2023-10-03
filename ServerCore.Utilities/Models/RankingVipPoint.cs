using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.Utilities.Models
{
    public class RankingVipPoint
    {
        public int Ranking { get; set; }
        public int Point { get; set; }
        public string RankingName { get; set; }
        public int MinExchangePoint { get; set; }
    }
}
