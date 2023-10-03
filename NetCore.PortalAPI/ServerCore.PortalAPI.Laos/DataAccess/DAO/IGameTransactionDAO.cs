using System;
using System.Collections.Generic;
using ServerCore.DataAccess.DAOImpl;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;

namespace ServerCore.DataAccess.DAO
{
    //Cac SP tu CoreDB. refTrans: transactionId tu sinh tu GameDB 
    public interface IGameTransactionDAO
    {
        long TranferVcoinToStar(long accountID, string username, int vcoin);

        long TopupStarFromVcoin(long accountID, string username, int vcoin, string ip, out int vcoinBalance, out long starBalance);

        List<long> TranferStarToVcoin(long accountID, string username, string ipAddress);

        long TranferStarToCoin(long accountID, string ipAddress, long bon, out long totalBac, out long bac, out long totalBon);

        List<TransactionCoreResponse> GetTransactionLogs(long accountID, GameMoneyType moneyType = GameMoneyType.BON);

        int TransferBon(long accountIdTrans, string nickNameTrans, int transferValue, long accountIdRecv, string nickNameRecv, int transferType, string ip, int sourceId, string reason, int isAgencyWeb, out long accountTransBalance, out long accountReceiveBalance, out long receiveAmount);

        double GetTransferRate(int type, int levelId);
        double GetTransferRateNew(string nickNameTran , string nickNameRev);

        List<TopupXuHistory> GetTopupXuHistory(int accountId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="type">0: lay tat, 1= mua bon, 2 = ban bon</param>
        /// <returns></returns>
        List<TransferBonHistory> GetTransferBonHistory(int accountId, int type = 0);
    }
}