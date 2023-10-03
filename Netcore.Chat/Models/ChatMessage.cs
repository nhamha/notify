using Newtonsoft.Json;
using System;

namespace Netcore.Chat.Models
{
    public class ChatMessage
    {
        [JsonProperty("i")]
        public string ChannelId;

        [JsonProperty("ad")]
        public bool IsAdmin;

        [JsonProperty("a")]
        public long AccountID;

        [JsonProperty("n")]
        public string NickName;

        [JsonProperty("c")]
        public string Content;

        [JsonProperty("v")]
        public int VipRank;

        [JsonIgnore]
        [JsonProperty("u")]
        public string Username;

        [JsonProperty("d"), JsonIgnore]
        public DateTime CreatedDate;

        [JsonProperty("l"), JsonIgnore]
        public DateTime LastModifiedDate;

        public ChatMessage()
        {
            CreatedDate = DateTime.Now;
        }

        public ChatMessage(string channelId, bool isAdmin, long accountID, string nickname, string username, string content)
        {
            ChannelId = channelId;
            IsAdmin = isAdmin;
            AccountID = accountID;
            NickName = nickname;
            Username = username;
            Content = content;
            CreatedDate = DateTime.Now;
        }
    }
}