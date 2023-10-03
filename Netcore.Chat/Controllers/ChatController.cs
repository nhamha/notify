using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Netcore.Chat.Hubs;
using Netcore.Chat.Interfaces;
using Netcore.Chat.Models;
using NetCore.Utils.Interfaces;
using NetCore.Utils.Log;
using NetCore.Utils.Sessions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Netcore.Chat.Controllers
{
    public class ChatController
    {
        private readonly object _lockChannel = new object();
        private readonly object _lockUser = new object();
        private const int _lockTime = 5000; //ms
        public readonly IHubContext<ChatHub> _hubContext;
        private readonly AccountSession _accountSession;
        private readonly AppSettings _settings;
        private readonly IHostingEnvironment _env;
        private readonly ISQLAccess _sql;
        private readonly ChatFilter _chatfilter;
        private readonly IDataService _dataService;

        /// <summary>
        /// Quản lý danh sách user Online
        /// </summary>
        private readonly ConcurrentDictionary<long, ChatUser> UserOnlines = new ConcurrentDictionary<long, ChatUser>();

        /// <summary>
        /// Quản lý các kênh Chat
        /// </summary>
        private readonly ConcurrentDictionary<string, ChatChannel> Channels = new ConcurrentDictionary<string, ChatChannel>();

        /// <summary>
        /// Quản lý các Connection
        /// </summary>
        private readonly ConcurrentDictionary<string, long> ConnectionIdAccountId = new ConcurrentDictionary<string, long>();

        public int TIME_INTERVAL = 60;

        /// <summary>
        /// Số lượng tối đa client online trong 1 kênh chat
        /// </summary>
        public int MAX_IDLE_USER_ONLINE = 600;

        /// <summary>
        /// Số lượng message tối đa trong 1 kênh chat
        /// </summary>
        public int MAX_MESSAGE_IN_CHANNEL = 100;

        /// <summary>
        /// Số ký tự tối đa của nội dung Chat
        /// </summary>
        public int MAX_MESSAGE_LENGTH = 200;

        /// <summary>
        /// Thời gian chát 1 tin nhắn
        /// </summary>
        private int MIN_MESSAGE_TIME_SECOND = 5;

        public int MIN_USER_INACTIVE_IN_CHANNEL = 10;

        public int TEN_MESSAGE_DURATION = 30;
        public int DUPLICATE_MESSAGE_DURATION = 300;
        public int TWO_MESSAGE_DURATION = 2;
        public int GLOBAL_TEN_SAME_MESSAGE_DURATION = 1800;

        private Timer _timerClean;

        //private readonly System.Timers.Timer aTimer;
        private List<ChatMessage> notifyList;

        /// <summary>
        /// Constructor - private
        /// </summary>
        /// <param name="hubContext"></param>
        public ChatController(IHubContext<ChatHub> hubContext, IOptions<AppSettings> options, IDataService dataService,
           IHostingEnvironment env, AccountSession accountSession, ChatFilter chatfilter, ISQLAccess sql)
        {
            _chatfilter = chatfilter;
            _accountSession = accountSession;
            _env = env;
            _sql = sql;
            _dataService = dataService;

            _hubContext = hubContext;
            _settings = options.Value;
            notifyList = new List<ChatMessage>();
            TIME_INTERVAL = _settings.TimeInterval;
            MAX_IDLE_USER_ONLINE = _settings.MaxIdleUserOnline;
            MAX_MESSAGE_IN_CHANNEL = _settings.MaxMessageInChanel;
            MAX_MESSAGE_LENGTH = _settings.MaxMessageLength;
            TEN_MESSAGE_DURATION = _settings.TenMessageDuration;
            DUPLICATE_MESSAGE_DURATION = _settings.DuplicateMessageDuration;
            TWO_MESSAGE_DURATION = _settings.TwoMessageDuration;
            MIN_MESSAGE_TIME_SECOND = _settings.MinMessageTimeSecond;
            TimerCallback cbClean = new TimerCallback(RemoveInactive);
            _timerClean = new Timer(cbClean, null, TIME_INTERVAL * 1000, TIME_INTERVAL * 1000); // 10 minutes
        }

        /// <summary>
        /// Check kết nối
        /// </summary>
        /// <param name="hubCallerContext"></param>
        public void PingPong(long accountId)
        {
            try
            {
                if (accountId < 1)
                {
                    return;
                }

                ChatUser chatUser = GetUser(accountId);
                if (chatUser != null)
                    chatUser.LastActivity = DateTime.Now;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
        }

        /// <summary>
        /// Gửi message chat
        /// </summary>
        /// <param name="hubCallerContext"></param>
        /// <param name="message"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public bool SendMessage(string connectionId, long accountId, string message, string channelId)
        {
            try
            {
                if (!string.IsNullOrEmpty(_settings.CloseChatServer))
                {
                    return false;
                }
                //  NLogManager.LogInfo("vao day ko 1");
                //LoadListAdmin();
                if (message.Length > MAX_MESSAGE_LENGTH)
                {
                    BroadcastMessage(connectionId, string.Format("Chat không vượt quá {0} kí tự!", MAX_MESSAGE_LENGTH));
                    return false;
                }
                // NLogManager.LogInfo("vao day ko 2");

                if (accountId < 1)
                {
                    NLogManager.LogInfo(string.Format("Sending message: not authenticated accountId: {0} - channel: {1} - content={2}", accountId, channelId, message));
                    return false;
                }
                //NLogManager.LogInfo("vao day ko 3");
                ChatChannel chatChannel = GetChannel(channelId);
                if (chatChannel == null)
                {
                    NLogManager.LogInfo(string.Format("Sending message: accountId: {0} - not has channel: {1} - content={2}", accountId, channelId, message));
                    return false;
                }
                //NLogManager.LogInfo("vao day ko 4");

                ChatUser chatUser = GetUser(accountId);
                if (chatUser == null)
                {
                    NLogManager.LogInfo(string.Format("Sending message: not chat user: {0} in channel={1} - content={2}", accountId, channelId, message));
                    return false;
                }

                //if (!AccountSession.IsOTP && !isAdmin)
                //{
                //    BroadcastMessage(hubCallerContext.ConnectionId, string.Format("Tài khoản cần có tối thiểu 10K và đã cài OTP!"));
                //    return false;
                //}
                //NLogManager.LogInfo("vao day ko 5");
                var isAdmin = _chatfilter.IsAdmin(chatUser.UserName);
                if (message.Contains("/pin") && isAdmin)
                {
                    message = message.Replace("/pin", "");
                    var pinText = new ChatMessage(channelId, !isAdmin ? false : true, chatUser.AccountID, !isAdmin ? chatUser.NickName : string.Format("{0}", chatUser.NickName), chatUser.UserName, message);
                    chatChannel.pinMessage = pinText;
                    PinMessage(pinText);
                }
                //NLogManager.LogInfo("vao day ko 6");

                if (message.Contains("/remove") && isAdmin)
                {
                    var emptyMessage = new ChatMessage();
                    emptyMessage.ChannelId = chatChannel.ChannelId;
                    emptyMessage.IsAdmin = isAdmin;
                    chatChannel.pinMessage = emptyMessage;
                    PinMessage(chatChannel.pinMessage);
                    return true;
                }
                //NLogManager.LogInfo("vao day ko 7");

                string filteredMessage = message;
                string tempFilteredMessage = message;
                bool flag = false;
                //khong phai admin thi loc bad link
                if (!isAdmin)
                {
                    _chatfilter.RemoveBadLinks(tempFilteredMessage, out flag);
                }
                //Check thời gian chát theo quy định
                //NLogManager.LogInfo("vao day ko 8");

                if (!isAdmin)
                {
                    if (DateTime.Now != chatUser.LastMessageSentTime)
                    {
                        var lastTime = (DateTime.Now - chatUser.LastMessageSentTime).TotalSeconds;
                        var a = MIN_MESSAGE_TIME_SECOND;
                        if (lastTime < MIN_MESSAGE_TIME_SECOND)
                        {
                            BroadcastMessage(connectionId, string.Format("Còn {0}s mới có thể chat", Math.Round((MIN_MESSAGE_TIME_SECOND - lastTime), 2)));
                            chatUser.LastMessageSentTime = DateTime.Now;
                            return false;
                        }
                    }
                }
                // NLogManager.LogInfo("vao day ko 9");

                if (_chatfilter.CheckBanUsers(chatUser))
                {
                    NLogManager.LogInfo(string.Format(">> {0} ({1}) - Tài khoản đang bị Block! ", chatUser.UserName, chatUser.NickName));
                    BroadcastMessage(connectionId, _chatfilter.ReturnCheckBanUsers(chatUser.UserName));
                    return false;
                }
                // NLogManager.LogInfo("vao day ko 10");

                //nếu nội dung chat có bad links thì block gói tin và ghi log
                if (flag)
                {
                    chatUser.CountBadLinks += 1;
                    filteredMessage = "***";
                    //if (chatUser.CountBadLinks > 5)
                    //{
                    //    _chatfilter.BanUser(chatUser.UserName);
                    //}
                    NLogManager.LogInfo(string.Format("User sent bad link: accountId={0}, username={1} - channelId={2} - content={3} - \n\rAgent: {4}", chatUser.AccountID, chatUser.UserName, channelId, message, chatUser.UserAgent));
                }
                //else
                //{
                //    //khong phai admin thi loc bad word
                //    if (!isAdmin)
                //        filteredMessage = _chatfilter.RemoveBadWords(message, out flag);

                //    NLogManager.LogInfo(flag
                //        ? string.Format(
                //            "User sent bad word: accountId={0}, username={1} - channelId={2} - content={3}",
                //            chatUser.AccountID, chatUser.UserName, channelId, message)
                //        : string.Format("User sent message: accountId={0}, username={1} - channelId={2} - content={3}",
                //            chatUser.AccountID, chatUser.UserName, channelId, message));
                //}

                //khong phai admin thi loc bad word
                if (!isAdmin)
                    filteredMessage = _chatfilter.RemoveBadWords(message);

                //Thay thế từ khóa
                if (!isAdmin)
                    filteredMessage = _chatfilter.ReplaceKeyword(filteredMessage);
                // NLogManager.LogInfo("vao day ko 11");

                //Thêm chat vào DB
                Task.Run(() =>
                {
                    InsertChat(new ChatDB
                    {
                        AccountID = int.Parse(chatUser.AccountID.ToString()),
                        UserName = string.Format("{0}", chatUser.UserName),
                        ChannelID = channelId,
                        NickName = chatUser.NickName,
                        Message = filteredMessage
                    });
                });
                //admin thì text chat màu đỏ
                //ChatMessage chatMessage = new ChatMessage(channelId, !isAdmin ? false : true, chatUser.AccountID, !isAdmin ? chatUser.NickName : string.Format("<span style='color:red; font-weight:bold'>ADMIN ({0})</span>", chatUser.NickName), !isAdmin ? filteredMessage : string.Format("{0}", filteredMessage));
                ChatMessage chatMessage = new ChatMessage(channelId, !isAdmin ? false : true,
                    chatUser.AccountID, !isAdmin ? chatUser.NickName : string.Format("{0}", chatUser.NickName),
                    chatUser.UserName, !isAdmin ? filteredMessage : string.Format("{0}", filteredMessage));
                chatMessage.VipRank = chatUser.VipRank;
                bool canSend;
                //  NLogManager.LogInfo("vao day ko 12");

                canSend = AddMessage(chatChannel, chatUser, chatMessage);

                if (canSend)
                {
                    ClientReceiveMessage(chatMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }

            NLogManager.LogInfo(string.Format(">>Gửi tin nhắn không thành công! "));
            return false;
        }
        public async Task<bool> BotSendMessage(string message, string channelId, int accountID, string UserName)
        {
            try
            {
                if (!string.IsNullOrEmpty(_settings.CloseChatServer))
                {
                    return false;
                }
                ChatChannel chatChannel = GetChannel(channelId);
                if (chatChannel == null)
                {
                    NLogManager.LogMessage(string.Format("BOT Sending message: accountId: {0} - not has channel: {1} - content={2}", accountID, channelId, message));
                    return false;
                }

                string filteredMessage = message;
                string tempFilteredMessage = message;
                //Thêm chat vào DB
                InsertChat(new ChatDB
                {
                    AccountID = accountID,
                    UserName = UserName,
                    ChannelID = channelId,
                    NickName = UserName,
                    Message = filteredMessage
                });
                ChatMessage chatMessage = new ChatMessage(channelId, false, accountID, UserName, UserName, filteredMessage);
                var infoVippoint = await _dataService.GetAsync<dynamic>(_settings.VippointURL + "?accountId=" + accountID);
                try
                {
                    if (infoVippoint != null && infoVippoint.code == "0" && infoVippoint.data != null)
                    {
                        chatMessage.VipRank = infoVippoint.data.levelMonth;
                    }
                }
                catch (Exception)
                {
                }
                bool canSend;
                canSend = AddMessage(chatChannel, chatMessage);

                if (canSend)
                {
                    ClientReceiveMessage(chatMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            NLogManager.LogMessage(string.Format(">>BOT Gửi tin nhắn không thành công! "));
            return false;
        }
        public bool AddMessage(ChatChannel chatChannel, ChatUser chatUser, ChatMessage chatMessage)
        {
            // NLogManager.LogInfo("vao day ko 13");

            if (chatMessage == null)
                return false;
            List<ChatMessage> lst = new List<ChatMessage>();
            DateTime now = DateTime.Now;
            string compareMessage = _chatfilter.CutOff(chatMessage.Content, " ,.-_():;/\\\'\"");
            if (chatUser.LastMessages.Count > 0)
            {
                // NLogManager.LogInfo("vao day ko 14");
                if (DateTime.Compare(chatUser.LastMessageSentTime.AddSeconds(TWO_MESSAGE_DURATION), now) > 0)
                    return false;
                // NLogManager.LogInfo("vao day ko 15");

                if (chatUser.LastMessages.Count > 100)
                {
                    int pos = chatUser.LastMessages.Count - 100;
                    if (chatUser.LastMessages.ElementAt(pos).CreatedDate.AddSeconds(TEN_MESSAGE_DURATION) > now)
                        return false;
                }
                // NLogManager.LogInfo("vao day ko 16");

                lst = new List<ChatMessage>(chatUser.LastMessages.Where(m => m != null &&
                !string.IsNullOrEmpty(m.Content) && _chatfilter.CutOff(m.Content, " ,.-_():;/\\\'\"").Equals(compareMessage) &&
                m.CreatedDate.AddSeconds(GLOBAL_TEN_SAME_MESSAGE_DURATION) > now));
                if (lst.Count > 120)
                    return false;
                //NLogManager.LogInfo("vao day ko 17");

                if (chatUser.LastMessages.Count > 1200)
                {
                    if (Monitor.TryEnter(chatUser.LastMessages, 50000))
                    {
                        chatUser.LastMessages.RemoveRange(0, chatUser.LastMessages.Count - 1200);
                        Monitor.Exit(chatUser.LastMessages);
                    }
                }
            }
            // NLogManager.LogInfo("vao day ko 18");

            chatUser.LastActivity = now;
            chatUser.LastMessageSentTime = now;
            chatUser.LastMessages.Add(chatMessage);

            //trong vòng 300s=5p không được gửi tin nhắn có nội dung giống nhau
            lst = new List<ChatMessage>(chatChannel.LastMessages.Where(m => m != null && !string.IsNullOrEmpty(m.Content)
            && _chatfilter.CutOff(m.Content, " ,.-_():;/\\\'\"").Equals(compareMessage)
            && m.CreatedDate.AddSeconds(DUPLICATE_MESSAGE_DURATION) > now));
            if (lst.Count > 0)
                return false;
            // NLogManager.LogInfo("vao day ko 19");

            if (chatChannel.LastMessages.Count > MAX_MESSAGE_IN_CHANNEL)
            {
                // NLogManager.LogInfo("vao day ko 20");

                if (Monitor.TryEnter(chatChannel.LastMessages, 500))
                {
                    chatChannel.LastMessages.RemoveRange(0, chatChannel.LastMessages.Count - MAX_MESSAGE_IN_CHANNEL);
                    Monitor.Exit(chatChannel.LastMessages);
                }
            }
            //NLogManager.LogInfo("vao day ko 21");

            chatChannel.LastMessages.Add(chatMessage);

            return true;
        }
        public bool AddMessage(ChatChannel chatChannel, ChatMessage chatMessage)
        {

            if (chatMessage == null)
                return false;
            List<ChatMessage> lst = new List<ChatMessage>();
            DateTime now = DateTime.Now;
            string compareMessage = _chatfilter.CutOff(chatMessage.Content, " ,.-_():;/\\\'\"");
            lst = new List<ChatMessage>(chatChannel.LastMessages.Where(m => m != null && !string.IsNullOrEmpty(m.Content)
            && _chatfilter.CutOff(m.Content, " ,.-_():;/\\\'\"").Equals(compareMessage)
            && m.CreatedDate.AddSeconds(DUPLICATE_MESSAGE_DURATION) > now));
            if (lst.Count > 0)
                return false;
            if (chatChannel.LastMessages.Count > MAX_MESSAGE_IN_CHANNEL)
            {
                if (Monitor.TryEnter(chatChannel.LastMessages, 500))
                {
                    chatChannel.LastMessages.RemoveRange(0, chatChannel.LastMessages.Count - MAX_MESSAGE_IN_CHANNEL);
                    Monitor.Exit(chatChannel.LastMessages);
                }
            }
            chatChannel.LastMessages.Add(chatMessage);
            return true;
        }
        public void BroadcastMessage(string connectionId, string message)
        {
            _hubContext.Clients.Client(connectionId).SendAsync("broadcastMessage", message);
        }

        /// <summary>
        /// Đăng ký kênh chat
        /// </summary>
        /// <param name="hubCallerContext"></param>
        /// <param name="channelId"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public async Task<ChatChannel> RegisterChat(string connectionId, string channelId, long accountId, string nickName = "")
        {
            try
            {
                NLogManager.LogInfo(string.Format("User join channel: ChannelId={0} | AccountID: {1} | NickName: {2}", channelId, accountId, nickName));
                await _hubContext.Groups.AddToGroupAsync(connectionId, channelId);
                ChatChannel chatChannel = GetChannel(channelId, true);
                if (accountId < 1)
                {
                    NLogManager.LogInfo(string.Format(">>AccountID < 1: {0} ", accountId));
                    return chatChannel;
                }

                ChatUser chatUser = GetUser(accountId, channelId, true);
                try
                {
                    var infoVippoint = await _dataService.GetAsync<dynamic>(_settings.VippointURL + "?accountId=" + accountId);
                    if (infoVippoint != null && infoVippoint.code == "0" && infoVippoint.data != null)
                    {
                        chatUser.VipRank = infoVippoint.data.levelMonth;
                    }
                }
                catch (Exception)
                {
                }
                chatUser.NickName = nickName;
                chatUser.SetActive(true);
                chatUser.SetChannelConnectionId(channelId, connectionId);
                chatChannel.AddUser(chatUser);
                chatChannel.pinMessage.ChannelId = channelId;
                if (chatChannel.LastMessages == null || chatChannel.LastMessages.Count <= 0)
                {
                    chatChannel.LastMessages = new List<ChatMessage>();
                    var lstChatDB = _sql.LoadListLastMessage(channelId);
                    if (lstChatDB!=null)
                    {
                        lstChatDB.ForEach(c => {
                            chatChannel.LastMessages.Add(new ChatMessage
                            {
                                AccountID = c.AccountID,
                                ChannelId = c.ChannelID,
                                Content = c.Message,
                                NickName = c.NickName,
                                Username = c.UserName,

                            });
                        });
                    }
                }
                ClientAddUserOnline(channelId, chatUser);
                PinMessage(chatChannel.pinMessage);
                return chatChannel;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return null;
        }

        public bool UnregisterChat(long accountId, string channelId)
        {
            try
            {
                if (accountId < 1)
                    return false;

                ChatChannel chatChannel = GetChannel(channelId);
                if (chatChannel != null)
                {
                    ChatUser chatUser = GetUser(accountId);
                    if (chatUser != null)
                    {
                        chatUser.SetActive(false);
                        NLogManager.LogInfo(string.Format("User leave channel: {0}:{1} - ChannelId={2}", chatUser.AccountID, chatUser.UserName, channelId));
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return false;
        }

        public void PinMessage(ChatMessage chatMessage)
        {
            _hubContext.Clients.Group(chatMessage.ChannelId).SendAsync("PinMessage", chatMessage);
        }

        public void ClientReceiveMessage(ChatMessage chatMessage)
        {
            _hubContext.Clients.Group(chatMessage.ChannelId).SendAsync("receiveMessage", chatMessage);
        }

        //public void ClientSystemMessage(string channelId, string message, int type)
        //{
        //    _hubContext.Clients.Group(channelId).SendAsync("systemMessage", message, type);
        //}

        //public void ClientListLastMessages(HubCallerContext hubCallerContext, string channelId)
        //{
        //    ChatChannel chatChannel = GetChannel(channelId);
        //    ClientListLastMessages(hubCallerContext, chatChannel);
        //}

        //public void ClientListLastMessages(HubCallerContext hubCallerContext, ChatChannel chatChannel)
        //{
        //    NLogManager.LogInfo("ClientListLastMessages : " + chatChannel);

        //    if (chatChannel != null)
        //    {
        //        _hubContext.Clients.Client(hubCallerContext.ConnectionId).listLastMessages(chatChannel.LastMessages.ToArray());
        //    }
        //}

        //public void ClientListUserOnlines(HubCallerContext hubCallerContext, string channelId)
        //{
        //    ChatChannel chatChannel = GetChannel(channelId);
        //    ClientListUserOnlines(hubCallerContext, chatChannel);
        //}

        //public void ClientListUserOnlines(HubCallerContext hubCallerContext, ChatChannel chatChannel)
        //{
        //    if (chatChannel != null)
        //    {
        //        HubContext.Clients.Client(hubCallerContext.ConnectionId).listUserOnlines(JsonConvert.SerializeObject(chatChannel.UserOnlines.Values.ToArray()));
        //    }
        //}

        public void ClientAddUserOnline(string channelId, ChatUser chatUser)
        {
            //send message new user join chat to other users in channel
            _hubContext.Clients.Group(channelId + "_admin").SendAsync("addUserOnline", chatUser);
            //send all user in channel message
            //ClientSystemMessage(channelId, string.Format("{0} đã vào game.", chatUser.NickName), 0);
        }

        public async Task ClientRemoveUserOnline(string channelId, ChatUser chatUser)
        {
            //send message user left chat to other users in channel
            await _hubContext.Clients.Group(channelId + "_admin").SendAsync(" removeUserOnline", chatUser);
            //send all user in channel message
            //ClientSystemMessage(channelId, string.Format("{0} đã rời game.", chatUser.NickName), 1);
        }

        public void OnConnected(long accountId)
        {
            if (accountId < 1)
                return;

            if (!Monitor.TryEnter(_lockUser, _lockTime))
                return;

            try
            {
                ChatUser chatUser = GetUser(accountId);
                if (chatUser != null)
                {
                    chatUser.SetActive(true);
                }
            }
            finally
            {
                Monitor.Exit(_lockUser);
            }

            //group theo accountId để gửi message cho toàn bộ các connectionId cùng accountId
            //HubContext.Groups.Add(hubCallerContext.ConnectionId, accountId.ToString());
        }

        public void OnReconnected(long accountId)
        {
            if (accountId < 1)
                return;
            if (!Monitor.TryEnter(_lockUser, _lockTime))
                return;

            try
            {
                ChatUser chatUser = GetUser(accountId);
                if (chatUser != null)
                {
                    chatUser.SetActive(true);
                }
            }
            finally
            {
                Monitor.Exit(_lockUser);
            }

            //group theo accountId để gửi message cho toàn bộ các connectionId cùng accountId
            //HubContext.Groups.Add(hubCallerContext.ConnectionId, accountId.ToString());
        }

        public void OnDisconnected(long accountId)
        {
            if (accountId < 1)
                return;

            try
            {
                ChatUser chatUser = GetUser(accountId);
                if (Monitor.TryEnter(_lockUser, _lockTime))
                {
                    if (chatUser != null)
                    {
                        chatUser.SetActive(false);
                    }
                }
            }
            finally
            {
                Monitor.Exit(_lockUser);
            }

            //group theo accountId để gửi message cho toàn bộ các connectionId cùng accountId
            //HubContext.Groups.Remove(hubCallerContext.ConnectionId, accountId.ToString());
        }

        public ChatChannel GetChannel(string channelId, bool AutoCreate = false)
        {
            if (!Monitor.TryEnter(_lockChannel, _lockTime)) return null;
            try
            {
                ChatChannel chatChannel = null;
                if (AutoCreate)
                {
                    chatChannel = Channels.GetOrAdd(channelId, _channel => new ChatChannel(channelId));
                    NLogManager.LogInfo(string.Format("Create channel chat: {0}", channelId));
                    return chatChannel;
                }
                Channels.TryGetValue(channelId, out chatChannel);
                return chatChannel;
            }
            finally
            {
                Monitor.Exit(_lockChannel);
            }
        }

        public ChatUser GetUser(long accountId, string channelId = "all_world", bool AutoCreate = false)
        {
            if (!Monitor.TryEnter(_lockUser, _lockTime)) return null;

            try
            {
                ChatUser chatUser = null;
                if (AutoCreate)
                {
                    string username = _accountSession.AccountName;
                    chatUser = UserOnlines.GetOrAdd(accountId, _user => new ChatUser(accountId, username, "", channelId));

                    NLogManager.LogInfo(string.Format("User join chat: {0} : {1}", chatUser.AccountID, chatUser.UserName));

                    return chatUser;
                }

                UserOnlines.TryGetValue(accountId, out chatUser);
                return chatUser;
            }
            finally
            {
                Monitor.Exit(_lockUser);
            }
        }

        public List<ChatUser> GetAllUserChanel(string channelId = "all_world")
        {
            var info = new List<ChatUser>();
            try
            {
                foreach (var userOnline in UserOnlines)
                {
                    var userInfo = userOnline.Value;
                    info.Add(userInfo);
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return info;
        }

        private void RemoveInactive(object obj)
        {
            try
            {
                //Xóa các user inactive trong 10p khỏi bộ nhớ hệ thống
                if (UserOnlines != null && UserOnlines.Count > 0)
                {
                    //Những user có kênh chat inactive trong 10p
                    DateTime now = DateTime.Now;
                    ConcurrentDictionary<long, ChatUser> list = new ConcurrentDictionary<long, ChatUser>(UserOnlines.Where(_ => _.Value != null && _.Value.LastActivity.AddSeconds(MAX_IDLE_USER_ONLINE) < now));//((!_.Value.IsActive && _.Value.LastMessageSentTime.AddSeconds(TIME_INTERVAL * 3) < now) || _.Value.LastActivity.AddSeconds(MAX_IDLE_USER_ONLINE) < now)));
                    if (list != null && list.Count > 0)
                    {
                        foreach (ChatUser chatUser in list.Values)
                        {
                            foreach (string channelId in chatUser.ChannelConnectionIds.Keys)
                            {
                                if (chatUser.RemoveChannelConnectionId(channelId, out string oldConnectionId))
                                    _hubContext.Groups.RemoveFromGroupAsync(oldConnectionId, channelId);

                                ChatChannel chatChannel = GetChannel(channelId);
                                if (chatChannel != null)
                                {
                                    NLogManager.LogInfo(string.Format("ChatChannel --> RemoveUser: {0} - {1}:{2}", channelId, chatUser.AccountID, chatUser.UserName));
                                    var userRemove = chatChannel.RemoveUser(chatUser.AccountID);
                                    if (userRemove != null)
                                    {
                                        ClientRemoveUserOnline(chatChannel.ChannelId, userRemove);
                                    }
                                }
                            }

                            if (chatUser.ChannelConnectionIds.Keys.Count == 0)
                            {
                                if (Monitor.TryEnter(_lockUser, _lockTime))
                                {
                                    try
                                    {
                                        UserOnlines.TryRemove(chatUser.AccountID, out ChatUser outChatUser);

                                        if (outChatUser != null)
                                        {
                                            ConcurrentDictionary<string, long> listConn = new ConcurrentDictionary<string, long>(ConnectionIdAccountId.Where(__ => __.Value == chatUser.AccountID));
                                            if (listConn != null && listConn.Count > 0)
                                            {
                                                foreach (string connId in listConn.Keys)
                                                    ConnectionIdAccountId.TryRemove(connId, out long accId);
                                            }

                                            NLogManager.LogInfo(string.Format("InActive User: {0} - {1}", outChatUser.AccountID, outChatUser.UserName));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        NLogManager.LogException(e);
                                    }
                                    finally
                                    {
                                        Monitor.Exit(_lockUser);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                NLogManager.LogInfo("Inactive 1");
                NLogManager.LogException(e);
            }

            try
            {
                //xóa các kênh chat không hoạt động
                if (Channels != null && Channels.Count > 0)
                {
                    ConcurrentDictionary<string, ChatChannel> list = new ConcurrentDictionary<string, ChatChannel>(Channels.Where(_ => _.Value != null && _.Value.UserOnlines.Count == 0));
                    if (list != null && list.Count > 0)
                    {
                        foreach (ChatChannel chatChannel in list.Values)
                        {
                            if (Monitor.TryEnter(_lockChannel, _lockTime))
                            {
                                try
                                {
                                    Channels.TryRemove(chatChannel.ChannelId, out ChatChannel outChatChannel);
                                    NLogManager.LogInfo(string.Format("Remove ChatChannel: {0}", chatChannel.ChannelId));
                                }
                                catch (Exception e)
                                {
                                    NLogManager.LogException(e);
                                }
                                finally
                                {
                                    Monitor.Exit(_lockChannel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                NLogManager.LogInfo("Inactive 2");
                NLogManager.LogException(e);
            }
        }

        public void ClearMessage(string channelId)
        {
            try
            {
                ChatChannel chatChannel = null;
                chatChannel = Channels.GetOrAdd(channelId, _channel => new ChatChannel(channelId));
                if (chatChannel != null)
                {
                    chatChannel.LastMessages.Clear();
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogInfo(">> Ex ClearMessage:" + ex.Message);
            }
        }

        private void InsertChat(object state)
        {
            var data = (ChatDB)state;

            _sql.SP_Chat_Insert(data.AccountID, data.UserName, data.NickName, data.ChannelID, data.Message);
        }
    }
}