using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

// ===============================
// AUTHOR     : QuangPm
// SPECIAL NOTES: Các thao tác cache theo tài khoản - Ipaddress
// ===============================
namespace ServerCore.Utilities.Utils
{
    public class CacheCounter
    {
        public static string InsertCacheKey(int totalSecond, string keyName, string value)
        {
            try
            {
                System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
                System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
                };
                cache.Set(keyName, value, policy);
                return value;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return string.Empty;
        }

        public static string GetCacheKey(string keyName)
        {
            try
            {
                //string ip = IPAddressHelper.GetClientIP();
                System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
                object cacheCounter = cache.Get(keyName);
                if (cacheCounter == null)
                {
                    return string.Empty;
                }
                return cacheCounter.ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// Kiểm tra ip thực hiện 1 hành động trong số giây (tự cộng số lượt mỗi lần gọi hàm check)
        /// Không ăn theo tài khoản
        /// </summary>
        /// <param name="totalSecond">Số giây kiểm tra</param>
        /// <param name="action">tên hành động</param>
        /// <returns>số lượt gọi hành động</returns>
        public static int CheckIpPostFrequency(int totalSecond, string action, string ip)
        {
           // string ip = IPAddressHelper.GetClientIP();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
            };
            object cacheCounter = cache.Get("P" + ip.ToLower() + "_" + action);
            if (cacheCounter == null)
            {
                cache.Set("P" + ip.ToLower() + "_" + action, 1, policy);
                return 0;
            }
            cache.Set("P" + ip.ToLower() + "_" + action, Convert.ToInt32(cacheCounter) + 1, policy);
            return Convert.ToInt32(cacheCounter);
        }

        /// <summary>
        /// Đếm số lượt của hành động dựa trên IP
        /// </summary>
        /// <param name="action">truyền vào tên action</param>
        /// <returns></returns>
        public static int IpActionCounter(string action, string ip)
        {
          //  string ip = IPAddressHelper.GetClientIP();


            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            object cacheCounter = cache.Get("P" + ip.ToLower() + "_" + action);
            return Convert.ToInt32(cacheCounter);
        }


        /// <summary>
        /// Xóa ip action cache
        /// </summary>
        /// <param name="action">truyền vào tên action</param>
        /// <returns></returns>
        public static void IpActionDelete(string action, string ip)
        {
           // string ip = IPAddressHelper.GetClientIP();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            cache.Remove("P" + ip.ToLower() + "_" + action);
        }


        /// <summary>
        /// Kiểm tra tài khoản thực hiện 1 hành động trong số giây (tự cộng số lượt mỗi lần gọi hàm check)
        /// </summary>
        /// <param name="accountName">Tên tài khoản</param>
        /// <param name="totalSecond">Số giây kiểm tra</param>
        /// <param name="action">tên hành động</param>
        /// <returns>số lượt gọi hành động</returns>
        public static int CheckAccountActionFrequency(string accountName, int totalSecond, string action)
        {
            accountName = accountName.ToLower();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
            };
            object cacheCounter = cache.Get("P" + accountName + "_" + action);
            if (cacheCounter == null)
            {
                cache.Set("P" + accountName + "_" + action, 1, policy);
                return 0;
            }
            cache.Set("P" + accountName + "_" + action, Convert.ToInt32(cacheCounter) + 1, policy);
            return Convert.ToInt32(cacheCounter);
        }


        /// <summary>
        /// Đếm số lượt của hành động dựa trên tài khoản
        /// </summary>
        /// <param name="accountName">truyền vào tên tài khoản</param>
        /// <param name="action">truyền vào tên action</param>
        /// <returns></returns>
        public static int AccountActionCounter(string accountName, string action)
        {
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            object cacheCounter = cache.Get("P" + accountName + "_" + action);
            return Convert.ToInt32(cacheCounter);
        }

        /// <summary>
        /// Đếm số lượt hành động của 1 account dựa trên ip trong số giây
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="action"></param>
        /// /// <param name="totalSecond"></param>
        /// <returns></returns>
        public static int AccountIPActionCounter(string accountName, string action, int totalSecond, string ip)
        {

            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
            };

            object cacheCounter = cache.Get("P" + accountName + "_" + ip.ToLower() + "_" + action);
            if (cacheCounter == null)
            {
                cache.Set("P" + accountName + "_" + ip.ToLower() + "_" + action, 1, policy);
                return 0;
            }
            cache.Set("P" + accountName + "_" + ip.ToLower() + "_" + action, Convert.ToInt32(cacheCounter) + 1, policy);
            return Convert.ToInt32(cacheCounter);
        }

        /// <summary>
        /// Xóa Account action cache
        /// </summary>
        /// <param name="action">truyền vào tên action</param>
        /// <returns></returns>
        public static void AccountActionDelete(string accountName, string action)
        {
            accountName = accountName.ToLower();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            cache.Remove("P" + accountName + "_" + action);
        }

        public static void SetCache(string key, string value, int timeToLive)
        {
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(timeToLive)
            };
            cache.Set("P" + key, value, policy);
        }

        public static void SetCache(string key, object value, int timeToLive)
        {
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(timeToLive)
            };
            cache.Set("P" + key, value, policy);
        }

        public static string GetCache(string key)
        {
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            object val = cache.Get("P" + key);
            if (val != null)
                return val.ToString();

            return null;
        }

        public static long GetCacheLong(string key)
        {
            ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            object val = cache.Get("P" + key);
            if (val != null)
                return Convert.ToInt64(val);

            return 0;
        }

        public static void RemoveCache(string key)
        {
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            cache.Remove("P" + key);
        }
    }
}