namespace Netcore.Notification.Models
{
    public class VQMMSpin
    {
        public int PrizeID { get; set; }
        public int PrizeValue { get; set; }
        public string PrizeName { get; set; }
        public long Balance { get; set; }
        public int ResponseCode { get; set; }
        public string Description { get; set; }
        public string GameName { get; set; }
        public int Remain { get; set; }
        public int FreeSpins { get; set; }
    }
}