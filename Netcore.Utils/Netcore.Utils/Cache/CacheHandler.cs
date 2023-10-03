using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using NetCore.Utils.Log;
using NetCore.Utils.Sessions;
using System;

namespace NetCore.Utils.Cache
{
    public class CacheHandler
    {
        private readonly AccountSession _accountSession;
        //private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memCache;

        public CacheHandler(/*IDistributedCache distributedCache,*/ IMemoryCache memCache, AccountSession accountSession)
        {
            //_distributedCache = distributedCache;
            _memCache = memCache;
            _accountSession = accountSession;
        }

        public bool MemTryGet<T>(string keyName, out T value)
        {
            value = default(T);
            try
            {
                if (_memCache.TryGetValue(keyName, out value))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }

        public T MemSet<T>(int totalSecond, string keyName, T value)
        {
            try
            {
                var res = _memCache.Set(keyName, value, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(totalSecond)
                });
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return default(T);
        }
        public int ActionCounter(int totalSecond, string action)
        {
            var count = 0;
            if (MemTryGet(action, out count))
            {
                count++;
            }
            else
            {
                count = 1;
            }
            MemSet(totalSecond, action, count);
            return count;
        }
        public int ActionCounterByIpAddress(int totalSecond, string action)
        {
            var keyName = _accountSession.IpAddress + "_" + action;
            return ActionCounter(totalSecond, keyName);
        }
        public int ActionCounterByAccountName(int totalSecond, string action)
        {
            var keyName = _accountSession.AccountName + "_" + action;
            return ActionCounter(totalSecond, keyName);

        }
        /// <summary>
        /// check xem có được thực hiện hành động không
        /// </summary>
        /// <param name="totalSecond"></param>
        /// <param name="action"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public bool CheckAction(string action, int number)
        {
            var count = 0;
            if (MemTryGet(action, out count))
            {
                if (count < number) return true;
                else
                {
                    NLogManager.LogInfo(string.Format("{0} quá {1} lần", action, number));
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public bool CheckActionByIpAddress(string action, int number)
        {
            var keyName = _accountSession.IpAddress + "_" + action;
            return CheckAction(keyName, number);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="number"></param>
        /// <returns>false: k được thực hiện, true đc thực hiện</returns>
        public bool CheckActionByAccountName(string action, int number)
        {
            var keyName = _accountSession.AccountName + "_" + action;
            return CheckAction(keyName, number);

        }
    }
}