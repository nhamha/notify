using System;
using System.Linq;

namespace ServerCore.Utilities.Models
{
    public class GameTransactionDb
    {
        public GameTransactionDb()
        {
        }

        public long GameTransactionId { get; set; }

        public long GameSessionId { get; set; }

        public long RoomId { get; set; }

        public string TransactionData { get; set; }

        public DateTime CreatedTime { get; set; }

        public string Players { get; set; }
    }
}