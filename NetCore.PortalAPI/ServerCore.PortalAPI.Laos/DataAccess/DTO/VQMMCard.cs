using System;

namespace ServerCore.DataAccess.DTO
{
    public class VQMMCard
    {
        public long SpinID { get; set; }
        public int CardValue { get; set; }
        public string CardType { get; set; }
        public string CardCode { get; set; }
        public string CardSeri { get; set; }
        public int Status { get; set; } = -1;
        public string Description { get; set; }
        public long AccountID { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}