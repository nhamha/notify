using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Netcore.Notification.Models;
using NetCore.Utils.Interfaces;
using NetCore.Utils.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Netcore.Notification.Controllers
{
    public class JackpotController
    {
        private readonly AppSettings _settings;
        private readonly ConnectionHandler _connection;
        private readonly IDataService _dataService;
        private ConcurrentDictionary<string, Jackpot> lstJackpot = new ConcurrentDictionary<string, Jackpot>();

        public JackpotController(IOptions<AppSettings> options, IDataService dataService, ConnectionHandler connection)
        {
            _dataService = dataService;
            _settings = options.Value;
            _connection = connection;
            var aTimer = new Timer(3000);
            aTimer.Elapsed += new ElapsedEventHandler(aTimer_Elapsed);
            aTimer.Enabled = true;
        }

        private async void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await GetListJackpotFromSV();
        }

        public List<Jackpot> GetListJackpot()
        {
            return lstJackpot.Values.ToList();
        }

        public async Task GetListJackpotFromSV()
        {
            try
            {
                var lstUrl = _settings.Links.Split(',');
                var gameIDs = _settings.GameIDs.Split(',');
                for (var i = 0; i < lstUrl.Length; i++)
                {
                    var strJackpot = await _dataService.GetAsync(lstUrl[i], false);
                    var jackpot = new Jackpot
                    {
                        GameID = Convert.ToInt32(gameIDs[i]),
                        JackpotFund = strJackpot.Replace("\"", "")
                    };
                    lstJackpot.AddOrUpdate(gameIDs[i], jackpot, (k, v) => jackpot);
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            await _connection._hubContext.Clients.All.SendAsync("jackpots", lstJackpot.Values.ToList());

        }
    }
}