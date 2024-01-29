using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netcore.Notification.DataAccess;
using Netcore.Notification.Models;
using NetCore.Utils.Interfaces;
using NetCore.Utils.Log;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Netcore.Notification.Controllers
{
    public class NotificationHandler
    {
        private readonly ConnectionHandler _connection;
        private readonly SQLAccess _sql;
        private readonly AppSettings _settings;
        private readonly PlayerHandler _playerHandler;
        private readonly IDataService _dataService;

        private ImmutableList<SystemNotification> lstNotification = ImmutableList<SystemNotification>.Empty;
        private ImmutableList<SystemNotification> lstTopJackpot = ImmutableList<SystemNotification>.Empty;
        private ImmutableList<PopupNotification> lstPopNotification = ImmutableList<PopupNotification>.Empty;
        private ImmutableList<UserNotification> lstUserNotification = ImmutableList<UserNotification>.Empty;
        private ImmutableList<LobbyText> lstLobbyText = ImmutableList<LobbyText>.Empty;
        int countUserNotifyTimer = 0;
        public NotificationHandler(ConnectionHandler connection, SQLAccess sql,
            IOptions<AppSettings> options, PlayerHandler playerHandler, IDataService dataService)
        {
            _playerHandler = playerHandler;
            _connection = connection;
            _sql = sql;
            _settings = options.Value;
            _dataService = dataService;
            var aTimer = new System.Timers.Timer(_settings.Timer);
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.Enabled = true;
        }

        private void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ServerElapsed();
        }

        public void ServerElapsed()
        {
            try
            {
                // CountUserNotifyTimer++;
                var res = _sql.GetSystemNotification(0);
                if (res != null || res.Count > 0)
                    lstNotification = res.ToImmutableList();
                lstLobbyText = _sql.GetLobbyText().ToImmutableList();
                if (lstNotification.Count > 0)
                {
                    _connection._hubContext.Clients.All.SendAsync("NotifySystem", new { GameID = 0, Data = lstNotification });
                }
                if (lstLobbyText.Count > 0)
                {
                    _connection._hubContext.Clients.All.SendAsync("LobbyText", lstLobbyText);
                }
                countUserNotifyTimer++;
                if (countUserNotifyTimer == 10)
                {
                    countUserNotifyTimer = 0;
                    var topJackpot = _sql.GetTopJackpot(0);
                    _connection._hubContext.Clients.All.SendAsync("topJackpot", topJackpot);

                    foreach (var item in topJackpot)
                    {
                        string gameName = string.Empty;
                        switch (item.GameID)
                        {
                            case 49:
                                gameName = "Windy";
                                break;
                            case 50:
                                gameName = "Halloween";
                                break;
                            case 53:
                                gameName = "Fox";
                                break;
                            case 100:
                                gameName = "Phục Sinh";
                                break;
                            case 101:
                                gameName = "Kungfu Panda";
                                break;
                            case 103:
                                gameName = "Ariel";
                                break;
                            case 104:
                                gameName = "Cowboy";
                                break;
                            case 105:
                                gameName = "Jungle";
                                break;
                            case 109:
                                gameName = "Gem";
                                break;
                            default:
                                break;
                        }
                        string content = string.Format("Chúc mừng {0} trúng hũ {1} game {2}", item.UserName, item.Amount.ToString("n0"), gameName);
                        NLogManager.LogInfo(content);
                        if (!string.IsNullOrEmpty(_settings.OneSignal))
                        {
                            var url = _settings.OneSignal + "?content=" + content;
                            _dataService.GetAsync(url);
                        }

                    }


                }
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
        }

        public List<SystemNotification> GetSystemNotification(int gameid)
        {
            return lstNotification.ToList();
        }
        public List<LobbyText> GetLobbyText()
        {
            return lstLobbyText.ToList();
        }

        public List<UserNotification> GetUserMail(long accountID, string nickName)
        {
            var res = _sql.GetUserMail(accountID, nickName);
            return res;
        }

        public List<UserNotification> GetUserPopup(long accountID, string nickName)
        {
            var res = _sql.GetUserPopup(accountID, nickName);
            return res;
        }

        public int GetUnReadUserNotifyQuantity(long accountID, string nickName)
        {
            var res = _sql.GetUnReadUserNotifyQuantity(accountID, nickName);
            return res;
        }

        //public List<UserQuantityNotification> GetUnSendUserNotifyQuantity(string userName)
        //{
        //    var res = _sql.GetGetUnSendUserNotifyQuantity(userName);
        //    return res;
        //}

        public string GetUserNotifyContent(long notifyID, long accountID, string nickName)
        {
            var res = _sql.GetUserNotifyContent(notifyID, accountID, nickName);
            return res;
        }

        public int DeleteUserNotify(long notifyID, long accountId, string nickName)
        {
            var res = _sql.DeleteUserNotify(notifyID, accountId, nickName);
            return res;
        }

        public bool CreateUserNotify(UserNotification data)
        {
            var res = _sql.CreateUserNotify((int)data.AccountID, data.UserName, data.Type, data.Title, data.Content);
            return res;
        }

        public void SetReadUserNotification(long notifyID, long accountID)
        {
            //var res = _sql.SetUserNotifyReadByID(notifyID, accountID);
            //var connectionID = ConnectionHandler.Instance.GetConnections(accountID).FirstOrDefault();
            //if (string.IsNullOrWhiteSpace(connectionID)) return;
            //var notify = lstUserNotification.FirstOrDefault(n => n.NotifyID == notifyID);
            //if (notify == null) return;

            //ConnectionHandler.Instance.HubContext.Clients.Client(connectionID).notifyNewMail(item);
            //return res;
        }

        public void SendPopupNoti(long accountId, string content, long balance, int type)
        {
            var connections = _connection.GetConnections(accountId);
            _connection._hubContext.Clients.Clients(connections).SendAsync("popup", content, type, balance);
        }
        public void UserShareProfit(string nickName, long prizeValue)
        {
            _connection._hubContext.Clients.All.SendAsync("userShareProfit", nickName, prizeValue);
        }

        //public List<UserNotification> SetReadPopupNotification(long notifyID, long accountID)
        //{
        //    var res = _sql.SetUserNotifyReadByID(notifyID, accountID);
        //    return res;
        //}
    }
}