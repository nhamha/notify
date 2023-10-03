using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Netcore.Chat.Models;
using NetCore.Utils.Log;
using NetCore.Utils.Sessions;

namespace Netcore.Chat.Controllers
{
    [Route("[Controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly ChatController _chatController;
        private readonly AccountSession _accountSession;
        private readonly AppSettings _settings;

        public ServiceController(ChatController chatController, IOptions<AppSettings> options,
            AccountSession accountSession)
        {
            _accountSession = accountSession;
            _settings = options.Value;
            _chatController = chatController;
            // Context.User.GetUserId();
        }

        [HttpGet]
        [Route("SendMessage")]
        public int SendMessage(int accountId, string nickname, string channelId, string message)
        {
            NLogManager.LogMessage(string.Format("BOT Sending message: accountId: {0} - not has channel: {1} - content={2}", accountId, channelId, message));
            _chatController.BotSendMessage(message, channelId, accountId, nickname);
            return 1;
        }
        public IActionResult PingPong()
        {
            _chatController.PingPong(1);
            return Ok();
        }
    }
}