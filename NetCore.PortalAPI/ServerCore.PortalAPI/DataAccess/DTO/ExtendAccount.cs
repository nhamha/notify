using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    [Serializable]
    public class ExtendAccount
    {
        public long AccountID { get; set; }
        public bool isLiked { get; set; }
        public bool isVoted { get; set; }
        public int Times { get; set; }
    }
}
