using Newtonsoft.Json;
using System;

namespace ServerCore.Utilities.Models
{

    public class TopupHistory
    {
        public string UserName{ get; set; }
        public string Type{ get; set; }
        public string CardSerial{ get; set; }
        public int Amount{ get; set; }
        public DateTime CreatedTime { get; set; }
        public int ErrorCode{ get; set; }
    }

    public class UserTransactionHistory
    {
        public string ServiceName{ get; set; }
        public string Descriptions{ get; set; }
        public long Amount{ get; set; }
        public DateTime CreatedTime { get; set; }
        public long TransactionID{ get; set; }
    }

    public class TopupXuHistory
    {
        public int Id {get; set;}
        [JsonProperty("TotalCoin")]
        public int Bit {get; set;}
        [JsonProperty("Xu")]
        public int Silver {get; set;}
        public DateTime CreatedDate {get; set;}
        [JsonIgnore]
        public int TimeInt {get; set;}
    }

    public class TransferBonHistory
    {
        public long TranferID { get; set; }
        public long AccountID {get; set;}
        public int TypePurchase { get; set; }
        [JsonIgnore]
        public string UserName {get; set;}
        public string NickName { get; set; }
        public DateTime CreateDate {get; set;}
        public long Value {get; set;}
        public long TAX { get; set; }
        public string ReceiverNickName { get; set; }
        public long ReceiverValue { get; set; }
        public long ReceiverAccountID { get; set; }
        public string Description { get; set; }
    }
}