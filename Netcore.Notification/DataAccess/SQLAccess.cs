using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netcore.Notification.Models;
using NetCore.Utils.Interfaces;
using NetCore.Utils.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Netcore.Notification.DataAccess
{
    public class SQLAccess
    {
        private readonly AppSettings _settings;
        private readonly IDBHelper _dbHelper;
        private readonly ILogger<SQLAccess> _logger;
        private string connectionString = string.Empty;
        private string eventConnectionString = string.Empty;
        public SQLAccess(IOptions<AppSettings> options, IDBHelper dbHepler, ILogger<SQLAccess> logger)
        {
            _settings = options.Value;
            _dbHelper = dbHepler;
            _logger = logger;
            connectionString = _settings.SQLConnectionString;
            eventConnectionString = _settings.EventConnectionString;
        }

        public List<SystemNotification> GetSystemNotification(int gameID)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_GameID", gameID);
                var res = _dbHelper.GetListSP<SystemNotification>("SP_Notifications_GetSystemNotify", pars);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<SystemNotification>();
            }
        }
        public List<SystemNotification> GetTopJackpot(int gameID)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                return _dbHelper.GetListSP<SystemNotification>("SP_Notifications_GetTopJackpot", new SqlParameter("@_GameID", gameID));
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<SystemNotification>();
            }
        }
        public List<LobbyText> GetLobbyText()
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var res = _dbHelper.GetListSP<LobbyText>("SP_LobbyText_Get");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<LobbyText>();
            }
        }

        public List<UserNotification> GetUserMail(long accountID, string nickName)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountId", accountID);
                pars[1] = new SqlParameter("@_NickName", nickName);
                var res = _dbHelper.GetListSP<UserNotification>("SP_UserNotify_Get", pars);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<UserNotification>();
            }
        }

        public List<UserNotification> GetUserPopup(long accountID, string nickName)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountId", accountID);
                pars[1] = new SqlParameter("@_NickName", nickName);
                var res = _dbHelper.GetListSP<UserNotification>("SP_UserNotify_Popup_Get", pars);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<UserNotification>();
            }
        }

        public int GetUnReadUserNotifyQuantity(long accountID, string nickName)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_Quantity", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_NickName", nickName);
                _dbHelper.ExecuteNonQuerySP("SP_UserNotify_GetUnReadQuantity", pars);
                return Convert.ToInt32(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return 0;
            }
        }

        public string GetUserNotifyContent(long notifyID, long accountID, string nickName)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_ID", notifyID);
                pars[1] = new SqlParameter("@_AccountID", accountID);
                pars[2] = new SqlParameter("@_Content", SqlDbType.NVarChar, 1000) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_NickName", nickName);
                var res = _dbHelper.ExecuteNonQuerySP("SP_UserNotify_GetContentByID", pars);
                return pars[2].Value.ToString();
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return string.Empty;
            }
        }

        public int DeleteUserNotify(long notifyID, long accountId, string nickName)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_ID", notifyID);
                pars[1] = new SqlParameter("@_AccountID", accountId);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_NickName", nickName);
                var res = _dbHelper.ExecuteNonQuerySP("SP_UserNotify_DeleteByID", pars);
                return Convert.ToInt32(pars[2].Value);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return -99;
            }
        }

        public List<UserNotification> UpdatePopupNotifyStatusByID(long notifyID, long accountID, int status)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_NotifyID", notifyID);
                pars[1] = new SqlParameter("@_AccountID", accountID);
                pars[2] = new SqlParameter("@_Status", status);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                var res = _dbHelper.GetListSP<UserNotification>("SP_Notifications_UpdatePopupNotifyStatusByID");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<UserNotification>();
            }
        }

        public bool CreateUserNotify(int accountId, string userName, int type, string title, string content)
        {
            try
            {
                _dbHelper.SetConnectionString(connectionString);
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_UserName", userName);
                pars[2] = new SqlParameter("@_GameID", 0);
                pars[3] = new SqlParameter("@_Type", type);
                pars[4] = new SqlParameter("@_Title", title);
                pars[5] = new SqlParameter("@_Content", content);
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP("SP_Notifications_CreateUserNotify", pars);
                return Convert.ToBoolean(pars[6].Value);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return false;
            }
        }

        #region Event Football

        public Football FootballGetAccountInfo(long accountId)
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                var lst = _dbHelper.GetListSP<Football>("SP_Football_GetInfo", pars);
                return lst.FirstOrDefault();
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new Football();
            }
        }
        public List<Football> FootballGetTop()
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var res = _dbHelper.GetListSP<Football>("SP_Football_GetTop");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<Football>();
            }
        }

        public List<Football> FootballGetTime(int accountId)
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                var res = _dbHelper.GetListSP<Football>("SP_Football_GetInfo", pars);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<Football>();
            }
        }
        public List<FootballGiftPrize> FootballGetPrizeByAccount(long accountId)
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                var lst = _dbHelper.GetListSP<FootballGiftPrize>("SP_Football_GetPrizeByAccount", pars);
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<FootballGiftPrize>();
            }
        }
        public int GetMoney(long accountID, string userName, long AccountPrizeId, int type, string ClientIP, out long Balance)
        {
            //var _dbHelper = new DBHelper(_appSettings.LuckyDiceConnectionString);
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_AccountId", accountID);
                pars[1] = new SqlParameter("@_Username", userName);
                pars[2] = new SqlParameter("@_AccountPrizeId", AccountPrizeId);
                pars[3] = new SqlParameter("@_ClientIP", ClientIP);
                pars[4] = new SqlParameter("@_Type", type);
                pars[5] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP("SP_Football_GetMoney", pars);
                var res = Convert.ToInt32(pars[6].Value);
                Balance = Convert.ToInt64(pars[5].Value);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                Balance = 0;
                return -99;
            }
        }
        #region Gift
        public List<Football> FootballGetAllGiftHistory()
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var res = _dbHelper.GetListSP<Football>("SP_Football_GetAllGiftHistory");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<Football>();
            }
        }
        public List<FootballGiftPrize> FootballGetAllGiftPrize()
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var res = _dbHelper.GetListSP<FootballGiftPrize>("SP_Football_GetListGift");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<FootballGiftPrize>();
            }
        }
        public long FootballGetGift(long accountId, string accountName, int prizeId, ref long balance)
        {
            try
            {
                balance = -1;
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_PrizeId", prizeId);
                pars[3] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP("SP_Football_GetGift", pars);
                var response = Convert.ToInt32(pars[4].Value);
                balance = Convert.ToInt64(pars[3].Value);
                return response;

            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return -99;
            }
        }
        #endregion
        #region Player
        public long FootbalBetPlayer(long accountId, string accountName, int prizeId, int betValue, ref long balance)
        {
            try
            {
                balance = -1;
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_PrizeId", prizeId);
                pars[3] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_BetValue", betValue);
                _dbHelper.ExecuteNonQuerySP("SP_Football_Bet", pars);
                var response = Convert.ToInt32(pars[4].Value);
                balance = Convert.ToInt64(pars[3].Value);
                return response;

            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return -99;
            }
        }
        public List<FootballPlayer> FootballGetListPlayer()
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var res = _dbHelper.GetListSP<FootballPlayer>("SP_Football_GetListPlayer");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<FootballPlayer>();
            }
        }
        public List<FootballBetAccount> FootballBetOfAccount(long accountId)
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                var lst = _dbHelper.GetListSP<FootballBetAccount>("SP_Football_GetBetPlayerOfAccount", pars);
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<FootballBetAccount>();
            }
        }
        public List<FootballTime> FootballGetTime()
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var lst = _dbHelper.GetListSP<FootballTime>("SP_Football_GetTime");
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<FootballTime>();
            }
        }

        #endregion
        #endregion

        #region Vip99
        public List<LoyaltyAccount> LoyaltyGetTop()
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var res = _dbHelper.GetListSP<LoyaltyAccount>("SP_Loyalty_GetTop");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<LoyaltyAccount>();
            }
        }


        public List<LoyaltyAccount> LoyaltyGetPrizeByAccount(long accountId)
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                var lst = _dbHelper.GetListSP<LoyaltyAccount>("SP_Loyalty_GetPrizeByAccount", pars);
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<LoyaltyAccount>();
            }
        }
        public LoyaltyAccount LoyaltyGetAccountInfo(long accountId)
        {
            try
            {
                _dbHelper.SetConnectionString(eventConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                var lst = _dbHelper.GetInstanceSP<LoyaltyAccount>("SP_Loyalty_GetAccountInfo", pars);
                return lst;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new LoyaltyAccount();
            }
        }
        public int LoyaltyGetMoney(long accountID, string userName, long prizeId, string clientIP, out long balance)
        {
            try
            {
                NLogManager.LogMessage(string.Format("GetStar =>> accountID:{0} | userName: {1}, AccountPrizeId: {2}, ClientIP:{3} ", accountID, userName, prizeId, clientIP));
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountId", accountID);
                pars[1] = new SqlParameter("@_Username", userName);
                pars[2] = new SqlParameter("@_AccountPrizeId", prizeId);
                pars[3] = new SqlParameter("@_ClientIP", clientIP);
                pars[4] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ResponseStatus_Event", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_Loyalty_GetMoney", pars);
                var res = Convert.ToInt32(pars[5].Value);
                balance = Convert.ToInt64(pars[4].Value);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.LogError(ex.Message);
                balance = 0;
                return -99;
            }
        }

        #endregion

        #region Tán lộc
        public (int, long) ShareProfit(long accountId, string username, long prizeValue, int sourceId,
         int merchantId)
        {
            long balance = -1;
            var response = -1;
            try
            {
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", username);
                pars[2] = new SqlParameter("@_TotalPrizeValue", prizeValue);
                pars[3] = new SqlParameter("@_SourceID", sourceId);
                pars[4] = new SqlParameter("@_MerchantID", merchantId);
                pars[5] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_ShareProfit_CreatePackage", pars);
                int.TryParse(pars[6].Value.ToString(), out response);
                if (response >= 0)
                {
                    long.TryParse(pars[5].Value.ToString(), out balance);
                }
                NLogManager.LogInfo(string.Format("ShareProfit: AccountId={0}|BetValue={1} : response={2}", accountId, prizeValue, response));
                return (response, balance);
            }
            catch (Exception exception)
            {
                NLogManager.LogException(exception);
                return (-99, -1);
            }
        }

        public (int, long) GetProfit(long accountId, string username, long totalCashFlow, int sourceId,
          int merchantId)
        {
            long balance = -1;
            var response = -1;
            try
            {
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", username);
                pars[2] = new SqlParameter("@_TotalCashFlow", totalCashFlow);
                pars[3] = new SqlParameter("@_SourceID", sourceId);
                pars[4] = new SqlParameter("@_MerchantID", merchantId);
                pars[5] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.ExecuteNonQuerySP(eventConnectionString, "SP_ShareProfit_GetPrize", pars);
                int.TryParse(pars[6].Value.ToString(), out response);
                if (response >= 0)
                {
                    long.TryParse(pars[5].Value.ToString(), out balance);
                }
                NLogManager.LogInfo(string.Format("GetProfit: AccountId={0}|BetValue={1} : response={2}", accountId, totalCashFlow, response));
                return (response, balance);
            }
            catch (Exception exception)
            {
                NLogManager.LogException(exception);
                return (-99, -1);
            }
        }
        public List<ShareProfit> GetListShareProfit()
        {
            try
            {
                return _dbHelper.GetListSP<ShareProfit>(eventConnectionString, "SP_ShareProfit_GetList");
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return new List<ShareProfit>();
            }
        }
        #endregion
    }
}