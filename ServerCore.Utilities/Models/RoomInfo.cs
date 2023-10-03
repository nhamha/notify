using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerCore.Utilities.Models
{
    public class RoomInfo {
        [JsonProperty("rn")]
        public string RoomName{ get; set; }
        [JsonProperty("rv")]
        public int RoomValue{ get; set; }
        [JsonProperty("pr")]
        public int PlayersInRoom{ get; set; }
        [JsonProperty("rt")]
        public byte RoomType{ get; set; }
        [JsonProperty("rj")]
        public int RoomJackpot{ get; set; }
        [JsonProperty("rp")]
        public bool IsPrivate{get; set;}
        [JsonProperty("pw")]
        public string PassWord{get; set;}
    }
}
