using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Netcore.Chat.Models
{
    public class ChatUser
    {
        [JsonProperty("a")]
        public long AccountID;

        [JsonProperty("u")]
        public string UserName;

        [JsonProperty("n")]
        public string NickName;

        [JsonProperty("c")]
        public string ClientIP;

        [JsonProperty("v")]
        public int VipRank { get; set; }

        [JsonIgnore]
        public string UserAgent;

        [JsonIgnore]
        public bool IsActive;

        [JsonIgnore]
        public DateTime LastActivity;

        //[JsonIgnore]
        //public DateTime LastSendMessage;

        [JsonProperty("l")]
        public DateTime LastMessageSentTime;

        [JsonIgnore]
        public List<ChatMessage> LastMessages;

        [JsonIgnore]
        public DateTime JoinChatTime;

        [JsonIgnore]
        public ConcurrentDictionary<string, string> ChannelConnectionIds;

        [JsonIgnore]
        public int CountBadLinks;

        [JsonProperty("i")]
        public string ChannelID;

        public ChatUser()
        {
            IsActive = true;
            LastActivity = DateTime.Now;
            JoinChatTime = DateTime.Now;
            LastMessageSentTime = DateTime.Now.AddDays(-1);
            LastMessages = new List<ChatMessage>();
            ChannelConnectionIds = new ConcurrentDictionary<string, string>();
            CountBadLinks = 0;
            ChannelID = "";
        }

        public ChatUser(long accountId)
            : this()
        {
            AccountID = accountId;
        }

        public ChatUser(long accountId, string username, string nickname, string channelid)
            : this()
        {
            AccountID = accountId;
            UserName = username;
            NickName = nickname;
            ChannelID = channelid;
        }

        public void SetActive(bool active)
        {
            IsActive = active;

            if (active)
                LastActivity = DateTime.Now;
        }

        public void SetChannelConnectionId(string channelId, string connectionId)
        {
            if (Monitor.TryEnter(ChannelConnectionIds, 50000))
                try
                {
                    ChannelConnectionIds.TryAdd(channelId, connectionId);
                }
                finally
                {
                    Monitor.Exit(ChannelConnectionIds);
                }
        }

        public bool RemoveChannelConnectionId(string channelId, out string connectionId)
        {
            if (Monitor.TryEnter(ChannelConnectionIds, 50000))
                try
                {
                    return ChannelConnectionIds.TryRemove(channelId, out connectionId);
                }
                finally
                {
                    Monitor.Exit(ChannelConnectionIds);
                }
            connectionId = null;
            return false;
        }
    }
}