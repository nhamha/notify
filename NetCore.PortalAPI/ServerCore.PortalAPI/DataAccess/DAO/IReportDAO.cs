using ServerCore.PortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DAO
{
    public interface IReportDAO
    {
        /// <summary>
        /// Kiem tra the co bao tri khong
        /// </summary>
        /// <param name="cardType">VTT, VMS, VNP, GATE</param>
        /// <param name="topupType">0 = mua the, 3 = nap the</param>
        /// <returns>true=bao tri, false=Khong bao tri</returns>
        string CheckCardMaintain(string cardType, int topupType);
        UserRevenueAffilicateModel GetUserRevenueAffilicate(long accountID, int fromDate, int toDate);
        List<TransactionLog> GetTransactionLog(long accountID, int limit);
        InfoRevenueAffiliate GetInfoRevenueAffiliate(long accountID, int fromDate, int toDate);
        List<HistoryDeductRevenue> GetHistoryDeductRevenue(long accountID);
        int WithdrawAffiliate(long accountID, string nickName, long withdrawValue, string ipAddress, int sourceId, out long balanceNew);
	}
}
