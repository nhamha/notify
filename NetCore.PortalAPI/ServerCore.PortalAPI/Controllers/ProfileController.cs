using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;

namespace PortalAPI.Controllers
{
    [Route("Profile")]
    public class ProfileController : ControllerBase
    {
       
        private readonly AccountSession _accountSession;

        public ProfileController(AccountSession accountSession)
        {
            this._accountSession = accountSession;
        }
    }
}