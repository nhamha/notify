using System;
using System.Linq;

namespace ServerCore.Utilities.Models
{
    public class RoomConfigDb
    {
        public RoomConfigDb()
        {
        }

        public int RoomConfigId { get; set; }

        public int MinBet { get; set; }

        public int MaxBet { get; set; }

        public int MinLevel { get; set; }

        public int MaxLevel { get; set; }

        public bool IsFriendOnly { get; set; }

        public byte MoneyType { get; set; }
    }
}