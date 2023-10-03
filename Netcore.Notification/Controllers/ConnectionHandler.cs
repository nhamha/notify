using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Netcore.Notification.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Netcore.Notification.Controllers
{
    public class ConnectionHandler
    {
        private readonly ILogger<ConnectionHandler> _logger;

        private readonly ConcurrentDictionary<string, long> _mapHubAccount = new ConcurrentDictionary<string, long>();

        private readonly ConcurrentDictionary<long, List<string>> _mapAccountHub = new ConcurrentDictionary<long, List<string>>();

        public readonly IHubContext<GateHub> _hubContext;

        public ConnectionHandler(IHubContext<GateHub> hubContext, ILogger<ConnectionHandler> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void AddGroup(string connectionId, string groupName)
        {
            _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
        }

        public void RemoveGroup(string connectionId, string groupName)
        {
            _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
        }

        public string PlayerConnect(long accountId, string connection)
        {
            if (accountId < 1 || string.IsNullOrEmpty(connection))
            {
                return string.Empty;
            }
            _mapHubAccount.TryAdd(connection, accountId);

            if (!_mapAccountHub.ContainsKey(accountId))
            {
                List<string> list = new List<string> { connection };
                _mapAccountHub.TryAdd(accountId, list);
            }
            else
            {
                List<string> list = null;
                if (!_mapAccountHub.TryGetValue(accountId, out list))
                {
                    return string.Empty;
                }

                if (!Monitor.TryEnter(list, 2000)) return string.Empty;
                if (list.Count < 1)
                {
                    return string.Empty;
                }
                try
                {
                    string first = list.FirstOrDefault();
                    list.Clear();
                    list.Add(connection);
                    return first;
                }
                finally
                {
                    Monitor.Exit(list);
                }
            }
            return string.Empty;
        }

        public long PlayerDisconnect(string connection)
        {
            if (string.IsNullOrEmpty(connection))
            {
                return -1;
            }
            long accountId = 0;
            _mapHubAccount.TryRemove(connection, out accountId);

            if (!_mapHubAccount.TryGetValue(connection, out accountId)) return accountId;
            List<string> list = null;
            _mapAccountHub.TryGetValue(accountId, out list);
            {
                if (list == null)
                {
                    return accountId;
                }

                if (!Monitor.TryEnter(list, 2000)) return accountId;
                try
                {
                    if (list.Contains(connection))
                    {
                        list.Remove(connection);
                    }
                    if (list.Count == 0)
                    {
                        _mapAccountHub.TryRemove(accountId, out list);
                    }
                }
                finally
                {
                    Monitor.Exit(list);
                }
            }
            return accountId;
        }

        public long GetAccountIdByConnectionId(string connectionId)
        {
            long current = 0;
            _mapHubAccount.TryGetValue(connectionId, out current);
            return current;
        }

        public IReadOnlyList<string> GetConnections(long accountId)
        {
            IReadOnlyList<string> listReturn = new List<string>().AsReadOnly();
            if (accountId < 1 || !_mapAccountHub.ContainsKey(accountId))
            {
                return listReturn;
            }

            List<string> trygetList = null;
            if (!_mapAccountHub.TryGetValue(accountId, out trygetList)) return listReturn;
            return trygetList != null ? trygetList.AsReadOnly() : listReturn;
        }

        public long GetAccountId(string connectionId)
        {
            const long accountId = 0;
            try
            {
                if (string.IsNullOrEmpty(connectionId)) return accountId;
                var lst = _mapHubAccount.ToList();
                var player = lst.FindAll(c => c.Key == connectionId);
                if (player.Count <= 0) return accountId;
                return player.FirstOrDefault().Value;
            }
            catch (Exception exception)
            {
                _logger.LogError("error: ", exception);
            }
            return accountId;
        }

        public string Replace(int accountid, string connectionid)
        {
            if (string.IsNullOrEmpty(connectionid) || accountid < 1)
            {
                return string.Empty;
            }
            if (!_mapAccountHub.Keys.Contains(accountid))
            {
                return string.Empty;
            }

            List<string> list = null;
            if (!_mapAccountHub.TryGetValue(accountid, out list)) return string.Empty;
            if (list.Count < 1)
            {
                return string.Empty;
            }

            if (!Monitor.TryEnter(list, 2000)) return string.Empty;
            try
            {
                if (list.Contains(connectionid))
                {
                    return string.Empty;
                }
                else
                {
                    string first = list.FirstOrDefault();
                    list.Clear();
                    list.Add(connectionid);
                    return first;
                }
            }
            finally
            {
                Monitor.Exit(list);
            }
        }

        #region SendMessage

        public async Task SendMessageToClient(long accountid, object message)
        {
            IReadOnlyList<string> list = GetConnections(accountid);
            foreach (var str in list)
            {
                await _hubContext.Clients.Client(str).SendAsync("message", message, 3);
            }
        }

        #endregion SendMessage
    }
}