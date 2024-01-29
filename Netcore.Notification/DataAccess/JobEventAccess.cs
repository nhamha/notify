using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netcore.Notification.Models;
using NetCore.Utils.Interfaces;
using NetCore.Utils.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Netcore.Notification.DataAccess
{
    public class JobEventAccess
    {
        private readonly AppSettings _settings;
        private readonly IDBHelper _dbHelper;
        private readonly ILogger<SQLAccess> _logger;
        private string connectionString = string.Empty;
        private string eventConnectionString = string.Empty;

        public JobEventAccess(IOptions<AppSettings> options, IDBHelper dbHepler, ILogger<SQLAccess> logger)
        {
            _settings = options.Value;
            _dbHelper = dbHepler;
            _logger = logger;
            connectionString = _settings.SQLConnectionString;
            eventConnectionString = _settings.EventConnectionString;
        }

        #region Bốc thăm

        public int EventVQMMGet(long accountID)
        {
            try
            {
                SqlParameter[] pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_Remain", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_VQMM_Get", pars);
                return Convert.ToInt32(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return -99;
            }
        }

        public VQMMSpin EventVQMMSpin(long accountID, string accountName)
        {
            try
            {
                SqlParameter[] pars = new SqlParameter[12];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_PrizeID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_PrizeValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_PrizeName", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_Remain", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_GameName", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_FreeSpins", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_RoomID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_GameId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_VQMM_Spin", pars);
                var responseStatus = Convert.ToInt32(pars[7].Value);
                if (responseStatus >= 0)
                    return new VQMMSpin
                    {
                        PrizeID = Convert.ToInt32(pars[2].Value),
                        PrizeValue = Convert.ToInt32(pars[3].Value),
                        PrizeName = pars[4].Value.ToString(),
                        Balance = Convert.ToInt64(pars[6].Value),
                        ResponseCode = responseStatus,
                        Description = "Quay thành công",
                        GameName = pars[8].Value.ToString(),
                        Remain = Convert.ToInt32(pars[5].Value),
                        FreeSpins = Convert.ToInt32(pars[9].Value),
                        GameId = Convert.ToInt32(pars[10].Value),
                        RoomId = Convert.ToInt32(pars[11].Value)
                    };
                switch (responseStatus)
                {
                    case -98:
                        return new VQMMSpin
                        {
                            ResponseCode = responseStatus,
                            Description = "Hết lượt quay"
                        };

                    case -99:
                        return new VQMMSpin
                        {
                            ResponseCode = responseStatus,
                            Description = "Lỗi hệ thống"
                        };
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return new VQMMSpin
            {
                ResponseCode = -99,
                Description = "Lỗi hệ thống"
            };
        }

        public List<VQMMSpin> VQMMGetAllPrize()
        {
            try
            {
                return _dbHelper.GetListSP<VQMMSpin>(eventConnectionString, "SP_VQMM_GetAllPrize");
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<VQMMSpin>();
            }
        }
        public List<VQMMSpin> VQMMGetUserPrize()
        {
            try
            {
                return _dbHelper.GetListSP<VQMMSpin>(eventConnectionString, "SP_VQMM_GetUserPrize");
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<VQMMSpin>();
            }
        }
        public long GetVQMMFund()
        {
            try
            {
                SqlParameter[] pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_Fund", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_VQMM_GetFund", pars);
                return Convert.ToInt64(pars[0].Value);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return -99;
            }
        }

        #endregion Bốc thăm

        #region Nạp tiền

        public List<Deposit> DepositGetPrizeByAccount(long accountId)
        {
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                var lst = _dbHelper.GetListSP<Deposit>(eventConnectionString, "SP_Deposit_GetPrizeByAccount", pars);
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<Deposit>();
            }
        }

        public (int, long) GetMoney(long accountID, string userName, long AccountPrizeId, string clientIP)
        {
            try
            {
                long balance = 0;
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountId", accountID);
                pars[1] = new SqlParameter("@_Username", userName);
                pars[2] = new SqlParameter("@_AccountPrizeId", AccountPrizeId);
                pars[3] = new SqlParameter("@_ClientIP", clientIP);
                pars[4] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ResponseStatus_Event", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_Deposit_GetMoney", pars);
                var res = Convert.ToInt32(pars[5].Value);
                balance = Convert.ToInt64(pars[4].Value);
                return (res, balance);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return (-99, -1);
            }
        }

        #endregion Nạp tiền

        #region Nhiệm vụ
        public List<Quest> QuestGetPrizeByAccount(long accountId)
        {
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                var lst = _dbHelper.GetListSP<Quest>(eventConnectionString, "SP_Quest_GetPrizeByAccount", pars);
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<Quest>();
            }
        }
        public List<ExchangeRate> QuestGetListExchangeRate()
        {
            try
            {
                var lst = _dbHelper.GetListSP<ExchangeRate>(eventConnectionString, "SP_Quest_GetListExchangeRate");
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<ExchangeRate>();
            }
        }

        public List<Quest> QuestGetList(long accountId, string nickName)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_AccountName", nickName);
                var lst = _dbHelper.GetListSP<Quest>(eventConnectionString, "SP_UserQuest_GetList", pars);
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<Quest>();
            }
        }

        public int QuestGetAward(long questId, long accountId)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_UserQuestId", questId);
                pars[1] = new SqlParameter("@_AccountId", accountId);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_UserQuest_GetPoint", pars);
                var point = Convert.ToInt32(pars[2].Value);
                return point;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return -99;
            }
        }
        public int QuestGetPointBalance(long accountId)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_UserQuest_GetPointBalance", pars);
                var point = Convert.ToInt32(pars[1].Value);
                return point;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return -99;
            }
        }
        public int QuestAttendance(long accountId, string nickName)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_AccountName", nickName);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_Quest_Attendance", pars);
                var point = Convert.ToInt32(pars[2].Value);
                return point;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return -99;
            }
        }

        public (int, long) QuestExchangePoint(long accountId, int point)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_Point", point);
                pars[2] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_UserQuest_ExchangePoint", pars);
                var response = Convert.ToInt32(pars[3].Value);
                if (response >= 0)
                {
                    var balance = Convert.ToInt64(pars[2].Value);
                    return (response, balance);
                }
                return (response, -1);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return (-99, -1);
            }
        }

        public (int, long) QuestGetMoney(long accountID, string userName, long AccountPrizeId, string clientIP)
        {
            try
            {
                long balance = 0;
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountId", accountID);
                pars[1] = new SqlParameter("@_Username", userName);
                pars[2] = new SqlParameter("@_AccountPrizeId", AccountPrizeId);
                pars[3] = new SqlParameter("@_ClientIP", clientIP);
                pars[4] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ResponseStatus_Event", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_Quest_GetMoney", pars);
                var res = Convert.ToInt32(pars[5].Value);
                balance = Convert.ToInt64(pars[4].Value);
                return (res, balance);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return (-99, -1);
            }
        }
        #endregion
    }
}