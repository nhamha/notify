namespace Netcore.Notification.Models
{
    public class AppSettings
    {
        public string SQLConnectionString { get; set; }
        public string EventConnectionString { get; set; }
        public string Links { get; set; }
        public string EventLinks { get; set; }
        public string GameIDs { get; set; }
        public string EventGameIDs { get; set; }
        public string Secret { get; set; }
        public int Timer { get; set; }
        public string RoomValue { get; set; }
        public double SecondsRequest { get; set; }
        public int TimeWaitGetProfit { get; set; }

        public int TimeWaitVQMM { get; set; }

        public string OneSignal { get; set; }

        public bool IsAlpha { get; set; }
    }
}