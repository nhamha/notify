using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess.DTO;
using ServerCore.PortalAPI.Models;
using ServerCore.Utilities.Database;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;

namespace ServerCore.DataAccess.DAOImpl
{
    public class GameTransactionDAOImpl : IGameTransactionDAO
    {
        private readonly AppSettings appSettings;
        public GameTransactionDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
         
        }
        /// <summary>
        /// Nạp sao từ vcoin (Bản chất là chuyển vcoin thành sao để chơi các minigame)
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="username"></param>
        /// <param name="vcoin"></param>
        /// <returns></returns>
        public long TopupStarFromVcoin(long accountID, string username, int vcoin, string ip, out int vcoinBalance, out long starBalance)
        {
            int responseStatus = -1;
            try
            {
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_Username", username);
                pars[2] = new SqlParameter("@_Vcoin", vcoin);
                pars[3] = new SqlParameter("@_ClientIP", ip);
                pars[4] = new SqlParameter("@_VcoinBalance", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_StarBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                
                SQLAccess.getAgency().ExecuteNonQuerySP("SP_GameCard_TopupStarFromVcoin", pars);

                int returnValue;

                responseStatus = int.TryParse(pars[6].Value.ToString(), out returnValue) ? returnValue : 0;

                if(responseStatus < 0)
                {
                    vcoinBalance = -1;
                    starBalance = -1;
                }
                else
                {
                    starBalance = (long)pars[5].Value;
                    vcoinBalance = (int)pars[4].Value;
                }
            }
            catch(Exception ex)
            {
                starBalance = -1;
                vcoinBalance = -1;
                NLogManager.Exception(ex);
            }
           
            //starBalance = -1;
            //vcoinBalance = - 1;
            return responseStatus;
        }


        public long TranferVcoinToStar(long accountID, string username, int vcoin)
        {
        
            int responseStatus = -1;
            long star = -1;
            try
            {
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_ServiceID", 100000);
                pars[1] = new SqlParameter("@_AccountID", accountID);
                pars[2] = new SqlParameter("@_AccountName", username);
                pars[3] = new SqlParameter("@_AccessToken", "");
                pars[4] = new SqlParameter("@_Vcoin", vcoin);
                pars[5] = new SqlParameter("@_Star", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };


                SQLAccess.getAgency().ExecuteNonQuerySP("SP_GameCard_TransferVcoinToStar", pars);

                responseStatus = (int)pars[6].Value;
                if(responseStatus < 0)
                {
                    star = -1;
                }
                else
                {
                    star = (long)pars[5].Value;
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return star;
        }

        /// <summary>
        /// Đổi sao sang vcoin
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="username"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public List<long> TranferStarToVcoin(long accountID, string username, string ipAddress)
        {
 
            long responseStatus = -1;
            long amount = 0;
            long vcoin = 0;
            List<long> retValues = new List<long>();

            try
            {
                var pars = new SqlParameter[6];
                //pars[0] = new SqlParameter("@_ServiceID", 100000);
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_AccountName", username);
                //pars[3] = new SqlParameter("@_AccessToken", "");
                pars[2] = new SqlParameter("@_ClientIP", ipAddress);
                pars[3] = new SqlParameter("@_Amount", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_Vcoin", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };


                SQLAccess.getAgency().ExecuteNonQuerySP("SP_GameCard_TransferStarToVcoin", pars);

                responseStatus = (long)pars[5].Value;
                if(responseStatus >= 0)
                {
                    amount = (long)pars[3].Value;
                    vcoin = (int)pars[4].Value;
                    retValues.Add(amount);
                    retValues.Add(vcoin);
                    return retValues;
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return retValues;
        }

        /// <summary>
        /// Nạp đá từ sao
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="ipAddress"></param>
        /// <param name="star"></param>
        /// <returns></returns>
        public long TranferStarToCoin(long accountID, string ipAddress, long bon, out long totalBac, out long bac, out long totalBon)
        {
          
            int responseStatus = -1;
            bac = -1;
            totalBac = -1;
            totalBon = -1;

            try
            {
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_AccountID", (int)accountID);
                pars[1] = new SqlParameter("@_ClientIP", ipAddress);
                pars[2] = new SqlParameter("@_Star", bon);
                pars[3] = new SqlParameter("@_Coin", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_Gift", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_TotalCoin", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                SQLAccess.getAgency().ExecuteNonQuerySP("SP_Accounts_TopupCoinFromStar", pars);

                responseStatus = (int)pars[6].Value;
                if(responseStatus >= 0)
                {
                    bac = (long)pars[3].Value;
                    totalBac = (long)pars[5].Value;
                    totalBon = responseStatus;
                }
                NLogManager.Info(string.Format("AccountID = {0}, TotalCoin = {1}, Xu = {2}, ResponseStatus = {3}", accountID, bon, bac, responseStatus));
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return responseStatus;
        }

        public List<TransactionCoreResponse> GetTransactionLogs(long accountID, GameMoneyType moneyType = GameMoneyType.BON)
        {
           
            List<TransactionCoreResponse> list = null;

            try
            {
             
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_TopCount", 50);

                if(moneyType == GameMoneyType.BAC)
                {
                    list = SQLAccess.getAuthen().GetListSP<TransactionCoreResponse>("SP_GetHistoryTransactionsXu", pars);
                }
                else if(moneyType == GameMoneyType.BON)
                {
                    list = SQLAccess.getAuthen().GetListSP<TransactionCoreResponse>("SP_GetHistoryTransactionsCoin", pars);
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return list;
        }

        public int TransferBon(long accountIdTrans, string nickNameTrans, int transferValue, long accountIdRecv, string nickNameRecv, int transferType, string ip, int sourceId, string reason, int isAgencyWeb, out long accountTransBalance, out long accountReceiveBalance, out long receiveAmount)
        {
            //            ALTER PROCEDURE[dbo].[SP_Tranfer]
            //              @_AccountID BIGINT
            //              ,@_NickName             NVARCHAR(100)
            //	            ,@_TranferValue BIGINT = 0
            //              ,@_ReceiverAccountID    BIGINT
            //	            ,@_ReceiverNickName NVARCHAR(30) = ''
            //	            ,@_ServiceId INT = 0
            //              ,@_ServiceKey           VARCHAR(35) = ''		
            //	            ,@_Description NVARCHAR(150) = 'Chuyển tiền User-User'
            //	            ,@_ClientIP VARCHAR(15) = ''
            //	            ,@_Type INT
            //              ,@_SourceID             INT
            //	            ,@_Balance BIGINT = 0  OUTPUT
            //	            ,@_ResponseStatus INT OUTPUT

            int response = -1;
            accountTransBalance = -1;
            accountReceiveBalance = -1;
            receiveAmount = 0;
            try
            {
                var pars = new SqlParameter[16];
                pars[0] = new SqlParameter("@_AccountID", accountIdTrans);
                pars[1] = new SqlParameter("@_NickName", nickNameTrans);
                pars[2] = new SqlParameter("@_TranferValue", transferValue);
                pars[3] = new SqlParameter("@_ReceiverAccountID", accountIdRecv);
                pars[4] = new SqlParameter("@_ReceiverNickName", nickNameRecv);
                pars[5] = new SqlParameter("@_ServiceId", 0);
                pars[6] = new SqlParameter("@_ServiceKey", "");
                pars[7] = new SqlParameter("@_Description", reason);
                pars[8] = new SqlParameter("@_ClientIP", ip);
                pars[9] = new SqlParameter("@_Type", transferType);
                pars[10] = new SqlParameter("@_SourceID", sourceId);
                pars[11] = new SqlParameter("@_IsAgency", isAgencyWeb);
                pars[12] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[13] = new SqlParameter("@_ReceiverBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[14] = new SqlParameter("@_ReceiveAmount", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[15] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                SQLAccess.getBilling().ExecuteNonQuerySP("SP_Tranfer", pars);

                response = (int)pars[15].Value;
                if (response >= 0)
                {
                    accountTransBalance = (long)pars[12].Value;
                    accountReceiveBalance = (long)pars[13].Value;
                    receiveAmount = (long)pars[14].Value;
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return response;
        }

        public double GetTransferRate(int type, int levelId)
        {
            double rate = 0;
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_Type", type);
                pars[1] = new SqlParameter("@_LevelID", levelId);
                pars[2] = new SqlParameter("@_Rate", SqlDbType.Float) { Direction = ParameterDirection.Output };
                SQLAccess.getAgency().ExecuteNonQuerySP("SP_Agency_GetRate", pars);
                rate = (double) pars[2].Value;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return rate;
        }

        public double GetTransferRateNew(string nickNameTran, string nickNameRev)
        {
            double rate = 0;
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_NickNameTran", nickNameTran);
                pars[1] = new SqlParameter("@_NickNameRecv", nickNameRev);
                pars[2] = new SqlParameter("@_Rate", SqlDbType.Float) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_ErrorCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SQLAccess.getAgency().ExecuteNonQuerySP("SP_Agency_GetRate_New", pars);
                int errorCode = Int32.Parse(pars[3].Value.ToString());
                if (errorCode >= 0)
                    rate = (double)pars[2].Value;
            }
            catch (Exception ex)
            {
                //NLogManager.Exception(ex);

            }
         
            return rate;
        }

        public List<TopupXuHistory> GetTopupXuHistory(int accountId)
        {
            //ALTER PROCEDURE[dbo].[SP_GetBitToBacLog]
            //@_AccountID BIGINT
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);

                var list = SQLAccess.getAuthen().GetListSP<TopupXuHistory>("SP_GetBitToBacLog", pars);
                if(list != null)
                    return list;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return new List<TopupXuHistory>();
        }

        List<TransferBonHistory> IGameTransactionDAO.GetTransferBonHistory(int accountId, int type)
        {
            // SP_GetTranferBon
            //@_AccountID BIGINT
            //@_Type INT 0 = All, 1: Bán TotalCoin, 2: Mua TotalCoin
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Type", type);

                var list = SQLAccess.getBilling().GetListSP<TransferBonHistory>("SP_GetTranferBon", pars);
                if (list != null)
                    return list;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
         
            return new List<TransferBonHistory>();
        }
    }
}