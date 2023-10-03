namespace ServerCore.DataAccess.DTO
{
    public class TXBot
    {
        public long AccountID { get; set; }
        public string AccountName { get; set; }
        public string NickName { get; set; }
        public int Type { get; set; }
        public int BettingStartTime { get; set; }
        public int BettingEndTime { get; set; }
        public int BettingQuantity { get; set; }
        public long Balance { get; set; }
        public bool Status { get; set; }
        public int OnlineStartTime { get; set; }
        public int OnlineEndTime { get; set; }
    }
}