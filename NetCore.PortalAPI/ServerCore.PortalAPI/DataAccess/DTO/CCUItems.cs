using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerCore.DataAccess.DTO 
{
    public class CCUItems 
    {
        public int RoomType
        {
            get;
            set;
        }
        public int RoomValue
        {
            get;
            set;
        }
        public int Total
        {
            get;
            set;
        }
    }

    public class TotalCCU {
        public int GameId
        {
            get;
            set;
        }
        public int CCUBit
        {
            get;
            set;
        }
        public int CCUBac
        {
            get;
            set;
        }
    }

    public class CCUInfo {
        [JsonProperty("G")]
        public int GameId;
        [JsonProperty("C")]
        public int Ccu;
        [JsonProperty("Cir")]
        public List<CCURoomInfo> CcuInRoom;
    }

    public class CCURoomInfo {
        [JsonProperty("T")]
        public int RoomType;
        [JsonProperty("V")]
        public int RoomValue;
        [JsonProperty("C")]
        public int Ccu;
    }
}
