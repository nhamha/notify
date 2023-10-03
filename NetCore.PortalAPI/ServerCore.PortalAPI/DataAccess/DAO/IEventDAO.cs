using PortalAPI.Models;
using ServerCore.DataAccess.DTO;
using System;
using System.Collections.Generic;

namespace ServerCore.DataAccess.DAO
{
    public interface IEventDAO
    {
        List<EventX2> GetEvent(long accountId);
        int CheckGiftCode(long accountId, string accountName, string nickName, string giftcode, int merchantId, string merchantKey, int sourceId, string ip, out long balance);
    }
}