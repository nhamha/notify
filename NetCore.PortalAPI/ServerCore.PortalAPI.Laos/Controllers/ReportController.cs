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
    [Route("Report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly OTPSecurity _otp;
        private readonly IAccountDAO _accountDAO;
        private readonly AccountSession _accountSession;
        private readonly CoreAPI _coreAPI;

        private readonly int _MIN_BON;
        private readonly int _MAX_BON;


        private static int _maxLengthUserName;
        private static int _maxLengthNickName;

        private static string _loginActionName;
        private static int _loginFailAllow;
        private static int _loginFailTime;
        private readonly AppSettings _appSettings;
        private IAuthenticateService _authenticateService;
        private readonly Captcha _captcha;

        public ReportController(IAuthenticateService authenticateService, AccountSession accountSession, OTPSecurity otp, IAccountDAO accountDAO, CoreAPI coreAPI, IOptions<AppSettings> options, Captcha captcha)
        {
            this._accountSession = accountSession;
            this._otp = otp;
            this._accountDAO = accountDAO;
            this._coreAPI = coreAPI;

            _appSettings = options.Value;
            _authenticateService = authenticateService;

            _maxLengthUserName = _appSettings.MaxLengthUserName;
            _maxLengthNickName = _appSettings.MaxLengthNickName;

            _loginFailAllow = _appSettings.LoginFailAllow;
            _loginFailTime = _appSettings.LoginFailTime;
            _loginActionName = _appSettings.LoginActionName;

            _MIN_BON = _appSettings.MinBonTransfer;
            _MAX_BON = _appSettings.MaxBonTransfer;
            _captcha = captcha;
        }

        /// <summary>
        /// Ham login cho Report
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public ActionResult<ResponseBuilder> LoginForWebReport(LoginAccount loginAccount)
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
                        return new ResponseBuilder(ErrorCodes.CAPTCHA_INVALID, lng, accountInfo);
                    }
                }

                LoginAccount account = loginAccount;
                account.IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                int response = _authenticateService.ReportLogin(account, out accountInfo);
                if (response == (int)ErrorCodes.SUCCESS)
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

        /// <summary>
        /// Xác thực mã OTP
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        [HttpGet("CheckOTP")]
        public ActionResult<ResponseBuilder> CheckOTP(string serviceKey, string otp, int accountId)
        {
            try
            {
                if (!PolicyUtil.IsServiceKeyOk(serviceKey, accountId))
                    return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);

                if (accountId <= 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);

                var otpCheck = _otp.CheckOTP(accountId, _accountSession.AccountName, otp, 1);
                if (otpCheck == OTPCode.OTP_NEED_INPUT)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language);

                if (otpCheck == OTPCode.OTP_NOT_VERIFY || otpCheck == OTPCode.OTP_VERIFY_ERROR)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                if (otpCheck == OTPCode.OTP_VERIFY_SUCCESS)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        /// <summary>
        /// API chuyen tien cho Report
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// ErrorCodes.OK
        /// Số dư của tk chuyển nếu thành công.
        /// </returns>
        [Authorize]
        [HttpPost("transferReport")]
        public ActionResult<ResponseBuilder> TransferReport([FromBody]dynamic data)
        {
            NLogManager.Info("transferReport: " +  JsonConvert.SerializeObject(data));
            if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
                return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Sai mã token");
            string ipAddress = _accountSession.IpAddress;
            int status = -1;
            try
            {
                int accountIdTrans = (int)_accountSession.AccountID;
                string nickNameTrans = _accountSession.NickName;
                string accountNameTrans = _accountSession.AccountName;
                int transferValue = data.transferValue;
                string nickNameRecv = data.nickNameRecv;
                int sourceId = data.sourceId; // 4 = web
                int transferType = 1;//data.transferType; // 1 = Chuyen TotalCoin, 2 = chuyen Xu
                //int otpType = data.otpType; // 1 = SMS OTP, 2 = App OTP
               // string otp = data.otp;
                //string serviceKey = data.serviceKey;
                string reason = data.reason;
                int isAgencyWeb = 0;
                try
                {
                    isAgencyWeb = data.isAgencyWeb;
                }
                catch (Exception e)
                {
                    isAgencyWeb = 0;
                }

                int isOtp = 0;
                try
                {
                    isOtp = data.isOtp;
                }
                catch (Exception e)
                {
                    isOtp = 0;
                }
                if (isAgencyWeb == 0)
                {
                    string captchaText = data.captchaText;
                    string captchaToken = data.captchaToken;
                    NLogManager.Info("VerifyCaptcha:" + captchaText + "|" + captchaToken);

                    if (_captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);
                }
                if (nickNameTrans.CompareTo(nickNameRecv) == 0)
                    return new ResponseBuilder(ErrorCodes.CAN_NOT_TRANSFER_TO_YOURSELF, _accountSession.Language);

                if (transferValue < _MIN_BON || transferValue > _MAX_BON)
                    return new ResponseBuilder(ErrorCodes.BON_VALUE_INVALID, _accountSession.Language);

                // Check tk nhận tiền
                int res = _accountDAO.GetAccountByNickName(nickNameRecv, out string userNameRecv, out int accountRecvId);
                if (res <= 0)
                    return new ResponseBuilder(ErrorCodes.RECEIVER_ACCOUNT_NOT_EXIST, _accountSession.Language);
                long balanceReceive = 0;
                long receiveAmount = 0;
                long balance = _coreAPI.Transfer(accountIdTrans, nickNameTrans, transferValue, accountRecvId, nickNameRecv, transferType, ipAddress, sourceId, reason, isAgencyWeb, out balanceReceive, out receiveAmount);
                NLogManager.Info(string.Format("Tranfer accountIdTrans = {0}, nickNameTrans = {1}, transferValue = {2}, nickNameRecv = {3}, reason = {4}, status = {5}, sourceId = {6}",
                accountIdTrans, nickNameTrans, transferValue, nickNameRecv, reason, balance, sourceId));
                if (balance >= 0)
                {
                    string notificationSend = string.Format("Ngày [{0}] tài khoản [{1}] Chuyển cho bạn số tiền là: {2:0,0}", DateTime.Now.ToString("dd/MM HH:mm:ss"), nickNameTrans, receiveAmount);
                    _coreAPI.SendPopup(accountRecvId, notificationSend, balanceReceive, 1);

                    string message = String.Format("Giao dịch CK số tiền -{0:0,0} cho TK [{1}] lúc {2}. Số dư {3:0,0}. Nội dung: {4}", transferValue, nickNameRecv, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), balance, reason);
                    _otp.PushMessageToTelegram(accountIdTrans, "", message);
                    message = String.Format("TK {0} Chuyển khoản +{1:0,0} lúc {2}. Nội dung: {3}. Số dư hiện tại là: {4:0,0}", nickNameTrans, receiveAmount, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), reason, balanceReceive);
                    _otp.PushMessageToTelegram(accountRecvId, "", message);

                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, balance.ToString());
                }
                else
                {
                    if (balance == -51)
                    {
                        return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_ENOUGH_MONEY, _accountSession.Language);
                    }
                    else if (balance == -50)
                    {
                        NLogManager.Info("-50");
                        return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_EXIST_ACCOUNT, _accountSession.Language);
                    }
                    else if (balance == -1001)
                    {
                        NLogManager.Info("-1001");
                        return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_SOURCEACCOUNT_IS_AGENCY, _accountSession.Language);
                    }
                    else if (balance == -100)
                    {
                        NLogManager.Info("-100");
                        return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_EXIST_SERVICEID, _accountSession.Language);
                    }
                    else if (balance == -60)
                    {
                        NLogManager.Info("-60");
                        return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_PARTNER_MONEYBETTING_INVALID_MONEY, _accountSession.Language);
                    }
                    else if (balance == -99)
                    {
                        NLogManager.Info("-99");
                        return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_TRANSACTIONS, _accountSession.Language);
                    }
                    else if (balance == -109)
                    {
                        NLogManager.Info("-109");
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_NOT_ENOUGH_24H, _accountSession.Language);
                    }
                    else
                    {
                        NLogManager.Info("1047");
                        return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Code: " + balance);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Chuyển khoản không thành công, response = " + status);
        }

        [Authorize]
        [HttpPost("getTransferRate")]
        public ActionResult<ResponseBuilder> GetTransferRate([FromBody]dynamic data)
        {
            try
            {
                if (_accountSession.AccountID < 1)
                    return new ResponseBuilder(ErrorCodes.TOKEN_ERROR, _accountSession.Language);

                string nickNameTrans = _accountSession.NickName;
                string accountNameTrans = _accountSession.AccountName;
                string nickNameRecv = data.nickNameRecv;

                if (nickNameRecv == null | nickNameRecv.Length < 1)
                    return new ResponseBuilder(ErrorCodes.NICKNAME_INVALID, _accountSession.Language);
                if (nickNameTrans.CompareTo(nickNameRecv) == 0)
                    return new ResponseBuilder(ErrorCodes.CAN_NOT_TRANSFER_TO_YOURSELF, _accountSession.Language);

                double rate = 0;
                int res = _coreAPI.GetTransferRate(nickNameTrans, nickNameRecv, out rate);

                if (res == (int)ErrorCodes.SUCCESS)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, rate);
                else
                    return new ResponseBuilder(res, _accountSession.Language);
            }
            catch (Exception ex)
            {
                //NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        /// <summary>
        /// API cho Report: Đăng ký OTP
        /// </summary>
        /// <param name="model">
        /// model.Type: 1--> Đăng ký OTP; 2--> Hủy OTP;
        ///
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost("ReportRegisterOTP")]
        public ActionResult<ResponseBuilder> RegisterSecurityReport([FromBody] RegisterSecurityModels model)
        {
            if (model == null)
                return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

            try
            {
                if (!PolicyUtil.IsServiceKeyOk(model.ServiceKey, (int)_accountSession.AccountID))
                    return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);

                //string captcha = model.Captcha;
                //string captchaToken = model.CaptchaToken;

                //if(string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || CaptchaUtil.VerifyCaptcha(captcha, captchaToken) < 0)
                //    return new ResponseBuilder(ErrorCodes.BadRequest, "Mã xác nhận không đúng, vui lòng thử lại", "BadRequest");

                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo((int)_accountSession.AccountID);
                if (securityInfo != null)
                {
                    if (model.Type == 1) // Đăng ký bảo mật bằng OTP
                    {
                        if (securityInfo.IsMobileActived)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_ALREADY_REGISTER_OTP, _accountSession.Language);

                        //if(securityInfo.IsEmailActived)
                        //    return new ResponseBuilder(ErrorCodes.BadRequest, "Bạn đã đăng ký bảo mật bằng Email, để đăng ký bảo mật bằng SMS bạn cần hủy bảo mật bằng Email!", "BadRequest");

                        if (string.IsNullOrEmpty(securityInfo.Mobile))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                        if (string.IsNullOrEmpty(model.SecureCode))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_SERCURE_CODE_INVALID, _accountSession.Language);

                        if (_otp.CheckOTP((int)_accountSession.AccountID, _accountSession.AccountName, model.SecureCode, 1, (int)OtpType.OTP_REGISTER) != OTPCode.OTP_VERIFY_SUCCESS)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language);

                        int res = _accountDAO.RegisterOTP((int)_accountSession.AccountID, _accountSession.AccountName, securityInfo.Mobile, (int)RegisterOTPType.REGISTER_OTP);
                        if (res > 0)
                        {
                            securityInfo = _accountDAO.GetSecurityInfo((int)_accountSession.AccountID);
                            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, securityInfo);
                        }
                    }
                    else if (model.Type == 2) // Huy OTP
                    {
                        if (!securityInfo.IsMobileActived)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER_YET, _accountSession.Language);

                        //if(securityInfo.IsEmailActived)
                        //    return new ResponseBuilder(ErrorCodes.BadRequest, "Bạn đã đăng ký bảo mật bằng Email, để đăng ký bảo mật bằng SMS bạn cần hủy bảo mật bằng Email!", "BadRequest");

                        if (string.IsNullOrEmpty(securityInfo.Mobile))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                        if (model.Mobile.CompareTo(securityInfo.Mobile) != 0)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_NOT_MATCH, _accountSession.Language);

                        if (string.IsNullOrEmpty(model.SecureCode))
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_SERCURE_CODE_INVALID, _accountSession.Language);

                        if (_otp.CheckOTP((int)_accountSession.AccountID, _accountSession.AccountName, model.SecureCode, 1, (int)OtpType.OTP_UNREGISTER) != OTPCode.OTP_VERIFY_SUCCESS)
                            return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                        int res = _accountDAO.RegisterOTP((int)_accountSession.AccountID, _accountSession.AccountName, securityInfo.Mobile, (int)RegisterOTPType.UNREGISTER_OTP);

                        if (res > 0)
                        {
                            securityInfo = _accountDAO.GetSecurityInfo((int)_accountSession.AccountID);
                            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, securityInfo);
                        }
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_CANCEL_FAILED, _accountSession.Language);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        // API cho Report
        [HttpGet("GetAccountInfo")]
        public ActionResult<ResponseBuilder> GetSecurityInfo(int accountId, string serviceKey)
        {
            try
            {
                if (accountId < 0)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                }

                if (PolicyUtil.IsServiceKeyOk(serviceKey, accountId))
                {
                    var securityInfo = _otp.GetSucurityInfoWithMask(accountId, false);
                    if (securityInfo != null)
                    {
                        return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, securityInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [HttpPost("UpdateUserFullName")]
        public ActionResult<ResponseBuilder> UpdateUserFullName(string accountName, long accountId, string fullName, string serviceKey)
        {
            try
            {
                //if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || (CaptchaUtil.VerifyCaptcha(captcha, captchaToken) < 0))
                //    return new ResponseBuilder(ErrorCodes.BadRequest, "Mã xác nhận không hợp lệ", "Captcha invalid");

                //if (AccountSession.AccountID < 0 || AccountSession.AccountID != accountId)
                //    return new ResponseBuilder(ErrorCodes.Unauthorized, "Bạn chưa đăng nhập!", "Unauthorized");

                //if (!string.IsNullOrEmpty(AccountSession.AccountFullName))
                //    return new ResponseBuilder(ErrorCodes.BadRequest, "Tên nhân vật của bạn đã được cập nhật.", "BadRequest");
                if (!PolicyUtil.IsServiceKeyOk(serviceKey, (int)accountId))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_UPDATE_FAILED, _accountSession.Language);
                //return new ResponseBuilder(ErrorCodes.BadRequest, "Cập nhật NickName không thành công. Vui lòng liên hệ nhà phát hành", "BadRequest");

                if (string.IsNullOrEmpty(accountName) || accountId < 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NAME_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(fullName) || !PolicyUtil.CheckNickName(fullName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_INVALID, _accountSession.Language);

                //if (fullName.ToLower().CompareTo(AccountSession.Mobile.ToLower()) == 0)
                //    return new ResponseBuilder(ErrorCodes.BadRequest, "Tên nhân vật không được trùng tên tài khoản.", "BadRequest");

                //string cookieUsername = AccountSession.SessionName(account.AccountID, account.UserName, account.MerchantID, account.PlatformID, account.UserFullname);
                var accountInfo = _accountDAO.GetAccount(accountId, accountName);
                if (accountInfo == null)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                }

                //if (fullName.ToLower().CompareTo(accountInfo.UserFullname.ToLower()) == 0)
                //    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_MUST_DEFFENT_ACCNAME, _accountSession.Language);

                if (!string.IsNullOrEmpty(accountInfo.UserFullname))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_UPDATED, _accountSession.Language);

                int response = _accountDAO.UpdateUserFullName(accountName, accountId, fullName);
                if (response >= 0)
                {
                    AccountDb accountDb = new AccountDb();
                    accountDb.AccountID = _accountSession.AccountID;
                    accountDb.UserName = _accountSession.AccountName;
                    accountDb.UserFullname = fullName;
                    accountDb.MerchantID = _accountSession.MerchantID;
                    accountDb.PlatformID = _accountSession.PlatformID;
                    accountDb.ClientIP = _accountSession.IpAddress;

                    //CoreAPI.Instance().SetAuthCookie(accountDb);

                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, "Cập nhật tên nhân vật thành công");
                }
                else if (response == -50)
                    return new ResponseBuilder(ErrorCodes.NICKNAME_EXIST, _accountSession.Language);

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        /// <summary>
        /// Kiểm tra thẻ có bảo trì hay không
        /// </summary>
        /// <param name="cardType">VTT, VMS, VNP, GATE</param>
        /// <param name="topupType">0 = Nạp thẻ, 3 = Mua thẻ</param>
        /// <returns>Thẻ OK: HttpCode = 200, Thẻ bảo trì: HttpCode != 200</returns>
        [Authorize]
        [HttpGet("CheckCardMaintain")]
        public ActionResult<ResponseBuilder> CheckCardMaintain(string cardType, int topupType)
        {
            try
            {
                string res = _coreAPI.CheckCardMaintain(cardType, topupType);
                if (string.IsNullOrEmpty(res))
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return new ResponseBuilder(ErrorCodes.SERVER_MAINTAIN, _accountSession.Language);
        }
    }
}
