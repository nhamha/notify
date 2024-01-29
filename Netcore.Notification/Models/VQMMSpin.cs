using Newtonsoft.Json;

namespace Netcore.Notification.Models
{
    public class VQMMSpin
    {
        //[JsonIgnore]
        public string AccountName { get; set; }
        public int PrizeID { get; set; }
        public int PrizeValue { get; set; }
        public string PrizeName { get; set; } = "Trượt";
        public long Balance { get; set; } = -1;
        public int ResponseCode { get; set; }
        public string Description { get; set; }
        public string GameName { get; set; }
        public int Remain { get; set; }
        public int FreeSpins { get; set; }
        public int GameId { get; set; }
        public int RoomId { get; set; }
    }
}