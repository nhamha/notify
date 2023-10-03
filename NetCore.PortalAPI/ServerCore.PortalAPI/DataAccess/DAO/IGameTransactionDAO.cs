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
        int TransferBon(long accountIdTrans, string nickNameTrans, int transferValue, long accountIdRecv, string nickNameRecv, int transferType, string ip, int sourceId, string reason, int isAgencyWeb, string deviceid, out long accountTransBalance, out long accountReceiveBalance, out long receiveAmount);
        List<TransferBonHistory> GetTransferBonHistory(int accountId, int type = 0);
        int TopupByIAP(long accountID, string userName, int amount, string description, string clientIP, int sourceID, out long balance, out int status);
    }
}