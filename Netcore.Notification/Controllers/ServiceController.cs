using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netcore.Notification.DataAccess;
using Netcore.Notification.Models;
using NetCore.Utils.Sessions;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace Netcore.Notification.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly ConnectionHandler _connection;
        private readonly PlayerHandler _playerHandler;
        private readonly AccountSession _accountSession;
        private readonly AppSettings _settings;
        private readonly NotificationHandler _notificationHandler;
        private readonly XJackpotController _xJackpotController;
        private readonly EventController _eventController;
        private readonly SQLAccess _sql;
        private readonly JobEventAccess _jobEventAccess;

        public ServiceController(ILogger<ServiceController> logger, ConnectionHandler connection,
            IOptions<AppSettings> options, XJackpotController xJackpotController, EventController eventController,
          AccountSession accountSession, NotificationHandler notificationHandler, PlayerHandler playerHandler, SQLAccess sql,
          JobEventAccess jobEventAccess)
        {
            _playerHandler = playerHandler;
            _connection = connection;
            _logger = logger;
            _accountSession = accountSession;
            _notificationHandler = notificationHandler;
            _settings = options.Value;
            _xJackpotController = xJackpotController;
            _eventController = eventController;
            _sql = sql;
            _jobEventAccess = jobEventAccess;

    }

        [HttpGet]
        [Route("DeleteManyUserMail")]
        public ActionResult DeleteManyUserMail(string ids)
        {
            var idArray = ids.Split(",");
            if (idArray.Length <= 0) return NotFound();
            var lstId = new List<int>();
            for (int i = 0; i < idArray.Length; i++)
            {
                var id = -1;
                if (int.TryParse(idArray[i], out id) && id > 0)
                {
                    _notificationHandler.DeleteUserNotify(id, 10189, "");
                }
            }
            return Ok();
        }

        // GET api/values
        [HttpGet]
        [Route("SendPopup")]
        public ActionResult SendPopup(int accountId, string content, long balance = -1, int type = 1)
        {
            _notificationHandler.SendPopupNoti(accountId, content, balance, type);
            return Ok(200);
        }

        [HttpGet]
        [Route("UserShareProfit")]
        public ActionResult UserShareProfit(string nickName, long prizeValue)
        {
            _notificationHandler.UserShareProfit(nickName, prizeValue);
            return Ok(200);
        }

        [HttpGet]
        [Route("Test")]
        public ActionResult Test()
        {
            long balance = 0;
            var response = _sql.FootbalBetPlayer(1, "test01", 1, 100, ref balance);
            return Ok(1);
        }

        [HttpGet]
        [Route("TestQuest")]
        public ActionResult TestQuest()
        {
            var res = _jobEventAccess.QuestGetPrizeByAccount(1);
            return Ok(res);
        }
        
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}