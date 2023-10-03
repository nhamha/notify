using System;

namespace ServerCore.DataAccess.DTO
{
    [Serializable]
    public class ChannelingAccount
    {
        public long ChannelingID { get; set; }

        public long AccountID { get; set; }

        public string AccountName { get; set; }

        public int Partner { get; set; }

        public string PartnerAccount { get; set; }

        public DateTime ChannelingTime { get; set; }

        public string PartnerAccountID { get; set; }

    }
}