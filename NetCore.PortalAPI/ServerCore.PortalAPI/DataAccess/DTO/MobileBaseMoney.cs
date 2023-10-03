using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    [Serializable]
    public class MobileBaseMoney
    {
        public int id { get; set; }
        public string star { get; set; }
        public string stone { get; set; }
        public int game_id { get; set; }
    }
}
