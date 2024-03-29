﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Netcore.Notification.DataAccess;
using Netcore.Notification.Models;
using NetCore.Utils.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netcore.Notification.Controllers
{
    public class EventController
    {
        private readonly ConnectionHandler _connection;
        private readonly SQLAccess _sql;
        private readonly JobEventAccess _eventSql;
        private readonly AppSettings _settings;
        private readonly PlayerHandler _playerHandler;
        private List<ShareProfit> _listShareProfit = new List<ShareProfit>();
        private List<VQMMSpin> _listVQMMUserPrize = new List<VQMMSpin>();
        private long VQMMFund = 0;

        public EventController(ConnectionHandler connection, SQLAccess sql, JobEventAccess eventSql,
            IOptions<AppSettings> options, PlayerHandler playerHandler)
        {
            _playerHandler = playerHandler;
            _connection = connection;
            _sql = sql;
            _eventSql = eventSql;
            _settings = options.Value;
            var aTimer = new System.Timers.Timer(3000);
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.Enabled = true;
        }
        private void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Task.Run(async () =>
            {
                //VQMMFund = _eventSql.GetVQMMFund();
                //await _connection._hubContext.Clients.All.SendAsync("VQMMFund", VQMMFund);
                await Task.Delay(500);
                _listShareProfit = _sql.GetListShareProfit();
                _listVQMMUserPrize = _eventSql.VQMMGetUserPrize();
                await _connection._hubContext.Clients.All.SendAsync("listShareProfit", _listShareProfit, _listVQMMUserPrize);
            });
        }
        public long GetVQMMFund()
        {
            return VQMMFund;
        }
        public List<ShareProfit> GetListShareProfit()
        {
            return _listShareProfit;
        }

        public List<VQMMSpin> GetListVQMMUserPrize()
        {
            return _listVQMMUserPrize;
        }
        public long FootballGetGift(long accountId, string accountName, int prizeId, ref long balance)
        {
            return _sql.FootballGetGift(accountId, accountName, prizeId, ref balance);
        }
        public Football FootballGetAccountInfo(long accountId)
        {
            return _sql.FootballGetAccountInfo(accountId);

        }
        public List<FootballGiftPrize> FootballGetPrizeByAccount(long accountId)
        {
            return _sql.FootballGetPrizeByAccount(accountId);

        }
        public List<Football> FootballGetTop()
        {
            return _sql.FootballGetTop();
        }
        public List<Football> FootballGetAllGiftHistory()
        {
            return _sql.FootballGetAllGiftHistory();
        }
        public List<FootballGiftPrize> FootballGetAllGiftPrize()
        {
            return _sql.FootballGetAllGiftPrize();
        }
        public int GetMoney(long accountID, string userName, long AccountPrizeId, int type, string ClientIP, out long Balance)
        {
            NLogManager.LogInfo(string.Format("GetMoney =>> accountID:{0} | userName: {1}, AccountPrizeId: {2}, ClientIP:{3} Type: {4} ", accountID, userName, AccountPrizeId, ClientIP, type));
            var res = _sql.GetMoney(accountID, userName, AccountPrizeId, type, ClientIP, out Balance);
            NLogManager.LogInfo("GetBON Result: " + res.ToString());
            return res;
        }
    }
}
