using System;
using System.Linq;

namespace ServerCore.Utilities.Models
{
    public class RoomDb
    {
        public RoomDb()
        {
        }

        //public byte RoomType { get; set; }
        public long RoomId { get; set; }

        public string RoomName { get; set; }

        public byte RoomStatus { get; set; }

        public byte GameId { get; set; }

        public byte MaxPlayer { get; set; }

        public byte MinLevel { get; set; }

        public byte MaxLevel { get; set; }

        public int MinBet { get; set; }

        public int MaxBet { get; set; }

        public bool IsPasswordProtected { get; set; }

        public string RuleDescription { get; set; }

        public DateTime CreatedTime { get; set; }

        public string Password { get; set; }

        public int BetStep { get; set; }

        public byte RoomType { get; set; }

        public byte NumberCurrentPlayers { get; set; }
    }
}