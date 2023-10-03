using Newtonsoft.Json;

namespace Netcore.Notification.Models
{
    public class XJackpotInfo
    {
        public bool IsEvent { get; set; }
        public string GameID { get; set; }
        public int EventJackpot { get; set; }
        public int NormalJackpot { get; set; }

        [JsonIgnore]
        public int RoomID { get; set; }

        public bool isCurrent { get; set; }
    }
}