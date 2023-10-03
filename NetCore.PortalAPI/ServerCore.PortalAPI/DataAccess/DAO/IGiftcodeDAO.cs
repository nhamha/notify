using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DAO
{
    public interface IGiftcodeDAO
    {
        int Giftcode(int accountId, string username, string giftcode, string clientIP, string mobile, ref int value);
    }
}
