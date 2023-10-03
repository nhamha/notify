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
using PortalAPI.Services;
using ServerCore.DataAccess.DAO;
using ServerCore.PortalAPI.Models;
using ServerCore.PortalAPI.OTP;
using ServerCore.PortalAPI.Services;
using ServerCore.Utilities.Captcha;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Security;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Utils;

namespace ServerCore.PortalAPI.Controllers
{
    [Route("api/fish")]
    [ApiController]
    public class FishingController : ControllerBase
    {
        private IHostingEnvironment _env;
        private IAuthenticateService _authenticateService;

        private static int _maxLengthUserName;
        private static int _maxLengthNickName;

        private static string _loginActionName;
        private static int _loginFailAllow;
        private static int _loginFailTime;
        private readonly AppSettings _appSettings;
        private readonly IAccountDAO _accountDAO;
        private readonly CoreAPI _coreAPI;
        private readonly OTPSecurity _otp;
   
        private readonly AccountSession _accountSession;

        public FishingController(AccountSession accountSession, IHostingEnvironment env, IAuthenticateService authenticateService, IOptions<AppSettings> options, IAccountDAO accountDAO, CoreAPI coreAPI, OTPSecurity otp)
        {
            _appSettings = options.Value;
            _otp = otp;
            _env = env;
            _authenticateService = authenticateService;

            _maxLengthUserName = _appSettings.MaxLengthUserName;
            _maxLengthNickName = _appSettings.MaxLengthNickName;

            _loginFailAllow = _appSettings.LoginFailAllow;
            _loginFailTime = _appSettings.LoginFailTime;
            _loginActionName = _appSettings.LoginActionName;

            this._accountSession = accountSession;
            this._accountDAO = accountDAO;
            this._coreAPI = coreAPI;
        }
        [HttpPost("FishHunterTransaction")]
        public  ActionResult<ResponseBuilder> FishHunterTransaction([FromBody] dynamic data)
        {
            string lng = Utils.GetLanguage(Request.HttpContext);
            var dataResult = _coreAPI.FishHunterTransaction(data);
            if (dataResult != null)
                return new ResponseBuilder((int)ErrorCodes.SUCCESS, lng, dataResult);
            else
                return new ResponseBuilder((int)ErrorCodes.INPUT_PARAM_ERROR, lng, dataResult);
        }

        [HttpPost("VerifyToken")]
        public ActionResult<ResponseBuilder> VerifyToken([FromBody] dynamic data)
        {
            string lng = Utils.GetLanguage(Request.HttpContext);
            string sign = data.Sign;
            string token = data.Token;
            string verifyData = Security.MD5Encrypt(token + _appSettings.FishingKey);
            // Verify Data
            if (verifyData.CompareTo(sign) != 0)
            {
                return new ResponseBuilder((int)ErrorCodes.TRANSACTION_FAILED, lng, null);
            }
            // Check Token
            OtpData otpData = _otp.ParseOTPTokenFish(token);
            if (otpData == null)
            {
                return new ResponseBuilder((int)ErrorCodes.TOKEN_ERROR, lng, null);
            }
            // Check datetime token
            long tickNow = DateTime.Now.Ticks;
            TimeSpan diffTime = new TimeSpan(tickNow - otpData.CreateTime);
            //if (diffTime.TotalHours > 12)
            //{
            //    return new ResponseBuilder((int)ErrorCodes.ACCOUNT_OTP_EXPIRED, lng, null);
            //}

            if (otpData.AccountId <= 0)
            {
                return new ResponseBuilder((int)ErrorCodes.ACCOUNT_NOT_EXIST, lng);
            }

            string nickName = otpData.AccountName;
            if (nickName.Length < 1)
            {
                NLogManager.Error("Tên tài khoản không hợp lệ");
                return new ResponseBuilder(ErrorCodes.ACCOUNT_NAME_INVALID, lng);
            }

            try
            {
                AccountInfo accountInfo = null;
                int responseStatus = _authenticateService.GetInfo(otpData.AccountId, "", out accountInfo);
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

        [HttpPost("ChangeMoney")]
        public ActionResult<ResponseBuilder> ChangeMoney([FromBody] dynamic data)
        {
            var watch = Stopwatch.StartNew();
            NLogManager.Info("ChangeMoneyFish:" + JsonConvert.SerializeObject(data));
            string lng = Utils.GetLanguage(Request.HttpContext);
            string sign = data.Sign;
            string token = data.Token;
            int amount =Convert.ToInt32(Math.Floor(Convert.ToDouble(data.Amount)));
            
            int type = data.Type;
            string verifyData = Security.MD5Encrypt(token + _appSettings.FishingKey + amount);
            // Verify Data
            if (verifyData.CompareTo(sign) != 0)
            {
                NLogManager.Info("ChangeMoneyFish1:" + (int)ErrorCodes.VALIDATE_ERROR);
                return new ResponseBuilder((int)ErrorCodes.VALIDATE_ERROR, lng, null);
            }
            // Check Token
            OtpData otpData = _otp.ParseOTPTokenFish(token);
            NLogManager.Info("ChangeMoneyFish:" + JsonConvert.SerializeObject(otpData));
            if (otpData == null)
            {
                NLogManager.Info("ChangeMoneyFish1:" + (int)ErrorCodes.TOKEN_ERROR);
                return new ResponseBuilder((int)ErrorCodes.TOKEN_ERROR, lng, null);
            }
            // Check datetime token
            long tickNow = DateTime.Now.Ticks;
            TimeSpan diffTime = new TimeSpan(tickNow - otpData.CreateTime);
            if (diffTime.TotalHours > 12)
            {
                NLogManager.Info("ChangeMoneyFish1:" + (int)ErrorCodes.ACCOUNT_OTP_EXPIRED);
                return new ResponseBuilder((int)ErrorCodes.ACCOUNT_OTP_EXPIRED, lng, null);
            }

            if (otpData.AccountId <= 0)
            {
                return new ResponseBuilder((int)ErrorCodes.ACCOUNT_NOT_EXIST, lng);
            }

            string nickName = otpData.AccountName;
            if (nickName.Length < 1)
            {
                NLogManager.Error("Tên tài khoản không hợp lệ");
                return new ResponseBuilder(ErrorCodes.ACCOUNT_NAME_INVALID, lng);
            }
            string desc = "Trừ tiền Fishing";
            if (type == 2)
                desc = "Cộng tiền Fishing";
            FishingInfo fishingInfo;
            int responseStatus = _accountDAO.ChangeMoneyFishing(otpData.AccountId, amount, desc, type, otpData.PlatformId, out fishingInfo);

            NLogManager.Info("ChangeMoneyFish:" + responseStatus);
           
            if (responseStatus >=0 )
            {
                var elapsedMs11 = watch.ElapsedMilliseconds;
                NLogManager.Info(string.Format("FISHTIME SendPopup: {0}", elapsedMs11));
               // _coreAPI.SendPopup(otpData.AccountId, "", fishingInfo.balance, -1);

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                NLogManager.Info(string.Format("FISHTIME: {0}", elapsedMs));

                return new ResponseBuilder((int)ErrorCodes.SUCCESS, lng, fishingInfo);
            }
            watch.Stop();
            var elapsedMs1 = watch.ElapsedMilliseconds;
            NLogManager.Info(string.Format("ERROR FISHTIME: {0}", elapsedMs1));

            return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, lng);
        }
    }
}
