using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore.Utilities.Models;

namespace ServerCore.DataAccess.DAO
{
    public interface IAgencyDAO
    {
        int CheckIsAgency(int accountID);
        int RegisterAgency(string userName, string agencyName, string phone);
        int EnableAgency(int accountId, int enable);

        /// <summary>
        /// Get agency qua nickName (account.UserFullName)
        /// </summary>
        /// <param name="NickName"></param>
        /// <returns></returns>
        AgenciesInfo AgencyGetInfo(string NickName);

        /// <summary>
        /// Lấy danh sách đại lý
        /// </summary>
        /// <param name="level"> 0: lấy tất, 1: lấy đại lý cấp 1, 2: cấp 2</param>
        /// <param name="status"> 2: lấy tất, 1: lấy đại lý đang hoạt động, 0: lấy đại lý không hoạt động</param>
        /// <param name="agencyParentID"></param>
        /// <returns> Danh sách đại lý</returns>
        List<Agency> GetAgencies(int level = 1, int status = 1, int agencyParentID = 0);
        List<Agency> GetAgenciesByAccountID(long accountid);

        List<GameAgency> GetGameAgencies();

        GameAgency GetGameAgency(int agencyAccountId);

        int RequestJoinAgency(int agencyAccountId, long accountId, string userName, string nickName);

        List<GameMember> GetListMember(long agencyAccountId, int status);

 

        List<GameJoinRequest> GetListRequestJoinAgency(long accountId, int status);

        long GetAgencyIDByMember(long accountId);
        int AcceptRequestInfo(long agencyAccountId, string nickName, int status, int isAgency);
        int UpdateRule(long agencyAccountId, string content);
        List<GameMember> GetListMemberOther(long agencyAccountId, int status);
    }
}
