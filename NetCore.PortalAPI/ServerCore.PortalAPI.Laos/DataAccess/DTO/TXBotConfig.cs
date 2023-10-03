namespace ServerCore.DataAccess.DTO
{
    public class TXBotConfig
    {
        public int RealUserQuantity { get; set; }
        public double Multiplier { get; set; }
        public int MinAI { get; set; }
        public int MaxAI { get; set; }
        public int LimitBet { get; set; }
        public double Rate { get; set; }
        public double Percent { get; set; }
    }
}