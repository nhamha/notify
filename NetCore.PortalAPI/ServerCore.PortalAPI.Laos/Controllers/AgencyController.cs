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

        public AgencyController(AccountSession accountSession, IAgencyDAO agencyDAO, IAuthenticateService authenticateService, IOptions<AppSettings> options, Captcha captcha)
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
            _captcha = captcha;
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
        [HttpPost("RegisterAgency")]
        private ActionResult<ResponseBuilder> RegisterAgency([FromBody] dynamic data)
        {
            //if (Request.Method == HttpMethod.Options)
            //{
            //    return new ResponseBuilder(ErrorCodes.OK, "Accept HttpOptions", "Accept HttpOptions");
            //}

            try
            {
                if (_accountSession.AccountID < 1 || string.IsNullOrEmpty(_accountSession.AccountName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

                string userName = _accountSession.AccountName;
                string agencyName = data.agencyName;
                string phone = data.agencyPhone;
                string captchaText = data.captchaText;
                string captchaToken = data.captchaToken;

                if (_captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(userName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NAME_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(agencyName))
                    return new ResponseBuilder(ErrorCodes.AGENCY_NAME_INVALID, _accountSession.Language);

                if (phone.Length < 10)
                    return new ResponseBuilder(ErrorCodes.MOBILE_NUMBER_INVALID, _accountSession.Language);

                int res = _agencyDAO.RegisterAgency(userName, agencyName, phone);
                if (res >= 0)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.AGENCY_REGISTER_FAILED, _accountSession.Language);
            }
            return new ResponseBuilder(ErrorCodes.AGENCY_REGISTER_FAILED, _accountSession.Language);
        }

        [HttpPost("EnableAgency")]
        private ActionResult<ResponseBuilder> EnableAgency([FromBody] dynamic data)
        {
            //if (Request.Method == HttpMethod.Options)
            //{
            //    return new ResponseBuilder(ErrorCodes.OK, "Accept HttpOptions", "Accept HttpOptions");
            //}

            try
            {
                if (_accountSession.AccountID < 1 || string.IsNullOrEmpty(_accountSession.AccountName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

                int agencyId = data.agencyId;
                int status = data.status;

                if (agencyId <= 0)
                    return new ResponseBuilder(ErrorCodes.AGENCY_ID_INVALID, _accountSession.Language);

                int res = _agencyDAO.EnableAgency(agencyId, status);
                if (res >= 0)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.AGENCY_REGISTER_FAILED, _accountSession.Language);
            }
            return new ResponseBuilder(ErrorCodes.AGENCY_REGISTER_FAILED, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("GetAgencies")]
        public ActionResult<ResponseBuilder> GetAgencies()
        {
            if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

            List<Agency> listTemp = _agencyDAO.GetAgenciesByAccountID(_accountSession.AccountID);
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

            //List<Agency> list = new List<Agency>();
            //foreach(Agency item in listTemp)
            //{
            //    if (item.Position>=0)
            //    {
            //        list.Add(item);
            //    }
            //}
            //list.Sort((x, y) => x.Position.CompareTo(y.Position));
            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, listTemp);
        }
    }
}