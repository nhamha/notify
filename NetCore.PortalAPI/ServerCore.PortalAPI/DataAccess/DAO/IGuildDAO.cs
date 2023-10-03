using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore.Utilities.Models;

namespace ServerCore.DataAccess.DAO
{
    public interface IGuildDAO
    {
        GameAgency GetGameAgency(int agencyAccountId);
        List<GameAgency> GetGameAgencies();
        int RequestJoinAgency(int agencyAccountId, long accountId, string userName, string nickName);
        List<GameMember> GetListMember(long agencyAccountId, int status);
        List<GameJoinRequest> GetListRequestJoinAgency(long accountId, int status);
        long GetAgencyIDByMember(long accountId);
        int AcceptRequestInfo(long agencyAccountId, string nickName, int status, int isAgency);
        int UpdateRule(long agencyAccountId, string content);
        List<GameMember> GetListMemberOther(long agencyAccountId, int status);
    }
}
