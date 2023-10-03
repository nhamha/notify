using Newtonsoft.Json;
using PortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using ServerCore.PortalAPI.OTP;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Utils;
using ServerCore.Utilities.Security;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Models;
using PortalAPI.Services;
using Microsoft.AspNetCore.Authorization;
using ServerCore.PortalAPI.Models;
using Microsoft.Extensions.Options;
using ServerCore.Utilities.Captcha;
using ServerCore.PortalAPI.Services;

namespace PortalAPI.Controllers
{
    [Route("Event")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventDAO _iEventDAO;
        private readonly AppSettings _appSettings;
        private readonly AccountSession _accountSession;


        public EventController( AccountSession accountSession, IEventDAO eventDAO, IOptions<AppSettings> options)
        {
            this._accountSession = accountSession;
            this._iEventDAO = eventDAO;
            _appSettings = options.Value;
        }
    
        [Authorize]
        [HttpPost("EventX2")]
        public ActionResult<ResponseBuilder> EventX2([FromBody]dynamic data)
        {
            try
            {
                if (_accountSession.AccountID < 1)
                    return new ResponseBuilder(ErrorCodes.TOKEN_ERROR, _accountSession.Language);

                List<EventX2> lsEvent =  _iEventDAO.GetEvent(_accountSession.AccountID);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, lsEvent);
            }
            catch (Exception ex)
            {
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }
    
    }
}
