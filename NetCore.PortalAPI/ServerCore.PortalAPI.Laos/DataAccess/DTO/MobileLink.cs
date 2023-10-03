using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    [Serializable]
    public class MobileLink
    {
        public int id { get; set; }
        public int game_id { get; set; }
        public string name { get; set; }
        public string domain { get; set; }
        public string hub_name { set; get; }
        public string hub { set; get; }
        public string ext { set; get; }
        public int type { get; set; }
        public bool is_maintain { get; set; }
    }

    [Serializable]
    public class MobileLink3
    {
        public int id { get; set; }
        public int game_id { get; set; }
        public string name { get; set; }
        public string url{ set; get; }
        public string hub { set; get; }
        public string ext { set; get; }
        public int type { get; set; }
        public bool is_maintain { get; set; }
        public string bit { set; get; }
        public string bac{ set; get; }
    }
}
