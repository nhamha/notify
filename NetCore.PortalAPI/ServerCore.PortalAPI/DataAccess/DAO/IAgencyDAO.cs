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
        List<Agency> GetAgenciesClient(int locationId, int level = -1, int status = 1, int agencyParentID = 0);

        List<AgencyBank> GetAgenciesBank();

        double GetTransferRate(int type, int levelId);
        double GetTransferRateNew(string nickNameTran, string nickNameRev);
    }
}
