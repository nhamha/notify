using Microsoft.Extensions.Logging;
using Netcore.Notification.Models;
using NetCore.Utils.Log;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Netcore.Notification.Controllers
{
    public class PlayerHandler
    {
        // Singleton instance
        private readonly ConcurrentDictionary<long, GamePlayer> _players;

        private readonly ILogger<PlayerHandler> _logger;

        public PlayerHandler(ILogger<PlayerHandler> logger)
        {
            _logger = logger;
            _players = new ConcurrentDictionary<long, GamePlayer>();
        }

        #region dictionary_methods

        private GamePlayer CreatePlayer(params object[] args)
        {
            return (GamePlayer)Activator.CreateInstance(typeof(GamePlayer), args);
        }

        public bool Contains(long accountId)
        {
            return _players.ContainsKey(accountId);
        }

        /// <summary>
        /// lấy player theo accountID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public GamePlayer GetPlayer(long accountId)
        {
            if (accountId < 1)
            {
                return null;
            }

            GamePlayer player = null;
            if (Contains(accountId))
            {
                _players.TryGetValue(accountId, out player);
            }
            return player;
        }

        public bool AddPlayer(GamePlayer player)
        {
            if (player == null)
            {
                return false;
            }

            if (_players.ContainsKey(player.Account.AccountID))
            {
                var oldAccount = player.Account;
                //update account
                if (!Monitor.TryEnter(oldAccount, 5000))
                    return true;
                try
                {
                    Copy(player.Account, oldAccount);
                }
                finally
                {
                    Monitor.Exit(oldAccount);
                }
                return true;
            }
            return _players.TryAdd(player.Account.AccountID, player);
        }

        public GamePlayer AddPlayer(Account account)
        {
            if (account == null)
            {
                return null;
            }
            if (account.AccountID < 1)
            {
                return null;
            }

            GamePlayer player = null;
            if (_players.ContainsKey(account.AccountID))
            {
                player = GetPlayer(account.AccountID);
                var oldAccount = player.Account;
                //update account
                if (!Monitor.TryEnter(oldAccount, 5000))

                    return player;
                try
                {
                    Copy(player.Account, oldAccount);
                }
                finally
                {
                    Monitor.Exit(oldAccount);
                }
                return player;
            }

            player = CreatePlayer(account);
            if (player == null)
                return null;
            AddPlayer(player);
            return player;
        }

        public GamePlayer AddPlayer(long accountId, string username)
        {
            if (accountId < 1 || string.IsNullOrEmpty(username))
            {
                return null;
            }

            if (_players.ContainsKey(accountId))
            {
                return GetPlayer(accountId);
            }
            try
            {
                var account = new Account()
                {
                    AccountID = accountId,
                    AccountName = username
                };

                return AddPlayer(account);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return null;
        }

        public bool RemovePlayer(long accountId)
        {
            GamePlayer player = null;
            return _players.TryRemove(accountId, out player);
        }

        public int Count()
        {
            return _players.Count;
        }

        #endregion dictionary_methods

        #region util_methods

        /// <summary>
        /// Copy from first to second.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void Copy(Account first, Account second)
        {
            second.AccountName = first.AccountName;
            second.AccountID = first.AccountID;
        }

        public Account GetAccount(long accountId)
        {
            if (accountId < 1)
            {
                return null;
            }

            GamePlayer player = null;
            return _players.TryGetValue(accountId, out player) ? player.Account : null;
        }

        #endregion util_methods
    }
}