using Microsoft.Extensions.Options;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess.DTO;
using ServerCore.Utilities.Database;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ServerCore.PortalAPI.Models;
using NetCore.Utils.Interfaces;
using PortalAPI.Models;

namespace ServerCore.DataAccess.DAOImpl
{
    public class EventDAOImpl : IEventDAO
    {
        private readonly AppSettings appSettings;
        private readonly IDBHelper _dbHelper;

        public EventDAOImpl(IDBHelper dbHepler, IOptions<AppSettings> options)
        {
            _dbHelper = dbHepler;
            appSettings = options.Value;
        }

        public int CheckGiftCode(long accountId, string accountName, string nickName, string giftcode, int merchantId, string merchantKey, int sourceId, string ip, out long balance)
        {
            //  PROCEDURE[dbo].[SP_GiftCodeData_Charging]
            //  @_AccountId BIGINT,
            //  @_AccountName NVARCHAR(50),
            //  @_NickName NVARCHAR(100),
            //  @_Code NVARCHAR(50),
            //  @_MerchantId INT = 0,
            //  @_MerchantKey NVARCHAR(50),
            //  @_SourceID INT, --1: android, 2 WP, 3 IOS, 4 WEB
            //  @_ClientIP NVARCHAR(30),
            //  @_Balance BIGINT OUTPUT,
            //  @_ResponseStatus BIGINT OUTPUT

            //DECLARE @_ERROR_NOT_EXIST_MERCHANTID INT = -90
            //DECLARE @_ERROR_GIFCODE_USING INT = -100
            //DECLARE @_ERROR_GIFCODE_NOT_EXIST INT = -101
            //DECLARE @_ERROR_GIFCODE_NOT_TIME INT = -102
            //DECLARE @_ERROR_GIFCODE_EXPIRE INT = -103
            //DECLARE @_ERROR_GIFCODE_LOCK INT = -104

            balance = -1;

            try
            {
                int responseStatus = 0;
                var pars = new SqlParameter[10];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_NickName", nickName);
                pars[3] = new SqlParameter("@_Code", giftcode);
                pars[4] = new SqlParameter("@_MerchantId", merchantId);
                pars[5] = new SqlParameter("@_MerchantKey", merchantKey);
                pars[6] = new SqlParameter("@_SourceID", sourceId);
                pars[7] = new SqlParameter("@_ClientIP", ip);
                pars[8] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                _dbHelper.SetConnectionString(appSettings.BillingGifcodeAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_GiftCodeData_Charging", pars);

                responseStatus = Convert.ToInt32(pars[9].Value);
                if (responseStatus >= 0)
                    balance = Convert.ToInt64(pars[8].Value);
                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return -99;
        }

        public List<EventX2> GetEvent(long accountId)
        {
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@AccountID", accountId);
                _dbHelper.SetConnectionString(appSettings.EventAPIConnectionString);
                List<EventX2> listAgency = _dbHelper.GetListSP<EventX2>("SP_GetEventInfoByAccountID", pars);

                return listAgency;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return null;
        }

    }
}