using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Netcore.Notification.Models;
using NetCore.Utils.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Netcore.Notification.Controllers
{
    public class XJackpotController
    {
        private readonly AppSettings _settings;
        private readonly ConnectionHandler _connection;
        private readonly IDataService _dataService;
        private ConcurrentDictionary<string, XJackpotInfo> lstEventInfo = new ConcurrentDictionary<string, XJackpotInfo>();

        public XJackpotController(IOptions<AppSettings> options, IDataService dataService, ConnectionHandler connection)
        {
            _dataService = dataService;
            _settings = options.Value;
            _connection = connection;
            var aTimer = new Timer(30000);
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.Enabled = true;
            GetListEventInfoFromSV();
        }

        private async void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await GetListEventInfoFromSV();
        }

        public List<XJackpotInfo> GetListEventInfo()
        {
            return lstEventInfo.Values.ToList();
        }

        public async Task GetListEventInfoFromSV()
        {
            var lstUrl = _settings.EventLinks.Split(',');
            var gameIDs = _settings.EventGameIDs.Split(',');
            for (var i = 0; i < lstUrl.Length; i++)
            {
                var eventInfo = await _dataService.GetAsync<XJackpotInfo>(lstUrl[i], false);
                if (eventInfo == null)
                {
                    continue;
                }
                eventInfo.GameID = gameIDs[i];
                lstEventInfo.AddOrUpdate(gameIDs[i], eventInfo, (k, v) => eventInfo);
            }
            await _connection._hubContext.Clients.All.SendAsync("events", lstEventInfo.Values.ToList());
        }
    }
}