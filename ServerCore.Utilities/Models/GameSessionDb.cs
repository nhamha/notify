using System;
using System.Linq;

namespace ServerCore.Utilities.Models
{
    public class SessionDb
    {
        public SessionDb()
        {
        }

        public long GameSessionId { get; set; }

        public byte GameId { get; set; }

        public string PlayersData { get; set; }

        public long RoomId { get; set; }

        public DateTime CreatedTime { get; set; }

        public bool Status { get; set; }

        public DateTime EndTime { get; set; }

        public long GameOwnerId { get; set; }

        public string RoomData { get; set; }
    }
}