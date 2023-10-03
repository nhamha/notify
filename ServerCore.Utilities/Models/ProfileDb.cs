using System;
using Newtonsoft.Json;

namespace ServerCore.Utilities.Models
{
    public class ProfileDb
    {
        public byte GameID { get; set; }
        public long AccountID { get; set; }

        [JsonProperty("NickName")]
        public string CharacterName { get; set; }
        public long TotalMoneyWins { get; set; }
        public int TotalWin { get; set; }
        public int TotalLose { get; set; }
        public byte RoomType { get; set; }
        public int ContinuousWins { get; set; }
        public int TotalPlayGameInDay { get; set; }
        public int Rank { get; set; }
        [JsonProperty("IP")]
        public string IpAddress { get; set; }

        [JsonIgnore]
        public string AccountName { get; set; }
        [JsonIgnore]
        public long CharacterID { get; set; }
        [JsonIgnore]
        public int Experiences { get; set; }
        [JsonIgnore]
        public byte Level { get; set; }
        [JsonIgnore]
        public bool Gender { get; set; }
        [JsonIgnore]
        public string AttributeData { get; set; }
        [JsonIgnore]
        public long TotalMoneyLoses { get; set; }
        [JsonIgnore]
        public string Achivements { get; set; }
        [JsonIgnore]
        public long TotalPoint { get; set; }
        [JsonIgnore]
        public DateTime LastCheckDate { get; set; }
        [JsonIgnore]
        public int NumberLoginTimes { get; set; }
        [JsonIgnore]
        public int RankInWeek { get; set; }
        [JsonIgnore]
        public int BestHistoryWinContinous { get; set; }
        [JsonIgnore]
        public long TotalMoneyWinsInWeek { get; set; }
        [JsonIgnore]
        public int TotalWinInWeek { get; set; }
    }
}