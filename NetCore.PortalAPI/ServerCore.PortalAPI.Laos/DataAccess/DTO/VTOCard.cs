using System;

namespace ServerCore.DataAccess.DTO
{
    [Serializable]
    public class VTOCard
    {
        public long CardID { get; set; }
        
        public string CardCode { get; set; }

        public long OriginalCode { get; set; }

        public int CardValue { get; set; }

        public string AccountIP { get; set; }

        public int OrderID { get; set; }

        public int ServiceID { get; set; }

        public string ServiceKey { get; set; }

        public string AccountName { get; set; }

        public DateTime InputTime { get; set; }

        public int Error { get; set; }

        public string Prefix { get; set; }
    }
}