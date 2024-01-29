namespace Netcore.Notification.Models
{
    public class Quest
    {
        public long Id { get; set; }
        public string Weekday { get; set; }
        public string Description { get; set; }
        public long Award { get; set; }
        public int Quantity { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public int RequireQuantity { get; set; }
        public int IsDone { get; set; }
        public string Name { get; set; }
        public int GameId { get; set; }
    }
    public class ExchangeRate
    {
        public int Id { get; set; }
        public int Point { get; set; }
        public int Money { get; set; }

    }
}