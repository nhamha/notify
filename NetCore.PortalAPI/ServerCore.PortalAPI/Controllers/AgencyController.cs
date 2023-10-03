using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ServerCore.Utilities.Utils;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Captcha;
using ServerCore.Utilities.Models;
using ServerCore.DataAccess.DAO;
using ServerCore.PortalAPI.Models;
using ServerCore.PortalAPI.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace PortalAPI.Controllers
{
    [Route("Agency")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly IAgencyDAO _agencyDAO;
        private readonly AccountSession _accountSession;
        private static string _loginActionName;
        private static int _loginFailAllow;
        private static int _loginFailTime;
        private static int _maxLengthUserName;
        private static int _maxLengthNickName;
        private readonly AppSettings _appSettings;
        private IAuthenticateService _authenticateService;
        private readonly Captcha _captcha;

        public AgencyController(AccountSession accountSession, IAgencyDAO agencyDAO, IAuthenticateService authenticateService, IOptions<AppSettings> options, Captcha captchaUtil)
        {
            this._accountSession = accountSession;
            this._agencyDAO = agencyDAO;

            _appSettings = options.Value;
            _authenticateService = authenticateService;

            _maxLengthUserName = _appSettings.MaxLengthUserName;
            _maxLengthNickName = _appSettings.MaxLengthNickName;
            _loginFailAllow = _appSettings.LoginFailAllow;
            _loginFailTime = _appSettings.LoginFailTime;
            _loginActionName = _appSettings.LoginActionName;
            _captcha = captchaUtil;

        }
        /// <summary>
        /// Đăng nhập vào cổng đại lý
        /// </summary>
        /// <param name="loginAccount"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public ActionResult<ResponseBuilder> Login(LoginAccount loginAccount)
        {
            string userName = loginAccount.UserName;
            string password = loginAccount.Password;
            string ipaddress = loginAccount.IpAddress;
            string uiid = loginAccount.Uiid;
            int platformId = loginAccount.PlatformId;
            int merchantId = loginAccount.MerchantId;

            string captcha = loginAccount.CaptchaText;
            string captchaToken = loginAccount.CaptchaToken;

            string lng = Utils.GetLanguage(Request.HttpContext);

            try
            {
                if (platformId < (int)PlatformDef.ANDROID_PLATFORM || platformId > (int)PlatformDef.WEB_PLATFORM)
                {
                    return new ResponseBuilder(ErrorCodes.PLATFORM_INVALID, lng);
                }

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                {
                    NLogManager.Error("Tên tài khoản không hợp lệ");
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NAME_INVALID, lng);
                }

                if (userName.Length > _maxLengthUserName)
                {
                    NLogManager.Error("Tên tài khoản không hợp lệ");
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NAME_INVALID, lng);
                }

                AccountInfo accountInfo = null;
                int countCache = CacheCounter.CheckAccountActionFrequency(userName, _loginFailTime, _loginActionName);
                if (countCache >= _loginFailAllow)
                {
                    if (_captcha.VerifyCaptcha(captcha, captchaToken) < 0)
                    {
                        accountInfo = new AccountInfo();
                        accountInfo.IsCaptcha = true;
                        return new ResponseBuilder(ErrorCodes.CAPTCHA_INVALID, lng, null);
                    }
                }

                LoginAccount account = loginAccount;
                account.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                int response = _authenticateService.Login(account, out accountInfo);
                if (response == (int)ErrorCodes.SUCCESS || response == (int)ErrorCodes.NEED_OTP_CODE)
                {
                    return new ResponseBuilder(response, lng, accountInfo);
                }
                else
                {
                    if (response == (int)ErrorCodes.ACCOUNT_NOT_EXIST || response == (int)ErrorCodes.PASSWORD_INVALID)
                        return new ResponseBuilder(ErrorCodes.LOGIN_ERROR, lng);

                    return new ResponseBuilder(response, lng);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.LOGIN_ERROR, lng);
            }
        }
        [Authorize]
        [HttpGet("GetAgencies")]
        public ActionResult<ResponseBuilder> GetAgencies()
        {
            if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

            List<Agency> listTemp = _agencyDAO.GetAgencies();
            //NLogManager.Info(string.Format("UpdateSecurity: accountId = {0}, mobile = {1}, passport = {2}, result = {3}", accountId, mobile, passport, result));
            //string agencyBlackList = ConfigurationManager.AppSettings["AgencyBlackList"];
            List<string> agencyBlackList = null; // CacheCounter.GetAgencyBlackList();
            if(agencyBlackList != null && agencyBlackList.Count > 0 && listTemp.Count > 0)
            {
                foreach(string itbl in agencyBlackList)
                {
                    foreach(Agency it in listTemp)
                    {
                        it.NickFacebook = "";
                        it.AgencyAddress = "";
                        //NLogManager.Info("itbl: " + itbl + ",it: " + it.UserName);
                        if(it.UserName.CompareTo(itbl) == 0)
                        {
                            listTemp.Remove(it);
                            break;
                        }
                    }
                }
            }
            //list.Sort((x, y) => x.Position.CompareTo(y.Position));
            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, listTemp);
        }
        [Authorize]
        [HttpGet("GetAgenciesClient")]
        public ActionResult<ResponseBuilder> GetAgenciesClient(int locationId)
        {
            if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

            List<Agency> listTemp = _agencyDAO.GetAgenciesClient(locationId);
            //NLogManager.Info(string.Format("UpdateSecurity: accountId = {0}, mobile = {1}, passport = {2}, result = {3}", accountId, mobile, passport, result));
            //string agencyBlackList = ConfigurationManager.AppSettings["AgencyBlackList"];
            List<string> agencyBlackList = null; // CacheCounter.GetAgencyBlackList();
            if (agencyBlackList != null && agencyBlackList.Count > 0 && listTemp.Count > 0)
            {
                foreach (string itbl in agencyBlackList)
                {
                    foreach (Agency it in listTemp)
                    {
                        it.NickFacebook = "";
                        it.AgencyAddress = "";
                        //NLogManager.Info("itbl: " + itbl + ",it: " + it.UserName);
                        if (it.UserName.CompareTo(itbl) == 0)
                        {
                            listTemp.Remove(it);
                            break;
                        }
                    }
                }
            }
            //list.Sort((x, y) => x.Position.CompareTo(y.Position));
            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, listTemp);
        }
    }
}