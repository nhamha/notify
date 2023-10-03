namespace ServerCore.DataAccess.DTO
{
    public class VQMMSpin
    {
        public int PrizeID1 { get; set; }
        public int PrizeID2 { get; set; }
        public int PrizeValue1 { get; set; }
        public int PrizeValue2 { get; set; }
        public string PrizeName1 { get; set; }
        public string PrizeName2 { get; set; }
        public long Balance { get; set; }
        public long SilverBalance { get; set; }
        public int Remain { get; set; }
        public int ResponseCode { get; set; }
        public string Description { get; set; }
    }
}