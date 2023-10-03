using Newtonsoft.Json;
using GameServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Utils;
using ServerCore.PortalAPI.OTP;
using ServerCore.PortalAPI.Models;
using ServerCore.DataAccess.DTO;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Security;
using PortalAPI.Models;
using ServerCore.Utilities.Models;
using PortalAPI.Services;
using ServerCore.Utilities.Captcha;
using Microsoft.Extensions.Options;
using ServerCore.PortalAPI.Services;

namespace PortalAPI.Controllers
{
    [Route("Account")]
    [ApiController]
    public class AccountAPIController : ControllerBase
    {
        private static readonly string OTPCHECK_SECRET_KEY = "KeyCheckOTP@TT.quay.club)(!@#&*(^@017";

        private readonly AccountSession _accountSession;
        private readonly OTPSecurity _otp;
        private readonly IEventDAO _eventDAO;
        private readonly IAccountDAO _accountDAO;
        private readonly IAgencyDAO _agencyDao;
        private readonly IMobileDAO _mobileDAO;
        private readonly CoreAPI _coreAPI;
        private readonly IOTPDAO _iOTPDAO;
        private readonly AppSettings _appSettings;
        private IAuthenticateService _authenticateService;
        private readonly Captcha _captcha;


        public AccountAPIController(AccountSession accountSession, IAuthenticateService authenticateService, OTPSecurity otp, IAccountDAO accountDAO, IAgencyDAO agencyDAO, IEventDAO eventDAO, IMobileDAO mobileDAO, CoreAPI coreAPI, IOTPDAO iOTPDAO, IOptions<AppSettings> options, Captcha captcha)
        {
            this._appSettings = options.Value;
            this._accountSession = accountSession;
            this._authenticateService = authenticateService;
            this._otp = otp;
            this._eventDAO = eventDAO;
            this._accountDAO = accountDAO;
            this._mobileDAO = mobileDAO;
            this._coreAPI = coreAPI;
            this._iOTPDAO = iOTPDAO;
            this._agencyDao = agencyDAO;
            _captcha = captcha;
        }

        [HttpGet("RefreshToken")]
        public ActionResult<ResponseBuilder> RefreshToken()
        {
            if (_accountSession.AccountID > 0)
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
            else
                return new ResponseBuilder(ErrorCodes.TOKEN_ERROR, _accountSession.Language);
        }

        /// <summary>
        /// Update thông tin tài khoản
        /// </summary>
        /// <param name="data">bỏ accountId</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateAccount")]
        public ActionResult<ResponseBuilder> UpdateAccount([FromBody] dynamic data)
        {
            try
            {
                long accountId = _accountSession.AccountID;
                if (accountId < 1)
                    return new ResponseBuilder(ErrorCodes.TOKEN_ERROR, _accountSession.Language);
                string passport = data.passport;
                string mobile = data.mobile;
                mobile = Utils.FormatMobile(mobile);
                int avatar = data.avatar;

                int type = data.type;
                if (type > 3 || type < 0)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                if (type == 3) // update all
                {
                    if (string.IsNullOrEmpty(mobile) || !PolicyUtil.CheckMobile(mobile))
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_INVALID, _accountSession.Language);

                    if (string.IsNullOrEmpty(passport))
                        return new ResponseBuilder(ErrorCodes.IDENTITY_CODE_INVALID, _accountSession.Language);

                }
                else if (type == 2) // update mobile
                {
                    // check mobile
                    if (string.IsNullOrEmpty(mobile) || !PolicyUtil.CheckMobile(mobile))
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_INVALID, _accountSession.Language);
                }
                else if (type == 1) // update passport
                {
                    if (string.IsNullOrEmpty(passport))
                        return new ResponseBuilder(ErrorCodes.IDENTITY_CODE_INVALID, _accountSession.Language);
                }

                else if (type == 0) // update Avatar
                {
                    if (avatar < 0)
                        return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);
                }

                // Nếu update mobile thì chek OTP: Nếu IsMobileActived true thì không được thay đổi SĐT
                if ((mobile != null && mobile.Length > 0) || type == 3 || type == 2)
                {
                    if (_otp.IsOTP(accountId))
                    {
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language);
                    }
                }

                var result = _accountDAO.UpdateInfo(accountId, "", mobile, passport, avatar);
                NLogManager.Info(string.Format("UpdateSecurity: accountId = {0}, mobile = {1}, passport = {2}, result = {3}", accountId, mobile, passport, result));

                if (result >= 0)
                {
                    var info = _otp.GetSucurityInfoWithMask(accountId);
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
                }
                else if (result == -102)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_ALREADY_USED, _accountSession.Language);
                }
                return new ResponseBuilder(ErrorCodes.ACCOUNT_UPDATE_FAILED, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language);
            }
        }

        /// <summary>
        /// API cho Report
        /// </summary>
        /// <param name="data">bỏ accountid</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateAccountInfo")]
        public ActionResult<ResponseBuilder> UpdateSecurityInfoReport([FromBody] dynamic data)
        {
            try
            {
                long accountId = _accountSession.AccountID;
                string mobile = Utils.FormatMobile(data.mobile);
                string otp = data.otp;
                int otpType = data.otpType; // 1 = SMS OTP, 2 = APP OTP
                string seviceKey = data.seviceKey;

                if (!PolicyUtil.IsServiceKeyOk(seviceKey, accountId))
                    return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);

                // check mobile
                if (string.IsNullOrEmpty(mobile) || !PolicyUtil.CheckMobile(mobile))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_INVALID, _accountSession.Language);

                var otpCheck = _otp.CheckOTP(accountId, _accountSession.AccountName, otp, otpType);
                if (otpCheck == OTPCode.OTP_NEED_INPUT)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language);

                if (otpCheck == OTPCode.OTP_NOT_VERIFY || otpCheck == OTPCode.OTP_VERIFY_ERROR)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                var result = _accountDAO.UpdateInfo(accountId, "", mobile, "", -1);
                NLogManager.Info(string.Format("UpdateSecurity: accountId = {0}, mobile = {1}, passport = {2}, result = {3}", accountId, mobile, "", result));

                if (result >= 0)
                {
                    var info = _otp.GetSucurityInfoWithMask(accountId);
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
                }
                else if (result == -102)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_ALREADY_USED, _accountSession.Language);
                }
                return new ResponseBuilder(ErrorCodes.ACCOUNT_UPDATE_FAILED, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language);
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="data">bỏ AccountID và Mobile</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("ChangePass")]
        public ActionResult<ResponseBuilder> ChangePassword(ChangePasswordData data)
        {

            try
            {
                string oldpass = data.OldPassword;
                string newpass = data.NewPassword;
                int sourceId = data.PlatformId;

                long accountId = _accountSession.AccountID;
                string accountName = _accountSession.AccountName;

                string captchaText = data.captchaText;
                string captchaToken = data.captchaToken;
                NLogManager.Info("VerifyCaptcha:" + captchaText + "|" + captchaToken);
                if (_captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                // Kiểm tra thông tin bảo mật OTP
                //  var securityInfo = _accountDAO.GetSecurityInfo(accountId);
                // Tài khoản chưa đăng ký bảo mật OTP
                // if (securityInfo == null) return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER, _accountSession.Language);

                // Check mobile truyền lên
                // if (securityInfo.Mobile == null || securityInfo.Mobile.Length < 1 || (!securityInfo.IsMobileActived) ) return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                if (oldpass.CompareTo(newpass) == 0)
                    return new ResponseBuilder(ErrorCodes.PASSWORD_NEW_NOT_MATCH, _accountSession.Language);

                if (string.IsNullOrEmpty(oldpass.Trim()) || string.IsNullOrEmpty(newpass.Trim()))
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                AccountEbank account = _accountDAO.Get(_accountSession.AccountID);

                string newPassEnc = Security.GetPasswordEncryp(newpass, accountName);
                string oldPassEnc = Security.GetPasswordEncryp(oldpass, accountName);

                // oldPass check
                if (oldPassEnc.CompareTo(account.Password) != 0)
                    return new ResponseBuilder(ErrorCodes.PASSWORD_OLD_INVALID, _accountSession.Language);

                //if (!System.Text.RegularExpressions.Regex.IsMatch(newpass, @"^(?=.{6,18})(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).*$"))
                //if (newpass.Length > 18 || newpass.Length < 6)
                //    return new ResponseBuilder(ErrorCodes.PASSWORD_NEW_INVALID, _accountSession.Language);

                if (_otp.IsOTP(accountId))
                {
                    string serviceData = JsonConvert.SerializeObject(new ChangePassWordData
                    {
                        AccountID = accountId,
                        AccountName = accountName,
                        NewPassWord = newpass,
                        OldPassWord = oldpass
                    });

                    string token = OTPSecurity.CreateOTPToken(accountName, accountId, (int)OtpServiceCode.OTP_SERVICE_CHANGE_PASSWORD, sourceId, serviceData);
                    return new ResponseBuilder(ErrorCodes.NEED_OTP_CODE, _accountSession.Language, new Models.OTPDataReturn(token));
                }
                else
                {
                    if (account.TotalVcoinPayment > 0)
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER, _accountSession.Language);

                    int response = _accountDAO.ChangePassword(accountId, oldPassEnc, newPassEnc);
                    if (response >= 0)
                        return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);

                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHANGE_PASSWORD_FAILED, _accountSession.Language);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language);
            }
        }

        [Authorize]
        [HttpGet("UpdateEmail")]
        public ActionResult<ResponseBuilder> UpdateEmail(string email)
        {

            try
            {
                //if(string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || Captcha.VerifyCaptcha(captcha, captchaToken) < 0)
                //    return new ResponseBuilder(ErrorCodes.BadRequest, "Mã xác nhận không hợp lệ", "Captcha Error");

                if (_accountSession.AccountID < 1 || string.IsNullOrEmpty(_accountSession.AccountName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

                //if(acountID < 1)
                //    return new ResponseBuilder(ErrorCodes.BadRequest, "Tài khoản không hợp lệ", "Account Error");

                if (string.IsNullOrEmpty(email) || !email.Contains("@") || email.StartsWith("@") || PolicyUtil.CheckEmail(email))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_EMAIL_INVALID, _accountSession.Language);

                //AccountDb accountDB = _accountDAO.GetAccount(acountID, userName);
                //if(accountDB != null)
                //{
                //    if(accountDB.Email.Contains("@"))
                //    {
                //        if(string.IsNullOrEmpty(emailCurrent) || emailCurrent.CompareTo(accountDB.EmailFull) < 0)
                //            return new ResponseBuilder(ErrorCodes.BadRequest, "Email hiện tại không hợp lệ", "Email Error");
                //    }
                //}

                int res = _mobileDAO.UpdateEmail(_accountSession.AccountID, email);
                NLogManager.Info("Account: " + _accountSession.AccountName + ", update email: " + email + ", result: " + res);
                if (res < 0)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_EMAIL_UPDATE_FAILED, _accountSession.Language);
                }
                return new ResponseBuilder(ErrorCodes.ACCOUNT_EMAIL_UPDATE_SUCCESS, _accountSession.Language);
            }
            catch (System.Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language);
            }
        }

        /// <summary>
        /// bỏ AccountID, Mobile, NickName
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("GiftCode")]
        public ActionResult<ResponseBuilder> GiftCode(Models.Giftcode input)
        {
            if (_accountSession.AccountID <= 0)
                return new ResponseBuilder(ErrorCodes.ACCOUNT_CAN_NOT_EXC_FUNCTION, _accountSession.Language);

            if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
                return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "invalid token");

            //int eventid = 1;
            try
            {
                if (_captcha.VerifyCaptcha(input.captcha, input.verifyCaptcha) < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(input.giftCode))
                    return new ResponseBuilder(ErrorCodes.GIFT_CODE_INVALID, _accountSession.Language);

                long balance = -1;
                var ipAddress = _accountSession.IpAddress;
                int res = _coreAPI.CheckGiftCode((int)_accountSession.AccountID, _accountSession.AccountName, _accountSession.NickName, input.giftCode,
                   100011, "123456", input.sourceId, ipAddress, out balance);

                //NLogManager.Info(string.Format("GiftCode={0}, response={1}", JsonConvert.SerializeObject(input), res));

                if (res >= 0)
                {

                    string message = String.Format("Successful: {0}! .at {1}. Wish you happy!", input.giftCode, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                    //_otp.PushMessageToTelegram(_accountSession.AccountID, "", message);

                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(new
                    {
                        Response = res,
                        Balance = balance
                    }));
                }
                else
                {
                    /// ERROR_GIFCODE_USING INT = -100
                    /// ERROR_GIFCODE_NOT_EXIST INT = -101
                    /// ERROR_GIFCODE_NOT_TIME INT = -102
                    /// ERROR_GIFCODE_EXPIRE INT = -103
                    /// ERROR_GIFCODE_LOCK INT = -104
                    /// ERROR_GIFCODE_OVERLIMIT INT = -105
                    /// ERROR_GIFCODE_FAILED_SOURCE INT = -106
                    string content = "GiftCode Invalid";
                    if (res == -100)
                        content = "GiftCode Used.";
                    else if (res == -101)
                        content = "GiftCode not exist.";
                    else if (res == -102)
                        content = "GiftCode not use this time.";
                    else if (res == -103)
                        content = "GiftCode expired.";
                    else if (res == -104)
                        content = "GiftCode locked.";
                    else if (res == -105)
                        content = "You have already used this giftcode.";
                    else if (res == -106)
                        content = "Do not use for this device.";
                    else
                        content = "Unsuccessful.";

                    return new ResponseBuilder(ErrorCodes.GIFT_CODE_INVALID, _accountSession.Language, content);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.GIFT_CODE_INVALID, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("GiftCode")]
        public ActionResult<ResponseBuilder> GiftCode(string giftCode, string captcha, string verifyCaptcha)
        {
            if (_accountSession.AccountID <= 0)
                return new ResponseBuilder(ErrorCodes.ACCOUNT_CAN_NOT_EXC_FUNCTION, _accountSession.Language);

            if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
                return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Invalid token");
            //int eventid = 1;
            try
            {
                if (_captcha.VerifyCaptcha(captcha, verifyCaptcha) < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(giftCode))
                    return new ResponseBuilder(ErrorCodes.GIFT_CODE_INVALID, _accountSession.Language);

                long balance = -1;
                var ipAddress = _accountSession.IpAddress;
                int res = _coreAPI.CheckGiftCode(_accountSession.AccountID, _accountSession.AccountName, _accountSession.NickName, giftCode,
                    100011, "123456", _accountSession.PlatformID, _accountSession.IpAddress, out balance);

                //NLogManager.Info(string.Format("GiftCode={0}, response={1}", JsonConvert.SerializeObject(input), res));

                if (res >= 0)
                {
                    string message = String.Format("Successful: {0}! . At {1}. Wish you happy!", giftCode, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                    _otp.PushMessageToTelegram(_accountSession.AccountID, "", message);

                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(new
                    {
                        Response = res,
                        Balance = balance
                    }));
                }
                else
                {
                    /// ERROR_GIFCODE_USING INT = -100
                    /// ERROR_GIFCODE_NOT_EXIST INT = -101
                    /// ERROR_GIFCODE_NOT_TIME INT = -102
                    /// ERROR_GIFCODE_EXPIRE INT = -103
                    /// ERROR_GIFCODE_LOCK INT = -104
                    /// ERROR_GIFCODE_OVERLIMIT INT = -105
                    /// ERROR_GIFCODE_FAILED_SOURCE INT = -106
                    string content = "GiftCode Invalid";
                    if (res == -100)
                        content = "GiftCode Used.";
                    else if (res == -101)
                        content = "GiftCode not exist.";
                    else if (res == -102)
                        content = "GiftCode not use this time.";
                    else if (res == -103)
                        content = "GiftCode expired.";
                    else if (res == -104)
                        content = "GiftCode locked.";
                    else if (res == -105)
                        content = "You have already used this giftcode.";
                    else if (res == -106)
                        content = "Do not use for this device.";
                    else
                        content = "Unsuccessful.";

                    return new ResponseBuilder(ErrorCodes.GIFT_CODE_INVALID, _accountSession.Language, content);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.GIFT_CODE_INVALID, _accountSession.Language);
        }

        #region Quên mật khẩu

        /// <summary>bigmidu
        ///  Kiểm tra tài khoản tồn tại trên hệ thống chưa
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns>
        /// returnData.ResponseCode>0 là có tồn tại
        /// </returns>
        [Authorize]
        [HttpGet("CheckAccountExists")]
        private ActionResult<ResponseBuilder> CheckAccountExists(int accountId)
        {
            try
            {
                string accountName = _accountSession.AccountName;
                if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(accountName))
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);
                }

                if (accountId <= 0)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                }

                var checkAccountExist = _accountDAO.Get(accountId);
                if (checkAccountExist == null)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                }

                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(new
                {
                    ResponseStatus = 0,
                    NickName = checkAccountExist.Fullname
                }));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
            }
        }
        #region [Đặt lại mật khẩu - Quên mật khẩu]
        /// <summary>
        /// Đặt lại mật khẩu (quên mk)
        /// </summary>
        /// <param name="model">
        /// model.Type: 1--> lấy qua SMS; 2--> lấy qua email
        ///
        /// </param>
        /// <returns>
        /// returnData.ResponseCode = -8004; // dữ liệu hợp lệ, trả về để client tiếp tục chuyển view nhập mã xác thực
        /// returnData.ResponseCode>0 : thành công
        /// </returns>
        [HttpPost("ResetPassword")]
        public ActionResult<ResponseBuilder> ResetPassword(ResetPassModels model)
        {
            try
            {
                // Kiểm tra tài khoản có tồn tại trên hệ thống không

                if (model == null || model.Token == null)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_RESET_PASSWORD_FAILED, _accountSession.Language);
                }
                var otpData = _otp.ParseToken(model.Token);

                // var checkAccountID = _accountDAO.CheckAccountExist(model.AccountName);
                // if (checkAccountID <= 0)
                //    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                // Lấy thông tin account cũ, trước khi Reset Pass
                AccountEbank account = _accountDAO.Get(otpData.AccountId);
                // if (model.Type == 1) // SMS
                //{
                int otpType = model.OtpType;
                string newPass = model.NewPassword;
                string otp = model.Otp;

                if (otpType < 1 || otpType > 2)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_DATA_NOT_MATCH, _accountSession.Language);

                if (string.IsNullOrEmpty(otp))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(newPass))
                    return new ResponseBuilder(ErrorCodes.PASSWORD_NEW_INVALID, _accountSession.Language);

                // Kiểm tra thông tin bảo mật OTP
                var securityInfo = _accountDAO.GetSecurityInfo(otpData.AccountId);

                // Tài khoản chưa đăng ký bảo mật OTP
                if (securityInfo == null) return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER, _accountSession.Language);
                if (securityInfo.Mobile == null || securityInfo.Mobile.Length < 1 || (!securityInfo.IsMobileActived)) return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_INVALID, _accountSession.Language);

                // Xác thực OTP qua API
                if (_otp.VerifyOtpByApi(securityInfo.Mobile, otp, otpType) != 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                string oldPass = account.Password;
                int result_reset = _accountDAO.ResetPassword(account.AccountName, Security.GetPasswordEncryp(newPass, account.AccountName), (short)model.Type);

                if (result_reset < 0)
                {
                    // Cập nhật lại Mật khẩu cũ khi reset failed
                    _accountDAO.ResetPassword(account.AccountName, oldPass, (short)model.Type);
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_RESET_PASSWORD_FAILED, _accountSession.Language);
                }
                NLogManager.Info(string.Format("Account = {0} reset successful", account.AccountName));
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language);
            }
        }

        [HttpPost("ResetPasswordAgency")]
        public ActionResult<ResponseBuilder> ResetPasswordAgency(ResetPassAgencyModels model)
        {
            try
            {
                if (model == null)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_RESET_PASSWORD_FAILED, _accountSession.Language);
                }

                if (string.IsNullOrEmpty(model.Password))
                    return new ResponseBuilder(ErrorCodes.PASSWORD_NEW_INVALID, _accountSession.Language);

                int result_reset = _accountDAO.ResetPasswordAgency(model.AccountIDAgency, model.UserName, Security.GetPasswordEncryp(model.Password, model.UserName));
                if (result_reset >= 0)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
                else
                    return new ResponseBuilder(result_reset, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language);
            }
        }
        /// <summary>
        /// Truyền lên accountName và mobile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("ResetPasswordCheck")]
        public ActionResult<ResponseBuilder> ResetPasswordCheck(dynamic model)
        {
            try
            {
                // Kiểm tra tài khoản có tồn tại trên hệ thống không
                string accountName = model.AccountName;
                string mobile = model.Mobile;
                string captcha = model.captcha;
                string verifyCaptcha = model.verifyCaptcha;

                if (_captcha.VerifyCaptcha(captcha, verifyCaptcha) < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                var accountID = _accountDAO.CheckAccountExist(accountName);
                if (accountID <= 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);

                // Kiểm tra thông tin bảo mật OTP
                var securityInfo = _accountDAO.GetSecurityInfo(accountID);
                // Tài khoản chưa đăng ký bảo mật OTP
                if (securityInfo == null) return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER, _accountSession.Language);
                if (securityInfo.Mobile == null || securityInfo.Mobile.Length < 1 || (!securityInfo.IsMobileActived)) return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_INVALID, _accountSession.Language);

                // Check mobile truyền lên
                mobile = Utils.FormatMobile(mobile);
                if (securityInfo.Mobile != mobile) return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_CURRENT_INVALID, _accountSession.Language);

                //string serviceData = JsonConvert.SerializeObject(account);
                OtpData otpData = new OtpData(accountName, accountID, DateTime.Now.Ticks, (int)OtpServiceCode.OTP_SERVICE_LOGIN, 1, mobile);
                string token = OTPSecurity.CreateOTPToken(otpData);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, token);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
            }
        }
        /// <summary>
        /// Hardrest không cần OTP 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("HardResetPassword")]
        public ActionResult<ResponseBuilder> HardResetPassword(ResetPassModels model)
        {
            try
            {
                // kiểm tra tài khoản có tồn tại trên hệ thống không
                var checkAccountID = _accountDAO.CheckAccountExist(model.AccountName);
                if (checkAccountID <= 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);

                AccountEbank account = _accountDAO.Get(checkAccountID);

                //string content = "";
                if (model.Type == 9) // Hard reset
                {
                    // Hardreset Code
                    if (!"683952".Equals(model.Otp))
                    {
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_RESET_PASSWORD_FAILED, _accountSession.Language);
                    }
                    string oldPass = account.Password;
                    int result_reset = _accountDAO.ResetPassword(account.AccountName, Security.GetPasswordEncryp(model.NewPassword, model.AccountName), (short)model.Type);

                    if (result_reset < 0)
                    {
                        // cap nhat lai mat khau cu
                        _accountDAO.ResetPassword(account.AccountName, oldPass, (short)model.Type);
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_RESET_PASSWORD_FAILED, _accountSession.Language);
                    }
                    NLogManager.Info(string.Format("Account = {0} reset successful", account.AccountName));
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_RESET_PASSWORD_SUCCESS, _accountSession.Language);
                }
                else if (model.Type == 2) // email
                {

                }

                return new ResponseBuilder(ErrorCodes.ACCOUNT_RESET_PASSWORD_FAILED, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language);
            }
        }
        #endregion

        /// <summary>
        /// Note : bỏ accountName, accountId
        /// </summary>
        /// <param name="captcha"></param>
        /// <param name="captchaToken"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("UpdateUserFullName")]
        public ActionResult<ResponseBuilder> UpdateUserFullName(string fullName, string captcha, string captchaToken)
        {
            try
            {
                if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || (_captcha.VerifyCaptcha(captcha, captchaToken) < 0))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                if (!string.IsNullOrEmpty(_accountSession.NickName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_UPDATED, _accountSession.Language);

                if (string.IsNullOrEmpty(fullName) || !PolicyUtil.CheckNickName(fullName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_INVALID, _accountSession.Language);

                //if (fullName.ToLower().CompareTo(_accountSession.AccountName.ToLower()) == 0)
                //    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_MUST_DEFFENT_ACCNAME, _accountSession.Language);

                //string cookieUsername = accountSession.SessionName(account.AccountID, account.UserName, account.MerchantID, account.PlatformID, account.UserFullname);
                var accountInfo = _accountDAO.GetAccount(_accountSession.AccountID, _accountSession.AccountName);
                if (accountInfo == null)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                }

                if (!string.IsNullOrEmpty(accountInfo.UserFullname))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_UPDATED, _accountSession.Language);

                int response = _accountDAO.UpdateUserFullName(_accountSession.AccountName, _accountSession.AccountID, fullName);
                if (response >= 0)
                {
                    AccountDb accountDb = new AccountDb();
                    accountDb.AccountID = _accountSession.AccountID;
                    accountDb.UserName = _accountSession.AccountName;
                    accountDb.UserFullname = fullName;
                    accountDb.MerchantID = _accountSession.MerchantID;
                    accountDb.PlatformID = _accountSession.PlatformID;
                    accountDb.ClientIP = _accountSession.IpAddress;
                    accountDb.IsAgency = _agencyDao.CheckIsAgency((int)accountDb.AccountID);

                    AccountInfo accountInfoReturn = new AccountInfo(accountDb);
                    accountInfoReturn.AccessToken = _authenticateService.GenerateToken(accountDb);
                    accountInfoReturn.AccessTokenFishing = _authenticateService.GenerateTokenFishing(accountDb);

                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, accountInfoReturn);
                }
                else if (response == -50)
                    return new ResponseBuilder(ErrorCodes.NICKNAME_EXIST, _accountSession.Language);

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_UPDATE_FAILED, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("UpdateUserFullNameFB")]
        public ActionResult<ResponseBuilder> UpdateUserFullNameFB(string fullName, string userName, string password, string captcha, string captchaToken)
        {
            try
            {
                if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || (_captcha.VerifyCaptcha(captcha, captchaToken) < 0))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                userName = userName.ToLower();
                fullName = userName;
                if (string.IsNullOrEmpty(fullName) || !PolicyUtil.CheckNickName(fullName))
                    return new ResponseBuilder(ErrorCodes.NICKNAME_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(userName) || !PolicyUtil.CheckUserName(userName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_INVALID, _accountSession.Language);

                if (!PolicyUtil.CheckPassword(password))
                    return new ResponseBuilder(ErrorCodes.PASSWORD_INVALID, _accountSession.Language);

                // if (fullName.ToLower().CompareTo(userName.ToLower()) == 0)
                //     return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_MUST_DEFFENT_ACCNAME, _accountSession.Language);

                //string cookieUsername = accountSession.SessionName(account.AccountID, account.UserName, account.MerchantID, account.PlatformID, account.UserFullname);
                var accountInfo = _accountDAO.GetAccount(_accountSession.AccountID, _accountSession.AccountName);
                if (accountInfo == null)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                }

                if (!_accountSession.AccountName.StartsWith("FB."))
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_ONLY_USE_FACEBOOK, _accountSession.Language);
                }

                if (!string.IsNullOrEmpty(accountInfo.UserFullname))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_UPDATED, _accountSession.Language);

                string passwordEncryp = Security.GetPasswordEncryp(password, userName);
                int response = _accountDAO.UpdateUserFullNameFB(_accountSession.AccountID, userName, passwordEncryp, fullName);
                if (response >= 0)
                {
                    AccountDb accountDb = new AccountDb();
                    accountDb.AccountID = _accountSession.AccountID;
                    accountDb.UserName = userName;
                    accountDb.UserFullname = fullName;
                    accountDb.MerchantID = _accountSession.MerchantID;
                    accountDb.PlatformID = _accountSession.PlatformID;
                    accountDb.ClientIP = _accountSession.IpAddress;
                    accountDb.IsAgency = _agencyDao.CheckIsAgency((int)accountDb.AccountID);

                    AccountInfo accountInfoReturn = new AccountInfo(accountDb);
                    accountInfoReturn.AccessToken = _authenticateService.GenerateToken(accountDb);
                    accountInfoReturn.AccessTokenFishing = _authenticateService.GenerateTokenFishing(accountDb);

                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, accountInfoReturn);
                }
                else if (response == -50)
                    return new ResponseBuilder(ErrorCodes.NICKNAME_EXIST, _accountSession.Language);
                else if (response == -49)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_EXIST, _accountSession.Language);

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_UPDATE_FAILED, _accountSession.Language);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetInfo")]
        public ActionResult<ResponseBuilder> GetSecurityInfo()
        {
            try
            {
                SecurityInfo securityInfo = _otp.GetSucurityInfoWithMask(_accountSession.AccountID);
                //_accountDAO.GetSecurityInfo((int)accountSession.AccountID);
                if (securityInfo != null)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, securityInfo);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }
        #region [ĐĂNG KÝ BẢO MẬT OTP]
        /// <summary>
        /// Đăng ký OTP
        /// </summary>
        /// <param name="model"> model.Type: 1 = Đăng ký OTP; 2 = Hủy OTP; 3 = Hủy số; </param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RegisterOTP")]
        public ActionResult<ResponseBuilder> RegisterSecurity([FromBody] RegisterSecurityModels model)
        {
            if (model == null)
                return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

            try
            {
                string userMobile = "";
                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo((int)_accountSession.AccountID);
                if (securityInfo != null)
                {
                    userMobile = securityInfo.Mobile;
                    if (model.Type == 1) // Đăng ký bảo mật
                    {
                        if (securityInfo.IsMobileActived)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_ALREADY_REGISTER_OTP, _accountSession.Language);

                        if (string.IsNullOrEmpty(securityInfo.Mobile))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                        if (string.IsNullOrEmpty(model.SecureCode))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_SERCURE_CODE_INVALID, _accountSession.Language);

                        // Check otp bằng API: với kiểu SMS
                        if (_otp.VerifyOtpByApi(userMobile, model.SecureCode, model.OtpType) != 0)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                        //if (!_otp.VerifyOTP(_accountSession.AccountName, (int)_accountSession.AccountID, model.SecureCode, 1, securityInfo.Mobile, 0))
                        //    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                        int res = _accountDAO.RegisterOTP((int)_accountSession.AccountID, _accountSession.AccountName, securityInfo.Mobile, (int)RegisterOTPType.REGISTER_OTP);
                        if (res >= 0)
                        {
                            securityInfo = _otp.GetSucurityInfoWithMask((int)_accountSession.AccountID);
                            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, securityInfo);
                        }
                        else
                        {
                            NLogManager.Error(string.Format("Đăng ký OTP không thành công. Mobile = {0}, Mobile = {1}, Otp = {2}, Respone = {3}", _accountSession.AccountName, securityInfo.Mobile, model.SecureCode, res));
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_REGISTER_OTP_FAILED, _accountSession.Language);
                        }
                    }
                    else if (model.Type == 2 || model.Type == 3) // Huy OTP va Huy số
                    {
                        if (!securityInfo.IsMobileActived)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER_YET, _accountSession.Language);

                        if (string.IsNullOrEmpty(securityInfo.Mobile))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                        //if(model.Mobile.CompareTo(securityInfo.Mobile) != 0)
                        //    return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại không đúng với số điện thoại đã đăng ký!", "BadRequest");

                        if (string.IsNullOrEmpty(model.SecureCode))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_SERCURE_CODE_INVALID, _accountSession.Language);

                        // Check otp bằng API: với kiểu SMS
                        //if (!_otp.VerifyOtpByApi(userMobile, model.SecureCode, 1))
                        //    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);
                        // Verify bằng SP
                        if (!_otp.VerifyOTP(_accountSession.AccountName, (int)_accountSession.AccountID, model.SecureCode, 1, securityInfo.Mobile, 0))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                        int res = _coreAPI.RegisterOTP((int)_accountSession.AccountID, _accountSession.AccountName, securityInfo.Mobile, (int)RegisterOTPType.UNREGISTER_OTP);
                        NLogManager.Info(string.Format("Mobile = {0}, Mobile = {1}, Otp = {2}, Respone = {3}, Type = {4}", _accountSession.AccountName, securityInfo.Mobile, model.SecureCode, res, model.Type));
                        if (res >= 0)
                        {
                            securityInfo = _otp.GetSucurityInfoWithMask((int)_accountSession.AccountID, false);
                            DeleteOTPApp(_accountSession.AccountName, userMobile);

                            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(securityInfo));
                        }
                        else
                        {
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_CANCEL_FAILED, _accountSession.Language);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }
        #endregion


        /// <summary>
        /// Thay đổi số điện thoại đăng ký OTP
        /// </summary>
        /// <param name="model">
        /// bỏ accountID và accountName
        /// </param>
        /// <returns>
        /// </returns>
        [Authorize]
        [HttpPost("ChangeMobile")]
        public ActionResult<ResponseBuilder> ChangeMobileOTP([FromBody] dynamic model)
        {
            return new ResponseBuilder(ErrorCodes.ACCOUNT_CAN_NOT_EXC_FUNCTION, _accountSession.Language);

            //if (model == null)
            //    return new ResponseBuilder(ErrorCodes.BadRequest, "Dữ liệu không hợp lệ", "BadRequest");

            //try
            //{
            //    string userName = accountSession.AccountName;
            //    string currentMobile = model.CurrentMobile;
            //    long accountId = accountSession.AccountID;
            //    string newMobile = model.Mobile;
            //    string otp = model.Otp;
            //    int step = model.Step;

            //    if (string.IsNullOrEmpty(otp))
            //        return new ResponseBuilder(ErrorCodes.BadRequest, "Mã bảo mật không hợp lệ!", "BadRequest");

            //    SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(accountId);
            //    if (securityInfo != null)
            //    {
            //        if (step == 1)
            //        {
            //            if (securityInfo.Mobile.CompareTo(currentMobile) != 0)
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại hiện tại không đúng", "BadRequest");

            //            if (string.IsNullOrEmpty(securityInfo.Mobile))
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Bạn chưa cập nhật thông tin số điện thoại!", "BadRequest");

            //            if (string.IsNullOrEmpty(newMobile) || !PolicyUtil.CheckMobile(newMobile))
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại mới không hợp lệ", "BadRequest");

            //            if (newMobile.CompareTo(securityInfo.Mobile) == 0)
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại mới phải khác số cũ", "BadRequest");

            //            // kiểm tra số đt mới đã đc đăng ký trên hệ thống chưa
            //            var newPhoneInfo = _accountDAO.GetSecurityInfoByMobile(newMobile);
            //            if (newPhoneInfo != null && newPhoneInfo.Count > 0)
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại mới đã được đăng ký OTP.", "BadRequest");

            //            if (securityInfo.IsMobileActived)
            //            {
            //                var resultCheck = _otp.CheckOTP(accountId, userName, otp, 1, (int)OtpType.OTP_NORMAL);
            //                if (resultCheck != OTPCode.OTP_VERIFY_SUCCESS)
            //                    return new ResponseBuilder(ErrorCodes.BadRequest, "Mã OTP không hợp lệ!", "BadRequest");

            //                // xóa số cũ trên DB
            //                string oldMobile = securityInfo.Mobile;
            //                int res = _coreAPI.UpdateMobile(accountId, newMobile);
            //                if (res >= 0)
            //                {
            //                    // Gửi thông tin số mới lại cho client, ch
            //                    securityInfo = _otp.GetSucurityInfoWithMask(accountId);
            //                    return new ResponseBuilder(ErrorCodes.OK, JsonConvert.SerializeObject(securityInfo), "OK");
            //                }
            //                else if (res == -102)
            //                {
            //                    return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại đã tồn tại.", "BadRequest");
            //                }
            //                else
            //                {
            //                    return new ResponseBuilder(ErrorCodes.BadRequest, "Thay đổi số điện thoại không thành công.", "BadRequest");
            //                }
            //            }
            //            else
            //            {
            //                string oldMobile = securityInfo.Mobile;
            //                int res = _coreAPI.UpdateMobile(accountId, newMobile);
            //                if (res >= 0)
            //                {
            //                    securityInfo = _otp.GetSucurityInfoWithMask(accountId);
            //                    return new ResponseBuilder(ErrorCodes.OK, JsonConvert.SerializeObject(securityInfo), "OK");
            //                }
            //                else if (res == -102)
            //                {
            //                    return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại đã tồn tại.", "BadRequest");
            //                }
            //                else
            //                {
            //                    return new ResponseBuilder(ErrorCodes.BadRequest, "Thay đổi số điện thoại không thành công.", "BadRequest");
            //                }
            //            }
            //        }
            //        else if (step == 2) // đăng ký OTP cho số mới
            //        {
            //            if (string.IsNullOrEmpty(otp))
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Mã OTP không hợp lệ.", "BadRequest");

            //            if (string.IsNullOrEmpty(securityInfo.Mobile))
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Bạn chưa cập nhật thông tin số điện thoại!", "BadRequest");

            //            if (string.IsNullOrEmpty(newMobile) || !PolicyUtil.CheckMobile(newMobile))
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại mới không hợp lệ", "BadRequest");

            //            if (newMobile.CompareTo(securityInfo.Mobile) != 0)
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Số điện thoại mới không khớp với số đã cập nhật trên hệ thống", "BadRequest");

            //            if (_otp.CheckOTP((int)accountSession.AccountID, accountSession.AccountName, otp, 1, (int)OtpType.OTP_REGISTER) != OTPCode.OTP_VERIFY_SUCCESS)
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Mã OTP không hợp lệ", "BadRequest");

            //            int res = _coreAPI.RegisterOTP((int)accountSession.AccountID, accountSession.AccountName, securityInfo.Mobile, (int)RegisterOTPType.REGISTER_OTP);
            //            if (res >= 0)
            //            {
            //                securityInfo = _otp.GetSucurityInfoWithMask((int)accountSession.AccountID);
            //                return new ResponseBuilder(ErrorCodes.OK, JsonConvert.SerializeObject(securityInfo), "OK");
            //            }
            //            else
            //            {
            //                NLogManager.Error(string.Format("Thay đổi số điện thoại không thành công. Mobile = {0}, Mobile = {1}, Otp = {2}, Respone = {3}", accountSession.AccountName, securityInfo.Mobile, otp, res));
            //                return new ResponseBuilder(ErrorCodes.BadRequest, "Thay đổi số điện thoại không thành công.", "BadRequest");
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Exception(ex);
            //}
            //return new ResponseBuilder(ErrorCodes.BadRequest, "Hệ thống lỗi, vui lòng thử lại!", "BadRequest");
        }

        #region [KÍCH HOẠT - HỦY ĐĂNG NHÂP 2 BƯỚC]
        /// <summary>
        /// Dang ky login co OTP
        /// </summary>
        /// <param name="model">
        /// model.Type: 1--> Đăng ký login co OTP; 2--> Hủy login OTP
        /// </param>
        /// <returns>
        /// Thanh cong tra ve thong tin bao mat cua nguoi choi
        /// </returns>
        [Authorize]
        [HttpPost("RegisterLoginOTP")]
        public ActionResult<ResponseBuilder> RegisterLoginOTP([FromBody] dynamic model)
        {
            if (model == null)
                return new ResponseBuilder(ErrorCodes.DATA_IS_NULL, _accountSession.Language);
            try
            {
                long accountId = _accountSession.AccountID;
                string accountName = _accountSession.AccountName;
                int type = model.type; // 1: đăng ký, 2: Hủy đăng ký
                string otp = model.otp;
                int otpType = model.otpType; // 1: SMS. 2: App
                //int minBon = model.minBon;
                int minBon = 0;

                if (type < 1 || type > 2)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                //string captcha = model.Captcha;
                //string captchaToken = model.CaptchaToken;

                //if (minBon < 0)
                //    return new ResponseBuilder(ErrorCodes.TOTAL_COIN_MUST_GREATER_THAN_0, _accountSession.Language);

                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(accountId);
                if (securityInfo != null)
                {
                    if (!securityInfo.IsMobileActived)
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER_YET, _accountSession.Language);

                    if (string.IsNullOrEmpty(securityInfo.Mobile))
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                    if (string.IsNullOrEmpty(otp))
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, "Mã bảo mật không hợp lệ!", "BadRequest");

                    if (type == 1) // Đăng ký bảo mật đăng nhập
                    {
                        if (securityInfo.IsLoginOTP)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_ALREADY_REGISTER_OTP, _accountSession.Language);

                        //OTPCode res = _otp.CheckOTP(accountId, accountName, otp, otpType);
                        // Xác thực qua API
                        if (_otp.VerifyOtpByApi(securityInfo.Mobile, otp, otpType) == 0)
                        {
                            if (_accountDAO.RegisterLoginOTP(accountId, accountName, 1, minBon) >= 0)
                            {
                                securityInfo = _otp.GetSucurityInfoWithMask(accountId);
                                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, securityInfo);
                            }
                        }

                        return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);
                    }
                    else if (type == 2) // Hủy đăng ký đăng nhập bằng OTP
                    {
                        if (!securityInfo.IsLoginOTP)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER_YET, _accountSession.Language);

                        OTPCode code = _otp.CheckOTP(accountId, accountName, otp, otpType);
                        if (code == OTPCode.OTP_VERIFY_SUCCESS)
                        {
                            if (_accountDAO.RegisterLoginOTP(accountId, accountName, 0, minBon) >= 0)
                            {
                                securityInfo = _otp.GetSucurityInfoWithMask(accountId);
                                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(securityInfo));
                            }
                        }
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.ACCOUNT_SERCURE_ACTIVE_FAILED, _accountSession.Language);
        }
        #endregion

        [Authorize]
        [HttpGet("GetOTPByEmail")]
        public ActionResult<ResponseBuilder> GetOTPByEmail(string captcha, string captchaToken)
        {
            try
            {
                if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || _captcha.VerifyCaptcha(captcha, captchaToken) < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo((int)_accountSession.AccountID);
                if (securityInfo != null)
                {
                    if (string.IsNullOrEmpty(securityInfo.Email))
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_EMAIL, _accountSession.Language);

                    string token = "";
                    if (SendOTPToUser(_accountSession.AccountName, (int)_accountSession.AccountID, securityInfo.Email, 2, ref token))
                    {
                        return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, token);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpPost("CancelSecurity")]
        public ActionResult<ResponseBuilder> CancelSecurity([FromBody] RegisterSecurityModels model)
        {
            if (model == null)
            {
                return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);
            }
            try
            {

                string captcha = model.Captcha;
                string captchaToken = model.CaptchaToken;

                if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || _captcha.VerifyCaptcha(captcha, captchaToken) < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo((int)_accountSession.AccountID);
                if (securityInfo != null)
                {
                    if (model.Type == 1) // Hủy bảo mật bằng SMS
                    {

                    }
                    else if (model.Type == 2) // Hủy bảo mật bằng Email
                    {
                        if (!securityInfo.IsEmailActived)
                        {
                            return BadRequest("Bạn chưa đăng ký bảo mật bằng email nên không thể hủy");
                        }

                        if (!string.IsNullOrEmpty(securityInfo.Email))
                        {
                            // nếu chưa gửi mã kích hoạt thì gửi mã kích hoạt tới email người chơi
                            if (string.IsNullOrEmpty(model.SecureCode))
                            {
                                string codeVerify = Security.RandomString(10);
                                //int res = _accountDAO.CreateSecurityEmail((int)accountSession.AccountID, accountSession.Mobile, securityInfo.Email, ref codeVerify);

                                if (!string.IsNullOrEmpty(codeVerify))
                                {
                                    string tick = DateTime.Now.Ticks.ToString();
                                    string token = Security.TripleDESEncrypt(OTPCHECK_SECRET_KEY, string.Format("{0}|{1}|{2}|{3}|{4}", _accountSession.AccountID, _accountSession.AccountName, codeVerify, tick, "ibon"));

                                    StringBuilder content = new StringBuilder();
                                    content.AppendLine("Chào bạn");
                                    content.AppendLine("Chúng tôi nhận được yêu cầu hủy bảo mật của tài khoản: " + _accountSession.AccountName);
                                    content.AppendLine("Mã hủy bảo mật của bạn là: " + codeVerify);
                                    content.AppendLine("Chúc bạn có những giây phút giải trí vui vẻ với Thần Tài Online.");
                                    content.AppendLine();

                                    if (SendEmail(securityInfo.Email, "Hủy bảo mật", content.ToString()))
                                    {
                                        //return Json(new
                                        //{
                                        //    Response = 0,
                                        //    Token = token
                                        //});
                                        return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, token);
                                    }
                                }
                            }
                            // nếu gửi securityCode thì kích hoạt bảo mật bằng email
                            else
                            {
                                if (string.IsNullOrEmpty(model.Token))
                                    return BadRequest("Dữ liệu không hợp lệ");

                                string token = Security.Decrypt(OTPCHECK_SECRET_KEY, model.Token);
                                var splitToken = token.Split('|');
                                if (splitToken.Length < 5)
                                    return BadRequest("Dữ liệu không hợp lệ");

                                if (Convert.ToInt64(splitToken[0]) != _accountSession.AccountID)
                                    return BadRequest("Dữ liệu không hợp lệ");

                                // Kiểm tra tính chính xác của mã bảo mật
                                if (model.SecureCode.CompareTo(splitToken[2]) < 0)
                                    return BadRequest("Mã bảo mật không hợp lệ, vui lòng thử lại");

                                // kiểm tra thời gian hiệu lực của mã bảo mật
                                long tickNow = DateTime.Now.Ticks;
                                long tickToken = Convert.ToInt64(splitToken[3]);
                                TimeSpan diffTime = new TimeSpan(tickNow - tickToken);
                                if (diffTime.TotalMinutes > 5)
                                    return BadRequest("Mã bảo mật đã hết hạn, vui lòng thử lại");

                                int res = _accountDAO.VerifySecurityEmail((int)_accountSession.AccountID, _accountSession.AccountName, securityInfo.Email, Convert.ToInt32(model.SecureCode));
                                if (res > 0)
                                {
                                    //return Json(new
                                    //{
                                    //    Response = 1,
                                    //    Token = "Hủy bảo mật thành công"
                                    //});
                                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, "Hủy bảo mật thành công");
                                }

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return BadRequest("Đã có lỗi xảy ra, vui lòng thử lại sau");
        }

        /// <summary>
        /// Đóng/mở băng
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="valueFrozen"></param>
        /// <param name="isFrozen"> 1 = đóng băng, 2 = mở băng</param>
        /// <returns> Thông tin đóng/mở băng của tài khoản</returns>
        /// http://10.6.6.6:8086/api/Account/frozen?accountId=112011&bonFrozen=10000&isFrozen=1&sourceId=2&otp=&otpType=1
        [Authorize]
        [HttpGet("frozen")]
        public ActionResult<ResponseBuilder> Frozen(long valueFrozen, int isFrozen, int sourceId, string otp, int otpType)
        {
            try
            {
                string accountName = _accountSession.AccountName;
                string ipAddress = _accountSession.IpAddress;
                long res = -1;
                long balance = -1;
                long frozenValue = -1;

                if (valueFrozen < 1000)
                    return new ResponseBuilder(ErrorCodes.TOTAL_COIN_MIN_FREEZE_NOT_ENOUGH, _accountSession.Language);

                if (isFrozen > 2 || isFrozen < 1)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                if (sourceId < 0)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                if (!_otp.IsOTP((int)_accountSession.AccountID))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER_YET, _accountSession.Language);

                if (isFrozen == 2)
                {
                    var checkOTP = _otp.CheckOTP((int)_accountSession.AccountID, accountName, otp, otpType);
                    if (checkOTP != OTPCode.OTP_VERIFY_SUCCESS)
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                    res = _coreAPI.Frozen(_accountSession.AccountID, valueFrozen, false, sourceId, ipAddress, out balance, out frozenValue);
                }
                else if (isFrozen == 1)
                {
                    res = _coreAPI.Frozen(_accountSession.AccountID, valueFrozen, true, sourceId, ipAddress, out balance, out frozenValue);
                }

                if (res >= 0)
                {
                    var obj = new FrozenData
                    {
                        Balance = balance,
                        FrozenValue = frozenValue
                    };
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(obj));
                }
                if (res == -98)
                {
                    return new ResponseBuilder(ErrorCodes.FROZEN_MIN_VALUE, _accountSession.Language);

                }
                return new ResponseBuilder(ErrorCodes.TRANSACTION_FAILED, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.COIN_FREEZE_FAILED, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("getFrozen")]
        public ActionResult<ResponseBuilder> GetFrozen()
        {
            try
            {
                long balance = -1;
                long frozenValue = _coreAPI.GetFrozenValue((int)_accountSession.AccountID, out balance);
                if (frozenValue >= 0)
                {
                    var obj = new FrozenData
                    {
                        Balance = balance,
                        FrozenValue = frozenValue
                    };
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(obj));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("updateFirebaseKey")]
        public ActionResult<ResponseBuilder> UpdateFirebaseKey(string fireBaseKey, string uiid, int productId)
        {
            try
            {
                long res = _coreAPI.UpdateFireBaseKey((int)_accountSession.AccountID, fireBaseKey, uiid, productId);
                if (res >= 0)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpPost("updateMobile")]
        public ActionResult<ResponseBuilder> UpdateMobile(dynamic data)
        {
            //string cookie = HttpContext.Current.Request.Cookies.Get(FormsAuthentication.FormsCookieName).Value;
            NLogManager.Info("TestUpdateMobile");
            string mobile = data.mobile;
            long accountId = _accountSession.AccountID;

            try
            {
                if (mobile.Length < 10)
                    return new ResponseBuilder(ErrorCodes.MOBILE_NUMBER_INVALID, _accountSession.Language);

                if (!Utils.IsNumber(mobile))
                    return new ResponseBuilder(ErrorCodes.MOBILE_NUMBER_INVALID, _accountSession.Language);
                NLogManager.Info("TestUpdateMobile1");
                mobile = Utils.FormatMobile(mobile);
                NLogManager.Info("TestUpdateMobile2");
                var info = _accountDAO.GetSecurityInfoByMobile(mobile);
                if (info != null && info.Count > 0)
                    return new ResponseBuilder(ErrorCodes.MOBILE_NUMBER_ALREADY_REGISTER, _accountSession.Language);
                NLogManager.Info("TestUpdateMobile3");
                long res = _coreAPI.UpdateMobile(accountId, mobile);
                if (res >= 0)
                {
                    var securityInfo = _otp.GetSucurityInfoWithMask(accountId, false);
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, securityInfo);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpPost("deleteMobile")]
        public ActionResult<ResponseBuilder> DeleteMobile(dynamic data)
        {
            string mobile = data.mobile;
            string otp = data.otp;
            int otpType = data.otptype;
            long accountId = _accountSession.AccountID;

            try
            {
                if (mobile.Length < 10)
                    return new ResponseBuilder(ErrorCodes.MOBILE_NUMBER_INVALID, _accountSession.Language);

                var info = _otp.GetSucurityInfoWithMask(accountId, false);
                if (info != null)
                {
                    if (info.Mobile.CompareTo(mobile) != 0)
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_NOT_MATCH, _accountSession.Language);

                    if (info.IsMobileActived)
                    {
                        var checkOTPStatus = _otp.CheckOTP(accountId, _accountSession.AccountName, otp, otpType, (int)OtpType.OTP_UNREGISTER);
                        if (checkOTPStatus != OTPCode.OTP_VERIFY_SUCCESS)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);
                    }
                }
                else
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                }

                long res = _coreAPI.DeleteMobile(accountId, mobile);
                if (res >= 0)
                {
                    var securityInfo = _otp.GetSucurityInfoWithMask(accountId, false);
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(securityInfo));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        #region [Loyalty]
        [Authorize]
        [HttpGet("getLoyalty")]
        public ActionResult<ResponseBuilder> GetLoyalty()
        {
            try
            {
                AccountLoyalty info = _accountDAO.GetLoyalty(_accountSession.AccountID);

                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, info);

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("checkNickNameExist")]
        public ActionResult<ResponseBuilder> CheckNickNameExist(string nickName)
        {
            try
            {
                if (_accountSession.AccountID <= 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

                int accountIdReceiver = -1;
                string accountNameReceive = "";
                int accountReceive = _coreAPI.GetAccountByNickName(nickName.Trim(), out accountIdReceiver, out accountNameReceive);
                if (accountReceive > 0)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, _accountSession.Language);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.NICKNAME_INVALID, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("vipPointTrade")]
        public ActionResult<ResponseBuilder> VipPointTrade(int vipPoint, int otpType, string otp)
        {
            try
            {
                if (vipPoint <= 0)
                    return new ResponseBuilder(ErrorCodes.VIP_POINT_INVALID, _accountSession.Language);

                if (otpType < 1 || otpType > 2 || string.IsNullOrEmpty(otp))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language);

                if (!_otp.IsOTP((int)_accountSession.AccountID))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER, _accountSession.Language);

                if (_accountDAO.CheckUpdateMobile(_accountSession.AccountID) < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_NOT_ENOUGH_24H, _accountSession.Language);

                var checkOTP = _otp.CheckOTP((int)_accountSession.AccountID, _accountSession.AccountName, otp, otpType);
                if (checkOTP != OTPCode.OTP_VERIFY_SUCCESS)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                int vipPointCurrent = 0;
                NLogManager.Info(string.Format("UserName = {0}, VipPoint = {1}, OtpType = {2}, Otp = {3}", _accountSession.AccountName, vipPoint, otpType, otp));
                long res = _accountDAO.VipPointTrade(_accountSession.AccountID, _accountSession.AccountName, vipPoint, out vipPointCurrent);

                var obj = new VipPointTradeData
                {
                    Balance = res,
                    VipPointCurrent = vipPointCurrent
                };
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(obj));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("getRankingVip")]
        public ActionResult<ResponseBuilder> GetRankingVip()
        {
            try
            {
                var info = _accountDAO.GetRankingVip();
                if (info != null)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(info));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [HttpGet("CheckPass")]
        public ActionResult<string> CheckPass(string password, string username)
        {
            return Security.GetPasswordEncryp(password, username);
        }


        #endregion
        #region private

        private bool ResetPasswordBySMS_CheckInput(ResetPassModels model, AccountEbank account, ref ReturnData returnData)
        {
            returnData = new ReturnData();
            try
            {
                var phone = string.Empty;
                var username = string.Empty;
                var securitycode = string.Empty;
                var status = 0;
                var listSmsBanking = _accountDAO.GetSmsbankingByAccountId(account.AccountID, ref phone, ref username, ref securitycode, ref status);
                var otpservice = listSmsBanking.Find(l => l.SMSServiceID == 5); // SMSServiceID == 5 là otp
                if (otpservice == null || otpservice.ServiceStatus != 1)
                {
                    returnData.ResponseCode = -8999;
                    returnData.Description = "Bạn chưa đăng ký SMSPlus. Vui lòng chọn phương thức khác";
                    return false;
                }

                if (string.IsNullOrEmpty(model.Otp))
                {
                    returnData.ResponseCode = -8999;
                    returnData.Description = "Vui lòng nhập mã xác thực";
                    return false;
                }

                // otp không đúng hoặc hết hạn
                var checkOTP = _accountDAO.CheckOtp(account.AccountID, model.Otp, 1);
                if (checkOTP == -6) //otp hết hạn
                {
                    returnData.ResponseCode = -8999;
                    returnData.Description = "Mã xác thực đã hết hạn. Vui lòng lấy lại";
                    return false;
                }

                if (checkOTP != 0)
                {
                    returnData.ResponseCode = -8999;
                    returnData.Description = "Mã xác thực không hợp lệ";
                    return false;
                }
                //
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                returnData.ResponseCode = -8999;
                returnData.Description = "Hệ thống đang bận, vui lòng quay lại sau";
                return false;
            }
        }

        private bool ResetPasswordByEmail_CheckInput(ResetPassModels model, AccountEbank account, ref ReturnData returnData)
        {
            returnData = new ReturnData();
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    returnData.ResponseCode = -8999;
                    returnData.Description = "Vui lòng nhập địa chỉ email";
                    return false;
                }

                if (string.IsNullOrEmpty(account.Email))
                {
                    returnData.ResponseCode = -8999;
                    returnData.Description = "Địa chỉ email đăng ký của tài khoản chưa được cập nhật";
                    return false;
                }

                if (model.Email.ToLower() != account.Email.ToLower())
                {
                    returnData.ResponseCode = -8999;
                    returnData.Description = "Địa chỉ email không hợp lệ";
                    return false;
                }

                // tạo ra 1 pass word mới
                var rnd = new Random();
                account.ConfirmCode = rnd.Next(100000, 999999);

                //if(!SendEmailSecureCode(account))
                //{
                //    returnData.ResponseCode = -8999;
                //    returnData.Description = "Hệ thống không gửi được email tới email của bạn, vui lòng thử lại sau";
                //    return false;
                //}

                // chưa truyền Otp thì gửi email
                //if (string.IsNullOrEmpty(model.Otp))
                //{
                //    if (!SendEmailSecureCode(account))
                //    {
                //        returnData.ResponseCode = -8999;
                //        returnData.Description = "Hệ thống không gửi được email tới email của bạn, vui lòng thử lại sau";
                //        return false;
                //    }

                //    returnData.ResponseCode = -8004;
                //    returnData.Description = "Vui lòng nhập mã xác thực";
                //    return false;
                //}
                //// có Otp thì kiểm tra
                //if (model.Otp != account.ConfirmCode.ToString())
                //{
                //    returnData.ResponseCode = -8999;
                //    returnData.Description = "Mã xác thực không hợp lệ";
                //    return false;
                //}

                // xác thực thành công thì update confirmcode mới cho tài khoản
                //var rnd = new Random();
                //account.ConfirmCode = rnd.Next(100000, 999999);
                //var resultUpdate = _accountDAO.UpdateUserConfirm(account.Mobile, account.ConfirmCode);
                //if (resultUpdate < 0)
                //{
                //    NLogManager.LogMessage("UpdateUserConfirm:-->" + account.Mobile + "|resultUpdate:-->" + resultUpdate);
                //    returnData.ResponseCode = -8999;
                //    returnData.Description = "Lấy lại mật khẩu không thành công.Vui lòng quay lại sau";
                //    return false;
                //}

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                returnData.ResponseCode = -8999;
                returnData.Description = "Hệ thống đang bận, vui lòng quay lại sau";
                return false;
            }
        }

        private bool SendSecureCode(string emailOrPhone, string content)
        {
            try
            {
                return SendEmail(emailOrPhone, "Reset mật khẩu", content);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return false;
            }
        }

        private bool SendEmail(string toEmail, string subject, string body)
        {
            string email = Convert.ToString(ConfigurationManager.AppSettings["Email"]);
            string password = Convert.ToString(ConfigurationManager.AppSettings["PasswordEmail"]);
            string emailHost = Convert.ToString(ConfigurationManager.AppSettings["EmailHost"]);
            int emailPort = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);
            string displayName = Convert.ToString(ConfigurationManager.AppSettings["DisplayName"]);
            bool result = false;

            try
            {
                var loginInfo = new NetworkCredential(email, password);
                var msg = new MailMessage();
                var smtpClient = new SmtpClient(emailHost, emailPort);

                msg.From = new MailAddress(email, displayName, Encoding.UTF8);
                msg.To.Add(new MailAddress(toEmail));
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = false;

                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = loginInfo;
                smtpClient.Timeout = 30000;
                smtpClient.SendAsync(msg, "Send email");

                NLogManager.Info(string.Format("Send email success from {0} to {1}", email, toEmail));
                result = true;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                result = false;
            }

            return result;
        }

        private bool SendPassWordResetToUser(AccountEbank account, string emailOrPhone, int type)
        {
            if (string.IsNullOrEmpty(emailOrPhone) || account == null)
            {
                return false;
            }

            string passRandom = Security.RandomString(10);
            var oldPassMd5 = account.Password;
            account.Password = Security.MD5Encrypt(passRandom);
            var result_reset = _accountDAO.ResetPassword(account.AccountName, account.Password, (short)type);

            if (account.Error < 0 || result_reset < 0)
            {
                return false;
            }

            //StringBuilder content = new StringBuilder();
            //content.AppendLine("Chào bạn");
            //content.AppendLine("Chúng tôi nhận được yêu cầu lấy lại mật khẩu của tài khoản: " + account.Mobile);
            //content.AppendLine("Mật khẩu mới của bạn là: " + passRandom);
            //content.AppendLine("Chúc bạn có những giây phút giải trí vui vẻ với Thần Tài Online.");
            //content.AppendLine();

            //if(!SendEmail(emailOrPhone, "Reset mật khẩu", content.ToString()))
            //{
            //    _accountDAO.ResetPassword(account.Mobile, oldPassMd5, (short)type);
            //    return false;
            //}

            // ghi log, không thành công, update oldpass
            var error = _accountDAO.LogAccountChangepassWaiting_Insert(account.AccountID, account.AccountName, account.Password, account.Password);
            if (error < 0)
            {
                NLogManager.Info("LogAccountChangepassWaiting_Insert failed, error = " + error);
                _accountDAO.ResetPassword(account.AccountName, oldPassMd5, (short)type);
                return false;
            }

            return true;
        }
        #endregion Quên mật khẩu

        private bool RegisterSecurityByEmail()
        {
            return true;
        }

        private bool RegisterSecurityBySMS()
        {
            return true;
        }

        private bool SendOTPToUser(string accountName, int accountId, string email, int type, ref string token)
        {
            try
            {
                string tick = DateTime.Now.Ticks.ToString();
                token = "";
                int codeVerify = 0;

                if (type == 1) // Đăng ký bảo mật bằng SMS
                {
                    int res = _accountDAO.CreateSecuritySMS(accountId, accountName, email, 1, ref codeVerify);
                    if (res > 0)
                    {
                        token = Security.TripleDESEncrypt(OTPCHECK_SECRET_KEY, string.Format("{0}|{1}|{2}|{3}|{4}", accountId, accountName, codeVerify, tick, "ibon"));
                    }
                }
                else if (type == 2) // Đăng ký bảo mật bằng Email
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        int res = _accountDAO.CreateSecurityEmail(accountId, accountName, email, ref codeVerify);
                        if (res > 0)
                        {
                            token = Security.TripleDESEncrypt(OTPCHECK_SECRET_KEY, string.Format("{0}|{1}|{2}|{3}|{4}", accountId, accountName, codeVerify, tick, "ibon"));
                            StringBuilder content = new StringBuilder();
                            content.AppendLine("Chào bạn");
                            content.AppendLine("Chúng tôi nhận được yêu cầu kích hoạt bảo mật của tài khoản: " + accountName);
                            content.AppendLine("Mã bảo mật của bạn là: " + codeVerify);
                            content.AppendLine("Chúc bạn có những giây phút giải trí vui vẻ với Thần Tài Online.");
                            content.AppendLine();

                            if (SendEmail(email, "Mã bảo mật OTP", content.ToString()))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return false;
        }

        private int DeleteOTPApp(string userName, string mobile)
        {
            try
            {
                //IOTPDAO _iOTPDAO = AbstractDAOFactory.Instance().IOTPDAO();

                OTPs.SetupAppTokenOutput _dataOutput = new OTPs.SetupAppTokenOutput();
                //_dataOutput.SystemID = int.Parse(ConfigurationManager.AppSettings["SystemID"]);
                //_dataOutput.ServiceID = int.Parse(ConfigurationManager.AppSettings["SERVICEID_OTP_APP"]);
                //_dataOutput.SystemUserName = userName;
                //_dataOutput.MinAmount = int.Parse(ConfigurationManager.AppSettings["MinAmount"]);

                _dataOutput.Mobile = mobile;
                _dataOutput.Status = 9;
                _dataOutput.TokenKey = "";
                _dataOutput.Description = "";
                _dataOutput.otp = 0;
                _dataOutput.VerifyCode = "";
                OTPs.ResponMes responMes2 = _iOTPDAO.SetupAppToken(_dataOutput);

                NLogManager.Info(string.Format("DeleteOTPApp: Mobile = {0}, Mobile = {1}, ResponCode = {2}", userName, mobile, responMes2.ResponCode));
                return responMes2.ResponCode;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1;
        }
        #endregion
    }

    #region class

    public class ResetPassModels
    {

        public int ServiceID { get; set; }

        public string AccountName { get; set; }

        public byte Type { get; set; } // 1: Lấy lại qua SMS; 2: Lấy lại qua email

        public string Email { get; set; }

        public string Otp { get; set; }

        public int OtpType { get; set; }

        public string OtpToken { get; set; }

        public string NewPassword { get; set; }

        public string Captcha { get; set; }

        public string Token { get; set; }
    }

    public class ResetPassAgencyModels
    {
        public int AccountIDAgency { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }


    public class RegisterSecurityModels
    {
        //public string Mobile
        //{
        //    get;
        //    set;
        //}

        //public int AccountId
        //{
        //    get;
        //    set;
        //}
        public byte Type { get; set; } // 1: Kích hoạt OTP; 2: Huy OTP

        public byte OtpType { get; set; } // 1 sms, 2 tele

        public string SecureCode { get; set; } // mã xác thực

        public string Token { get; set; }

        public string Captcha { get; set; }

        public string CaptchaToken { get; set; }

        public string Mobile { get; set; }

        public string ServiceKey { get; set; }
    }

    public class ReturnData
    {
        public int ResponseCode { get; set; }

        public string Description { get; set; }

        public string Extend { get; set; }
    }

    #endregion


}