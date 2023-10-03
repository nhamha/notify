namespace Netcore.Notification.Models
{
    public class Deposit
    {
        public long Id { get; set; }
        public int PrizeId { get; set; }
        public string PrizeName { get; set; }
        public long PrizeValue { get; set; }
        public int Quantity { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public long AccountId { get; set; }
        public int DepositValue { get; set; }
    }
}