using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    [Serializable]
    public class MobileVersion
    {
        public int id { get; set; }
        public int version { get; set; }
        public string link { get; set; }
        public string link_market { get; set; }
        public bool is_maintain { get; set; }
        public string description { get; set; }
        public string app_name { get; set; }
    }
}
