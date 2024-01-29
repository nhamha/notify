using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Netcore.Notification.Controllers;
using Netcore.Notification.DataAccess;
using Netcore.Notification.Models;
using NetCore.Utils;
using NetCore.Utils.Cache;
using NetCore.Utils.Log;
using NetCore.Utils.Sessions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NetCore.Utils.Enums;

namespace Netcore.Notification.Hubs
{
    public class GateHub : Hub
    {
        private readonly ConnectionHandler _connection;
        private readonly PlayerHandler _playerHandler;
        private readonly AccountSession _accountSession;
        private readonly AppSettings _settings;
        private readonly NotificationHandler _notificationHandler;
        private readonly JackpotController _jackpotController;
        private readonly XJackpotController _xJackpotController;
        private readonly EventController _eventController;
        private readonly SQLAccess _sql;
        private readonly CacheHandler _cacheHandler;
        private readonly JobEventAccess _jobAccess;

        public GateHub(ConnectionHandler connection, IOptions<AppSettings> options,
            AccountSession accountSession, JackpotController jackpotController, XJackpotController xJackpotController,
            NotificationHandler notificationHandler, PlayerHandler playerHandler, JobEventAccess jobAccess,
            EventController eventController, SQLAccess sql, CacheHandler cacheHandler)
        {
            _playerHandler = playerHandler;
            _connection = connection;
            _accountSession = accountSession;
            _notificationHandler = notificationHandler;
            _settings = options.Value;
            _jackpotController = jackpotController;
            _xJackpotController = xJackpotController;
            _eventController = eventController;
            _cacheHandler = cacheHandler;
            _sql = sql;
            _jobAccess = jobAccess;
        }

        public async Task GetSystemNotification(int gameid)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            try
            {
                if (gameid < 0) throw new Exception("GameID không đúng");
                var notifications = _notificationHandler.GetSystemNotification(gameid);
                await Clients.Caller.SendAsync("NotifySystem", new { GameID = gameid, Data = notifications });
                await Clients.Caller.SendAsync("LobbyText", _notificationHandler.GetLobbyText());
                await Clients.Caller.SendAsync("popupLogin", _notificationHandler.GetUserPopup(accountId, username));
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
        }

        public int GetUserMail()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            try
            {
                if (accountId < 1 || string.IsNullOrEmpty(username))
                {
                    NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                    return -1001;
                }
                _connection.PlayerConnect(accountId, Context.ConnectionId);
                var player = _playerHandler.GetPlayer(accountId);
                if (player == null)
                {
                    player = _playerHandler.AddPlayer(accountId, username);
                }
                var notifications = _notificationHandler.GetUserMail(accountId, username);
                Clients.Caller.SendAsync("UserMail", notifications);
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return -99;
        }

        public int GetUnReadUserMailQuantity()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            try
            {
                if (accountId < 1 || string.IsNullOrEmpty(username))
                {
                    NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                    return -1001;
                }
                _connection.PlayerConnect(accountId, Context.ConnectionId);
                var player = _playerHandler.GetPlayer(accountId);
                if (player == null)
                {
                    player = _playerHandler.AddPlayer(accountId, username);
                }
                var notifications = _notificationHandler.GetUnReadUserNotifyQuantity(accountId, username);
                Clients.Caller.SendAsync("notifyNewMail", notifications);
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return -99;
        }

        public int GetUserMailContent(long id)
        {
            if (id < 1) return -99;
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            try
            {
                if (accountId < 1 || string.IsNullOrEmpty(username))
                {
                    NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                    return -1001;
                }
                _connection.PlayerConnect(accountId, Context.ConnectionId);
                var player = _playerHandler.GetPlayer(accountId);
                if (player == null)
                {
                    player = _playerHandler.AddPlayer(accountId, username);
                }
                var notifications = _notificationHandler.GetUserNotifyContent(id, accountId, username);
                Clients.Caller.SendAsync("mailContent", notifications);
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return -99;
        }

        public int DeleteUserMail(long id)
        {
            if (id < 0) throw new Exception();
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            try
            {
                if (accountId < 1 || string.IsNullOrEmpty(username))
                {
                    NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                    return -1001;
                }
                var notifications = _notificationHandler.DeleteUserNotify(id, accountId, username);
                Clients.Caller.SendAsync("deleteResult", notifications);
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return -99;
        }

        public int DeleteManyUserMail(string ids)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return -1001;
            }
            try
            {
                var idArray = ids.Split(",");
                if (idArray.Length <= 0) return -1;
                var lstId = new List<int>();
                for (int i = 0; i < idArray.Length; i++)
                {
                    var id = -1;
                    if (int.TryParse(idArray[i], out id) && id > 0)
                    {
                        _notificationHandler.DeleteUserNotify(id, accountId, username);
                    }
                }
                Clients.Caller.SendAsync("deleteResult", 1);
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return -99;
        }

        public int ReadPopupNotification()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            try
            {
                if (accountId < 1 || string.IsNullOrEmpty(username))
                {
                    NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                    return -1001;
                }
                _connection.PlayerConnect(accountId, Context.ConnectionId);
                var player = _playerHandler.GetPlayer(accountId);
                if (player == null)
                {
                    player = _playerHandler.AddPlayer(accountId, username);
                }
                var notifications = _notificationHandler.GetUserMail(accountId, username);
                Clients.Caller.SendAsync(" NotifyUser", notifications);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return -99;
        }

        public void GetEventInfo()
        {
            try
            {
                var res = _xJackpotController.GetListEventInfo();
                Clients.Caller.SendAsync("events", res);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
        }

        #region Event Football

        public async Task FootballGetGift(int prizeId)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                long balance = 0;
                var response = _eventController.FootballGetGift(accountId, username, prizeId, ref balance);
                await Clients.Caller.SendAsync("footballGetGiftResult", response, balance);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("footballGetGiftResult", Enums.ErrorCode.Exception);
            }
        }

        public async Task FootballGetAccountInfo()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            NLogManager.LogInfo(string.Format("{0} - {1}", accountId, username));

            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _eventController.FootballGetAccountInfo(accountId);
                if (response == null)
                {
                    response = new Football();
                }
                await Clients.Caller.SendAsync("footballAccountInfo", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task FootballGetPrizeByAccount()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _eventController.FootballGetPrizeByAccount(accountId);
                await Clients.Caller.SendAsync("footballGiftPrize", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task FootballGetTop()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _eventController.FootballGetTop();
                await Clients.Caller.SendAsync("footballTop", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task FootballGetAllGiftHistory()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _eventController.FootballGetAllGiftHistory();
                await Clients.Caller.SendAsync("footballAllGiftHistoryResult", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task FootballGetListGift()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _eventController.FootballGetAllGiftPrize();
                await Clients.Caller.SendAsync("footballListGift", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task FootballGetMoney(int prizeId, int type)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username) || type < 0 || type > 1 || prizeId <= 0)
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var ip = _accountSession.IpAddress;
                long balance = 0;
                var res = _eventController.GetMoney(_accountSession.AccountID, _accountSession.NickName, prizeId, type, ip, out balance);
                await Clients.Caller.SendAsync("FootballGetMoneyResult", res, balance);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        #region Player

        public async Task FootballGetListPlayer()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _sql.FootballGetListPlayer();
                await Clients.Caller.SendAsync("footballPlayer", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task FootballBetOfAccount()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _sql.FootballBetOfAccount(accountId);
                await Clients.Caller.SendAsync("resultOfAccount", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("resultOfAccount", Enums.ErrorCode.Exception);
            }
        }

        public async Task FootballGetTime()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _sql.FootballGetTime();
                await Clients.Caller.SendAsync("footballTime", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("footballTime", Enums.ErrorCode.Exception);
            }
        }

        public async Task FootballBet(int prizeId, int betValue)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            NLogManager.LogInfo("FootballBet: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress + "|prize: " + prizeId + "|betvalue: " + betValue);
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            if (betValue <= 0 || prizeId <= 0)
            {
                return;
            }
            try
            {
                long balance = 0;
                var response = _sql.FootbalBetPlayer(accountId, username, prizeId, betValue, ref balance);
                await Clients.Caller.SendAsync("resultBet", response, balance);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("resultBet", Enums.ErrorCode.Exception);
            }
        }

        #endregion Player

        #endregion Event Football

        #region Vip99

        public async Task LoyaltyGetPrizeByAccount()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _sql.LoyaltyGetPrizeByAccount(accountId);
                await Clients.Caller.SendAsync("loyaltyPrize", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task LoyaltyGetAccount()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _sql.LoyaltyGetAccountInfo(accountId);
                await Clients.Caller.SendAsync("loyaltyAccount", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task LoyaltyGetTop()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _sql.LoyaltyGetTop();
                await Clients.Caller.SendAsync("loyaltyTop", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task LoyaltyGetMoney(int prizeId)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username) || prizeId <= 0)
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var ip = _accountSession.IpAddress;
                long balance = 0;
                var res = _sql.LoyaltyGetMoney(_accountSession.AccountID, _accountSession.NickName, prizeId, ip, out balance);
                await Clients.Caller.SendAsync("LoyaltyGetMoneyResult", res, balance);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        #endregion Vip99

        #region Event tán lộc

        [HubMethodName("ShareProfit")]
        public void ShareProfit(int prizeValue)
        {
            try
            {
                var accountId = _accountSession.AccountID;
                if (accountId < 1 || string.IsNullOrEmpty(_accountSession.NickName) || _accountSession.IsAgency)
                {
                    Clients.Caller.SendAsync("shareProfitResult", Enums.ErrorCode.NotAuthen);
                    return;
                }
                //nếu số lượng quá lớn
                if (prizeValue > 100000000 || prizeValue < 1000)
                {
                    Clients.Caller.SendAsync("shareProfitResult", Enums.ErrorCode.ERR_BETTINGDATA_INVALID);
                    return;
                }
                var response = _sql.ShareProfit(accountId, _accountSession.NickName, prizeValue,
                    _accountSession.PlatformID,
                    _accountSession.MerchantID);
                //DECLARE @_PROCESS_SUCCESS INT = 0;
                //DECLARE @_Exception INT = -99
                //DECLARE @_ERR_BETTIME_EXPIRED INT = -207 -- Het thoi gian dat cuoc
                //DECLARE @_ERR_BETVALUE_INVALID INT = -212	-- Giá trị đặt cược không hợp lệ
                //DECLARE @_ERR_NOTBET_MULTILOCATION INT = -208 -- Dat 2 cua 1 phong
                //DECLARE @_ERR_BETTINGDATA_INVALID INT = -232; --Du lieu dat cuoc ko hop le
                //gửi về cho người tán lộc
                Clients.Caller.SendAsync("shareProfitResult", response.Item1, response.Item2);
                //gửi cho tất cả user
                if (response.Item1 >= 0)
                {
                    Clients.All.SendAsync("userShareProfit", _accountSession.NickName, prizeValue);
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                Clients.Caller.SendAsync("shareProfitResult", Enums.ErrorCode.Exception);
            }
        }

        [HubMethodName("GetListShareProfit")]
        public void GetListShareProfit()
        {
            try
            {
                var accountId = _accountSession.AccountID;
                if (accountId < 1 || string.IsNullOrEmpty(_accountSession.NickName))
                {
                    Clients.Caller.SendAsync("listShareProfit", Enums.ErrorCode.NotAuthen);
                    return;
                }
                var response = _eventController.GetListShareProfit();
                var response1 = _eventController.GetListVQMMUserPrize();
                Clients.Caller.SendAsync("listShareProfit", response, response1);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                Clients.Caller.SendAsync("listShareProfit", Enums.ErrorCode.Exception);
            }
        }
        [HubMethodName("GetProfit")]
        public async Task GetProfit()
        {
            return;
            try
            {
                var accountId = _accountSession.AccountID;
                if (accountId < 1 || string.IsNullOrEmpty(_accountSession.NickName) || _accountSession.IsAgency)
                {
                    await Clients.Caller.SendAsync("getProfitResult", Enums.ErrorCode.NotAuthen);
                    return;
                }
                if (_accountSession.IsAgency)
                {
                    await Clients.Caller.SendAsync("getProfitResult", Enums.ErrorCode.ERR_IS_AGENCY);
                    return;
                }
                if (!_accountSession.IsOTP)
                {
                    await Clients.Caller.SendAsync("getProfitResult", Enums.ErrorCode.ERR_NOT_PERMISSTION);
                    return;
                }
                DateTime dateTime = DateTime.Now.AddDays(-1);
                var key = "GetProfit" + _accountSession.NickName;
                if (_cacheHandler.MemTryGet(key, out dateTime))
                {
                    await Clients.Caller.SendAsync("getProfitResult", -1, -1);
                    return;
                }
                var response = _sql.GetProfit(accountId, _accountSession.NickName, 1000000,
                    _accountSession.PlatformID,
                    _accountSession.MerchantID);
                _cacheHandler.MemSet(_settings.TimeWaitGetProfit, key, DateTime.Now);
                //DECLARE @_PROCESS_SUCCESS INT = 0;
                //DECLARE @_Exception INT = -99
                //DECLARE @_ERR_BETTIME_EXPIRED INT = -207 -- Het thoi gian dat cuoc
                //DECLARE @_ERR_BETVALUE_INVALID INT = -212	-- Giá trị đặt cược không hợp lệ
                //DECLARE @_ERR_NOTBET_MULTILOCATION INT = -208 -- Dat 2 cua 1 phong
                //DECLARE @_ERR_BETTINGDATA_INVALID INT = -232; --Du lieu dat cuoc ko hop le
                //DECLARE @_ERR_OUT_OF_QUANTITY INT = -273; --Du lieu dat cuoc ko hop le
                //DECLARE @_ERR_RECEIVED INT = -274; --Du lieu dat cuoc ko hop le
                await Clients.Caller.SendAsync("getProfitResult", response.Item1, response.Item2);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                await Clients.Caller.SendAsync("getProfitResult", Enums.ErrorCode.Exception);
            }
        }

        [HubMethodName("GetTime")]
        public async Task GetTime()
        {
            try
            {
                var accountId = _accountSession.AccountID;
                if (accountId < 1 || string.IsNullOrEmpty(_accountSession.NickName))
                {
                    await Clients.Caller.SendAsync("getTimeResult", Enums.ErrorCode.NotAuthen);
                    return;
                }
                DateTime dateTime = DateTime.Now.AddDays(-1);
                var key = "GetProfit" + _accountSession.NickName;
                if (_cacheHandler.MemTryGet(key, out dateTime))
                {
                    var seconds = _settings.TimeWaitGetProfit - (int)(DateTime.Now - dateTime).TotalSeconds;
                    await Clients.Caller.SendAsync("getTimeResult", seconds);
                    return;
                }
                await Clients.Caller.SendAsync("getTimeResult", 0);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                await Clients.Caller.SendAsync("getTimeResult", Enums.ErrorCode.Exception);
            }
        }

        #endregion Event tán lộc

        #region VQMM

        [HubMethodName("VQMMSpin")]
        public async Task Spin()
        {
            var accountId = _accountSession.AccountID;
            if (accountId < 1 || string.IsNullOrEmpty(_accountSession.NickName) || _accountSession.IsAgency)
            {
                return;
            }
            NLogManager.LogInfo($"accountid: {accountId} nickname: {_accountSession.NickName} spin");
            if (_accountSession.IsAgency)
            {
                await Clients.Caller.SendAsync("VQMMSpinResult", Enums.ErrorCode.ERR_IS_AGENCY);
                return;
            }
            if (!_accountSession.IsOTP)
            {
                await Clients.Caller.SendAsync("VQMMSpinResult", Enums.ErrorCode.ERR_NOT_PERMISSTION);
                return;
            }
            DateTime dateTime = DateTime.Now.AddDays(-1);
            var key = "VQMMGet" + _accountSession.AccountName;
            if (!_settings.IsAlpha && _cacheHandler.MemTryGet(key, out dateTime))
            {
                await Clients.Caller.SendAsync("VQMMSpinResult", new VQMMSpin(), new List<VQMMSpin>());
                return;
            }
            var res = _jobAccess.EventVQMMSpin(accountId, _accountSession.NickName);
            NLogManager.LogInfo(JsonConvert.SerializeObject(res));
            await Clients.Caller.SendAsync("VQMMSpinResult", res, _jobAccess.VQMMGetAllPrize());
            _cacheHandler.MemSet(_settings.TimeWaitVQMM, key, DateTime.Now);
        }

        [HubMethodName("VQMMGet")]
        public async Task VQMMGet()
        {
            var accountId = _accountSession.AccountID;
            if (accountId < 1 || string.IsNullOrEmpty(_accountSession.NickName))
            {
                return;
            }
            DateTime dateTime = DateTime.Now.AddDays(-1);
            var key = "VQMMGet" + _accountSession.AccountName;
            if (!_settings.IsAlpha && _cacheHandler.MemTryGet(key, out dateTime))
            {
                var seconds = _settings.TimeWaitVQMM - (int)(DateTime.Now - dateTime).TotalSeconds;
                await Clients.Caller.SendAsync("VQMMGetResult", 0, seconds);
                return;
            }

            var res = _jobAccess.EventVQMMGet(accountId);
            NLogManager.LogInfo(key + " " + res);

            await Clients.Caller.SendAsync("VQMMGetResult", res);
        }

        [HubMethodName("VQMMGetFund")]
        public async Task VQMMGetFund()
        {
            var accountId = _accountSession.AccountID;
            if (accountId < 1 || string.IsNullOrEmpty(_accountSession.NickName))
            {
                return;
            }
            var res = _eventController.GetVQMMFund();
            await Clients.Caller.SendAsync("VQMMFund", res);
        }

        #endregion VQMM

        #region Nạp thẻ

        public async Task DepositGetPrizeByAccount()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _jobAccess.DepositGetPrizeByAccount(accountId);
                await Clients.Caller.SendAsync("DepositPrize", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public async Task DepositGetMoney(int prizeId)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username) || prizeId <= 0)
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var res = _jobAccess.GetMoney(_accountSession.AccountID, _accountSession.NickName, prizeId, _accountSession.IpAddress);
                await Clients.Caller.SendAsync("DepositGetMoneyResult", res.Item1, res.Item2);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        #endregion Nạp thẻ

        #region Quest

        public async Task QuestGetPrizeByAccount()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var response = _jobAccess.QuestGetPrizeByAccount(accountId);
                await Clients.Caller.SendAsync("QuestPrize", response);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }
        public async Task QuestGetMoney(int prizeId)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username) || prizeId <= 0)
            {
                NLogManager.LogInfo("PlayNow-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var res = _jobAccess.QuestGetMoney(_accountSession.AccountID, _accountSession.NickName, prizeId, _accountSession.IpAddress);
                await Clients.Caller.SendAsync("QuestGetMoneyResult", res.Item1, res.Item2);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }
        public async Task QuestGetListExchangeRate()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("QuestGetListExchangeRate-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var res = _jobAccess.QuestGetListExchangeRate();
                await Clients.Caller.SendAsync("QuestListExchangeRateResult", new { code = Enums.ErrorCode.Success, data = res });
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("QuestGetListResult", new { code = Enums.ErrorCode.Exception });
            }
        }
        public async Task QuestGetList()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("QuestGetListExchangeRate-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var res = _jobAccess.QuestGetList(accountId, username);
                await Clients.Caller.SendAsync("QuestGetListResult", new { code = Enums.ErrorCode.Success, data = res });
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("QuestGetListResult", new { code = Enums.ErrorCode.Exception });
            }
        }
        public async Task QuestGetPoint(long questId)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("QuestGetListExchangeRate-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var res = _jobAccess.QuestGetAward(questId, accountId);
                if (res < 0)
                {
                    await Clients.Caller.SendAsync("QuestGetAwardResult", new
                    {
                        code = Enums.ErrorCode.Failed,
                        description = "Chưa hoàn thành nhiệm vụ hoặc đã nhận thưởng rồi"
                    });

                }
                else
                {
                    await Clients.Caller.SendAsync("QuestGetAwardResult", new
                    {
                        code = Enums.ErrorCode.Success,
                        description = "Nhận thưởng thành công",
                        data = new { Point = res }
                    });
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("QuestGetAwardResult", new { code = Enums.ErrorCode.Exception });
            }
        }

        public async Task QuestGetPointBalance()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("QuestGetListExchangeRate-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var res = _jobAccess.QuestGetPointBalance(accountId);
                await Clients.Caller.SendAsync("QuestPointBalanceResult", new { code = Enums.ErrorCode.Success, data = res });
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("QuestPointBalanceResult", new { code = Enums.ErrorCode.Exception });
            }
        }
        public async Task QuestExchangePoint(int point)
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("QuestGetListExchangeRate-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var res = _jobAccess.QuestExchangePoint(accountId, point);
                if (res.Item1 < 0)
                {
                    switch (res.Item1)
                    {
                        case -1:
                            await Clients.Caller.SendAsync("QuestExchangePointResult",
                                new { code = Enums.ErrorCode.Failed, description = "Không tìm thấy tỉ lệ" });

                            break;
                        case -51:
                            await Clients.Caller.SendAsync("QuestExchangePointResult",
                                new { code = Enums.ErrorCode.Failed, description = "Không đủ số point" });
                            break;
                        default:
                            await Clients.Caller.SendAsync("QuestExchangePointResult",
                               new { code = Enums.ErrorCode.Failed, description = "Có lỗi xảy ra" });
                            break;
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("QuestExchangePointResult", new
                    {
                        code = Enums.ErrorCode.Success,
                        data = new
                        {
                            point = res.Item1,
                            balance = res.Item2
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("QuestExchangePointResult", new { code = Enums.ErrorCode.Exception });
            }
        }
        public async Task QuestAttendance()
        {
            long accountId = _accountSession.AccountID;
            string username = _accountSession.NickName;
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                NLogManager.LogInfo("QuestGetListExchangeRate-NotAuthen: AccountId: " + accountId + "|ClientIP: " + _accountSession.IpAddress);
                return;
            }
            try
            {
                var res = _jobAccess.QuestAttendance(accountId, username);
                await Clients.Caller.SendAsync("QuestAttendanceResult", new { code = Enums.ErrorCode.Success, data = res });
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                await Clients.Caller.SendAsync("QuestAttendanceResult", new { code = Enums.ErrorCode.Exception });
            }

        }
        #endregion Quest

        #region Base

        public override Task OnConnectedAsync()
        {
            try
            {
                long accountId = _accountSession.AccountID;
                string username = _accountSession.NickName;

                if (accountId < 1)
                {
                    return base.OnConnectedAsync();
                }
                NLogManager.LogInfo($"Account: {accountId}  {username} connect");

                _connection.PlayerConnect(accountId, Context.ConnectionId);
                var player = _playerHandler.GetPlayer(accountId);
                if (player == null)
                {
                    player = _playerHandler.AddPlayer(accountId, username);
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            long accountId = _connection.PlayerDisconnect(this.Context.ConnectionId);
            if (accountId < 1)
            {
                return base.OnDisconnectedAsync(exception);
            }

            _connection.PlayerDisconnect(Context.ConnectionId);
            _playerHandler.RemovePlayer(accountId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("pong");
        }

        #endregion Base
    }
}