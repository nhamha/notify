using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PortalAPI.Models;
using PortalAPI.Services;
using ServerCore.DataAccess.DAO;
using ServerCore.PortalAPI.Models;
using ServerCore.PortalAPI.OTP;
using ServerCore.Utilities.Captcha;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Utils;

namespace PortalAPI.Controllers
{
    [Route("Loyalty")]
    public class LoyaltyAPIController : ControllerBase
    {
        private readonly AccountSession _accountSession;
        private readonly OTPSecurity _otp;
        private readonly CoreAPI _coreAPI;
        private readonly AppSettings _appSettings;
        private readonly IGameTransactionDAO _coreDAO;


        public LoyaltyAPIController(AccountSession accountSession, OTPSecurity otp, CoreAPI coreAPI, IOptions<AppSettings> options, IGameTransactionDAO coreDAO)
        {
            this._accountSession = accountSession;
            this._otp = otp;
            this._coreAPI = coreAPI;
            this._appSettings = options.Value;
            this._coreDAO = coreDAO;
        }

        [Authorize]
        [HttpGet("getVPLevel")]
        public ActionResult<ResponseBuilder> getVPLevel(int type)
        {
            try
            {
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, _coreAPI.GetVpLevel(type));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.COIN_FREEZE_FAILED, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("getVPInfo")]
        public ActionResult<ResponseBuilder> getVPInfo(long accountid)
        {
            try
            {
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, _coreAPI.FindVipPointInfo(accountid));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.COIN_FREEZE_FAILED, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("getTopReward")]
        public ActionResult<ResponseBuilder> FindTopAccountVpTransaction(long accountid, int top)
        {
            if (top > 100)
                return new ResponseBuilder(ErrorCodes.COIN_FREEZE_FAILED, _accountSession.Language);
            try
            {
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, _coreAPI.FindTopAccountVpTransaction(accountid, top));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.COIN_FREEZE_FAILED, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("getRankTop")]
        public ActionResult<ResponseBuilder> FindTopRankVipPointLevelInfo(long accountid, int top, int type)
        {
            if (top > 100)
                return new ResponseBuilder(ErrorCodes.COIN_FREEZE_FAILED, _accountSession.Language);
            try
            {
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, _coreAPI.FindTopRankVipPointLevelInfo(accountid, top, type));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.COIN_FREEZE_FAILED, _accountSession.Language);
        }
    }
}