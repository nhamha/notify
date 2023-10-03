using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    public class InvitedEvent
    {
        public int AccountID { get; set; }
        public int InvitedQuantity { get; set; }
        public long Bonus { get; set; }
        public int Offset { get; set; }
    }
}
