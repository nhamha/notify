using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Netcore.Chat.Models
{
    public class ChatChannel
    {
        private const int _lockTime = 50; //ms
        public int Type;
        public string ChannelId;
        public ConcurrentDictionary<long, ChatUser> UserOnlines;
        public List<ChatMessage> LastMessages;
        public ChatMessage pinMessage { get; set; }

        public ChatChannel(string channelId)
        {
            ChannelId = channelId;
            UserOnlines = new ConcurrentDictionary<long, ChatUser>();
            LastMessages = new List<ChatMessage>();
            pinMessage = new ChatMessage();
        }

        public bool AddUser(ChatUser chatUser)
        {
            if (chatUser == null)
                return false;

            const bool ret = false;
            if (Monitor.TryEnter(UserOnlines, _lockTime))
            {
                try
                {
                    ChatUser newChatUser = new ChatUser(chatUser.AccountID, chatUser.UserName, chatUser.NickName, "abc1")
                    {
                        ClientIP = chatUser.ClientIP
                    };
                    UserOnlines.GetOrAdd(chatUser.AccountID, newChatUser);
                    return true;
                }
                finally
                {
                    Monitor.Exit(UserOnlines);
                }
            }
            return ret;
        }

        public ChatUser GetUser(long accountId)
        {
            ChatUser chatUser = null;
            if (Monitor.TryEnter(UserOnlines, _lockTime))
            {
                try
                {
                    UserOnlines.TryGetValue(accountId, out chatUser);
                }
                finally
                {
                    Monitor.Exit(UserOnlines);
                }
            }
            return chatUser;
        }

        public ChatUser RemoveUser(long accountId)
        {
            ChatUser chatUser = null;
            if (Monitor.TryEnter(UserOnlines, _lockTime))
            {
                try
                {
                    UserOnlines.TryRemove(accountId, out chatUser);
                    return chatUser;
                }
                finally
                {
                    Monitor.Exit(UserOnlines);
                }
            }

            return chatUser;
        }
    }
}