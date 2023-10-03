using System;
using System.Threading;

namespace ServerCore.Utilities.Models
{
    public enum ConnectionStatus
    {
        DISCONNECTED = 0,
        CONNECTED = 1,
        REGISTER_LEAVE_GAME = 2
    }

    public enum PlayerStatus
    {
        NOT_INGAME = -1,
        VIEWER = 0,
        INGAME = 1,
        WAITING = 2
    }

    public enum AccountInfoType
    {
        BON = 1,
        BAC = 2,
        ALL = 0
    }

    public class TransactionCoreResponse
    {
        public DateTime CreatedTime { get; set; }

        public string ServiceName { get; set; }

        public string Money { get; set; }

        public string Description { get; set; }
    }

    public class IdGenerator
    {
        private readonly static Lazy<IdGenerator> _instance = new Lazy<IdGenerator>(() => new IdGenerator());
        private long _countSessionId = 0;
        private long _countGameLoopId = 0;

        private IdGenerator()
        {
        }

        public static IdGenerator Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public long NextSessionId()
        {
            return Interlocked.Increment(ref _countSessionId);
        }

        public long NextGameLoopId()
        {
            return Interlocked.Increment(ref _countGameLoopId);
        }
    }
}