using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using NetCore.Utils.Interfaces;
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
        private readonly IDBHelper _dbHelper;
        public GameTransactionDAOImpl(IDBHelper dbHelper, IOptions<AppSettings> options)
        {
            this.appSettings = options.Value;
            this._dbHelper = dbHelper;
        }

        public int TransferBon(long accountIdTrans, string nickNameTrans, int transferValue, long accountIdRecv, string nickNameRecv, int transferType, string ip, int sourceId, string reason, int isAgencyWeb, string deviceid, out long accountTransBalance, out long accountReceiveBalance, out long receiveAmount)
        {
            int response = -1;
            accountTransBalance = -1;
            accountReceiveBalance = -1;
            receiveAmount = 0;
            try
            {
                var pars = new SqlParameter[17];
                pars[0] = new SqlParameter("@_AccountID", accountIdTrans);
                pars[1] = new SqlParameter("@_NickName", nickNameTrans);
                pars[2] = new SqlParameter("@_TranferValue", transferValue);
                pars[3] = new SqlParameter("@_ReceiverAccountID", accountIdRecv);
                pars[4] = new SqlParameter("@_ReceiverNickName", nickNameRecv);
                pars[5] = new SqlParameter("@_ServiceId", 0);
                pars[6] = new SqlParameter("@_ServiceKey", deviceid);
                pars[7] = new SqlParameter("@_Description", reason);
                pars[8] = new SqlParameter("@_ClientIP", ip);
                pars[9] = new SqlParameter("@_DeviceID", deviceid);
                pars[10] = new SqlParameter("@_Type", 0);
                pars[11] = new SqlParameter("@_SourceID", sourceId);
                pars[12] = new SqlParameter("@_IsAgency", isAgencyWeb);
                pars[13] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[14] = new SqlParameter("@_ReceiverBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[15] = new SqlParameter("@_ReceiveAmount", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[16] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };



                _dbHelper.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_Tranfer", pars);

                response = (int)pars[16].Value;
                if (response >= 0)
                {
                    accountTransBalance = (long)pars[13].Value;
                    accountReceiveBalance = (long)pars[14].Value;
                    receiveAmount = (long)pars[15].Value;
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return response;
        }

     
        List<TransferBonHistory> IGameTransactionDAO.GetTransferBonHistory(int accountId, int type)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Type", type);
                _dbHelper.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                var list = _dbHelper.GetListSP<TransferBonHistory>("SP_GetTranferBon", pars);
                if (list != null)
                {
                    foreach(TransferBonHistory itemHis in list)
                    {
                        if (itemHis.AccountID == accountId)
                            itemHis.TypePurchase = 0;
                        if (itemHis.ReceiverAccountID == accountId)
                            itemHis.TypePurchase = 1;
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
         
            return new List<TransferBonHistory>();
        }

        public int TopupByIAP(long accountID, string userName, int amount, string description, string clientIP, int sourceID, out long balance, out int status)
        {
            //  ALTER PROCEDURE[dbo].[SP_Topup_IAP]
            //  @_AccountID BIGINT
            // ,@_UserName VARCHAR(100)
            // ,@_Amount INT
            // ,@_Description NVARCHAR(200)
            // ,@_ClientIP VARCHAR(50)
            // ,@_SourceID INT
            // ,@_TotalGameBalance BIGINT OUTPUT
            // ,@_ResponseStatus INT OUTPUT
            //----WITH ENCRYPTION
            //AS
            int response = -1;
            balance = -1;
            status = -1;
            try
            {
                var pars = new SqlParameter[8];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_UserName", userName);
                pars[2] = new SqlParameter("@_Amount", amount);
                pars[3] = new SqlParameter("@_Description", description);
                pars[4] = new SqlParameter("@_ClientIP", clientIP);
                pars[5] = new SqlParameter("@_SourceID", sourceID);
                pars[6] = new SqlParameter("@_TotalGameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHelper.SetConnectionString(appSettings.CardGameBettingAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_Topup_IAP", pars);

                response = (int)pars[7].Value;
                if (response >= 0)
                {
                    balance = (long)pars[6].Value;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return response;
        }
    }
}