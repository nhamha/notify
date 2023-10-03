namespace Netcore.Notification.Models
{
    //public class Football
    //{
    //    public long AccountId { get; set; }
    //    public string NickName { get; set; }
    //    public int Current { get; set; }
    //    public int Used { get; set; }
    //    public int Total { get; set; }
    //    public int PrizeId { get; set; }
    //    public string PrizeName { get; set; }
    //    public long PrizeValue { get; set; }
    //    public long Rank { get; set; }
    //    public int LastChange { get; set; }
    //    public DateTime CreatedTime { get; set; }
    //}
    //public class LoyaltyPrize
    //{
    //    public int AccountId { get; set; }
    //    public int PrizeId { get; set; }
    //    public string PrizeName { get; set; }
    //    public int Status { get; set; }
    //}
    public class LoyaltyAccount
    {
        public string NickName { get; set; }
        public int PrizeId { get; set; }
        public string PrizeName { get; set; }
        public int Status { get; set; }
        public int Rank { get; set; }
        public long TotalMoney { get; set; }
        public int Total { get; set; }
        public int PrizeValue { get; set; }
        public int Id { get; set; }
    }
}