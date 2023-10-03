using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Netcore.Chat.Controllers;
using Netcore.Chat.Models;
using NetCore.Utils.Log;
using NetCore.Utils.Sessions;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Netcore.Chat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatController _chatController;
        private readonly AccountSession _accountSession;
        private readonly AppSettings _settings;

        public ChatHub(ChatController chatController, IOptions<AppSettings> options,
            AccountSession accountSession)
        {
            _accountSession = accountSession;
            _settings = options.Value;
            _chatController = chatController;
            // Context.User.GetUserId();
        }

        [HubMethodName("PingPong")]
        public void PingPong()
        {
            _chatController.PingPong(_accountSession.AccountID);
        }

        [HubMethodName("SendMessage")]
        public bool SendMessage(string message, string channelId)
        {
            NLogManager.LogInfo(string.Format("message: {0}, channelId: {1}", message, channelId));
            NLogManager.LogInfo(channelId);
            if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(message))
                return false;
            return _chatController.SendMessage(Context.ConnectionId, _accountSession.AccountID, message, channelId);
        }

        [HubMethodName("RegisterChat")]
        public async Task<bool> RegisterChat(string channelId)
        {
            try
            {
                NLogManager.LogInfo("channelId: " + channelId);
                if (string.IsNullOrEmpty(channelId))
                    return false;

                var chatChannel = await _chatController.RegisterChat(Context.ConnectionId, channelId, _accountSession.AccountID, _accountSession.NickName);
                await Clients.Caller.SendAsync("listUserOnlines", JsonConvert.SerializeObject(chatChannel.UserOnlines.Values.ToArray()));
                await Clients.Caller.SendAsync("listLastMessages", chatChannel.LastMessages.OrderByDescending(c => c.CreatedDate).Take(20).ToArray());
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
                return false;
            }
        }

        public void RegisterAdmin(string channelId)
        {
            Groups.AddToGroupAsync(Context.ConnectionId, channelId + "_admin");
        }

        [HubMethodName("UnregisterChat")]
        public bool UnregisterChat(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                return false;
            Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);

            return _chatController.UnregisterChat(_accountSession.AccountID, channelId);
        }

        public override Task OnConnectedAsync()
        {
            _chatController.OnConnected(_accountSession.AccountID);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _chatController.OnDisconnected(_accountSession.AccountID);
            return base.OnDisconnectedAsync(exception);
        }
    }
}