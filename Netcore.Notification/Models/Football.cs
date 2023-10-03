using System;

namespace Netcore.Notification.Models
{
    public class Football
    {
        public long AccountId { get; set; }
        public string NickName { get; set; }
        public int Current { get; set; }
        public int Used { get; set; }
        public int Total { get; set; }
        public int PrizeId { get; set; }
        public string PrizeName { get; set; }
        public long PrizeValue { get; set; }
        public long Rank { get; set; }
        public int LastChange { get; set; }
        public DateTime CreatedTime { get; set; }
    }
    public class FootballGiftPrize
    {
        public int PrizeId { get; set; }
        public string PrizeName { get; set; }
        public long PrizeValue { get; set; }
        public int Quantity { get; set; }
        public int Cost { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public int Percent { get; set; }
    }
    public class FootballBetAccount
    {
        public int PrizeId { get; set; }
        public string PrizeName { get; set; }
        public int TotalBetValue { get; set; }
        public int Status { get; set; }

    }
    public class FootballTime
    {
        public DateTime StartDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Phase { get; set; }
    }

    public class FootballPlayer
    {
        public int PrizeId { get; set; }
        public string PrizeName { get; set; }
        public int Result { get; set; }
        public int Multiplier { get; set; }
        public int TotalBetValue { get; set; }
        public int Status { get; set; }
    }
}