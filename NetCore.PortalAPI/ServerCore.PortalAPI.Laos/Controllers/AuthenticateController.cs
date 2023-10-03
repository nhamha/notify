using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerCore.PortalAPI.Models;
using ServerCore.PortalAPI.Services;
using ServerCore.Utilities.Captcha;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Security;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Utils;

namespace ServerCore.PortalAPI.Controllers
{
    [Route("Authen")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        //private IHttpContextAccessor _httpContextAccessor;
        private IHostingEnvironment _env;
        private IAuthenticateService _authenticateService;

        private static int _maxLengthUserName;
        private static int _maxLengthNickName;

        private static string _loginActionName;
        private static int _loginFailAllow;
        private static int _loginFailTime;
        private readonly AppSettings _appSettings;

        private readonly AccountSession _accountSession;
        private readonly Captcha _captcha;

        public AuthenticateController(AccountSession accountSession, IHostingEnvironment env, IAuthenticateService authenticateService, IOptions<AppSettings> options, Captcha captcha)
        {
            _appSettings = options.Value;
            _env = env;
            _authenticateService = authenticateService;

            _maxLengthUserName = _appSettings.MaxLengthUserName;
            _maxLengthNickName = _appSettings.MaxLengthNickName;

            _loginFailAllow = _appSettings.LoginFailAllow;
            _loginFailTime = _appSettings.LoginFailTime;
            _loginActionName = _appSettings.LoginActionName;

            this._accountSession = accountSession;
            _captcha = captcha;
        }

        [HttpGet("RefreshToken")]
        public ActionResult<ResponseBuilder> RefreshToken()
        {
            string lng = Utils.GetLanguage(Request.HttpContext);
            long accountID = _accountSession.AccountID;
            if (accountID <=0)
            {
                return new ResponseBuilder((int)ErrorCodes.TOKEN_ERROR, lng);
            }
            string userName = _accountSession.AccountName;

            try
            {
                if (userName.Length > _maxLengthUserName)
                {
                    NLogManager.Error("Tên tài khoản không hợp lệ");
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NAME_INVALID, lng);
                }

                AccountInfo accountInfo = null;
                int countCache = CacheCounter.CheckAccountActionFrequency(userName, _loginFailTime, _loginActionName);
                if (countCache >= _loginFailAllow)
                {
                    return new ResponseBuilder(ErrorCodes.TOKEN_ERROR, lng);
                }
                int responseStatus = _authenticateService.GetInfo(accountID, userName, out accountInfo);
                if (accountInfo == null || accountInfo.AccountID < 1)
                {
                    NLogManager.Error("Tài khoản không tồn tại");
                    return new ResponseBuilder((int)ErrorCodes.TOKEN_ERROR, lng, null);
                }
           
                if (responseStatus == (int)ErrorCodes.SUCCESS)
                    return new ResponseBuilder((int)ErrorCodes.SUCCESS, lng, accountInfo);
                else
                    return new ResponseBuilder((int)ErrorCodes.TOKEN_ERROR, lng, null);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder((int)ErrorCodes.TOKEN_ERROR, lng, null);
            }
        }

        [HttpPost("Login")]
        public ActionResult<ResponseBuilder> Login(LoginAccount loginAccount)
        {
            //var watch = Stopwatch.StartNew();
            string userName = loginAccount.UserName;
            string password = loginAccount.Password;
            string ipaddress = loginAccount.IpAddress;
            string uiid = loginAccount.Uiid;
            int platformId = loginAccount.PlatformId;
            int merchantId = loginAccount.MerchantId;

            string captcha = loginAccount.CaptchaText;
            string captchaToken = loginAccount.CaptchaToken;
            NLogManager.Info("Login");
            string lng = Utils.GetLanguage(Request.HttpContext);
            NLogManager.Info("Login2");
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

                // Lowercase userName khi đăng nhập;
                loginAccount.UserName = loginAccount.UserName.ToLower();

                AccountInfo accountInfo = null;
                int countCache = CacheCounter.CheckAccountActionFrequency(userName, _loginFailTime, _loginActionName);
                if (countCache >= _loginFailAllow)
                {
                    if (_captcha.VerifyCaptcha(captcha, captchaToken) < 0)
                    {
                        accountInfo = new AccountInfo();
                        accountInfo.IsCaptcha = true;
                        return new ResponseBuilder(ErrorCodes.CAPTCHA_INVALID, lng, accountInfo);
                    }
                }

                LoginAccount account = loginAccount;
                account.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                int response = _authenticateService.Login(account, out accountInfo);

                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("LOGIN: {0}", elapsedMs));

                if (response == (int)ErrorCodes.SUCCESS || response == (int)ErrorCodes.NEED_OTP_CODE)
                {
                    return new ResponseBuilder(response, lng, accountInfo);
                }
                else
                {
                     if (response < 0)
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

        [HttpPost("LoginFacebook")]
        public async Task<ActionResult<ResponseBuilder>> LoginFacebook(LoginAccount loginAccount)
        {
            var watch = Stopwatch.StartNew();
            string lng = Utils.GetLanguage(Request.HttpContext);
            try
            {
                if (loginAccount == null)
                    return new ResponseBuilder(ErrorCodes.DATA_IS_NULL, lng);

                //AccountInfo accountInfo = null;
                //int response = _authenticateService.LoginFBAsync(loginAccount, out accountInfo);
                AccountInfo accountInfo = await _authenticateService.LoginFBAsync(loginAccount);

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                NLogManager.Info(string.Format("TOTALTIME FB: {0}", elapsedMs));

                if (accountInfo.ErrorCode == (int)ErrorCodes.SUCCESS)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, lng, accountInfo);
                }

                if (accountInfo.ErrorCode == (int)ErrorCodes.NEED_OTP_CODE)
                    return new ResponseBuilder(ErrorCodes.NEED_OTP_CODE, lng, accountInfo);
                return new ResponseBuilder(accountInfo.ErrorCode, lng);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.LOGIN_ERROR, lng);
            }
        }

        [HttpPost("Register")]
        public ActionResult<ResponseBuilder> Register(LoginAccount loginAccount)
        {
            string lng = Utils.GetLanguage(Request.HttpContext);
            
            try
            {
                if (loginAccount == null)
                    return new ResponseBuilder(ErrorCodes.DATA_IS_NULL, lng);

                if(_captcha.VerifyCaptcha(loginAccount.CaptchaText, loginAccount.CaptchaToken) != (int)ErrorCodes.SUCCESS)
                    return new ResponseBuilder(ErrorCodes.VERIFY_CODE_INVALID, lng);

                loginAccount.UserName = loginAccount.UserName.ToLower();
                loginAccount.NickName = loginAccount.UserName;

                if (!PolicyUtil.CheckUserName(loginAccount.UserName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NAME_INVALID, lng);

                if (!PolicyUtil.CheckPassword(loginAccount.Password))
                    return new ResponseBuilder(ErrorCodes.PASSWORD_INVALID, lng);

                loginAccount.NickName = loginAccount.UserName;
                if (loginAccount.NickName != null && loginAccount.NickName.Length > 0)
                {
                    if (!PolicyUtil.CheckNickName(loginAccount.NickName))
                        return new ResponseBuilder(ErrorCodes.NICKNAME_INVALID, lng);

                    //if (loginAccount.NickName.ToLower().Equals(loginAccount.UserName.ToLower()))
                    //    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_MUST_DEFFENT_ACCNAME, lng);
                }
                string checkMobile = loginAccount.UserName;
                checkMobile = loginAccount.UserName.Replace("+", "");
                if (checkMobile.All(char.IsDigit))
                {
                   if (_authenticateService.CheckUserNameIsMobileOther(checkMobile))
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_CURRENT_IS_MOBILE_ACTIVE_ACCOUNT_OTHER, lng);
                }

               // string ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                string ipAddress = IPAddressHelper.GetRemoteIPAddress(Request.HttpContext, true);
                LoginAccount account = loginAccount;
                account.IpAddress = ipAddress;
                AccountInfo accountInfo = null;
                int response = _authenticateService.Register(account, out accountInfo);

                //if (response == -105)
                //    return new ResponseBuilder(ErrorCodes.ACCOUNT_EXIST, lng);
                //else if (response == -106)
                //    return new ResponseBuilder(ErrorCodes.NICKNAME_EXIST, lng);

                if (accountInfo != null)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, lng, accountInfo);

                return new ResponseBuilder(response, lng);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.REGISTER_ACCOUNT_ERROR, lng);
            }
        }

        [Authorize]
        [HttpGet("Validate")]
        public ActionResult<ResponseBuilder> Validate(string token)
        {
            string lng = Utils.GetLanguage(Request.HttpContext);
            try
            {
                //var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                //if(authHeader != null && !string.IsNullOrEmpty(authHeader.Parameter))
                AccountInfo accountInfo = null;
                int response = _authenticateService.Validate(token, out accountInfo);
                return new ResponseBuilder(response, lng, accountInfo);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.VALIDATE_ERROR, lng);
        }
    }
}
