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
using System.Text.RegularExpressions;
using ServerCore.Utilities;
using System.Security.Cryptography;
using ServerCore.Utilities.Interfaces;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Caching.Distributed;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Dynamic;

namespace PortalAPI.Controllers
{
    [Route("Report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly OTPSecurity _otp;
        private readonly IAccountDAO _accountDAO;
        private readonly IReportDAO _reportDAO;
        private readonly AccountSession _accountSession;
        private readonly CoreAPI _coreAPI;
		private readonly IDataService _dataService;

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
		private readonly IDistributedCache _cache;

		public ReportController(IAuthenticateService authenticateService, AccountSession accountSession, OTPSecurity otp, IAccountDAO accountDAO, CoreAPI coreAPI, IOptions<AppSettings> options, Captcha captchaUtil, IReportDAO reportDAO, IDataService dataService,IDistributedCache cache)
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
            _captcha = captchaUtil;
            _reportDAO = reportDAO;
            _dataService = dataService;
            _cache = cache;
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
            if (_accountSession.AccountID <=0 || _accountSession.NickName == null)
                return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Sai mã token" );
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
                int otpType = data.otpType; // 1 = SMS OTP, 2 = App OTP
                string otp = data.otp;
                string serviceKey = data.serviceKey;
                string reason = data.reason;
                string deviceid = "";
                try
                {
                    deviceid = data.deviceid;
                }
                catch (Exception e)
                {
                    deviceid = "";
                }
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
             

                if (!PolicyUtil.IsServiceKeyOk(serviceKey, accountIdTrans))
                    return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);

                if (nickNameTrans.CompareTo(nickNameRecv) == 0)
                    return new ResponseBuilder(ErrorCodes.CAN_NOT_TRANSFER_TO_YOURSELF, _accountSession.Language);

                if (transferValue < _MIN_BON || transferValue > _MAX_BON)
                    return new ResponseBuilder(ErrorCodes.BON_VALUE_INVALID, _accountSession.Language);

                // Check tk nhận tiền
                int res = _accountDAO.GetAccountByNickName(nickNameRecv, out string userNameRecv, out int accountRecvId);
                if(res <= 0) return new ResponseBuilder(ErrorCodes.RECEIVER_ACCOUNT_NOT_EXIST, _accountSession.Language);

                var securityInfo = _accountDAO.GetSecurityInfo(_accountSession.AccountID);

                // Tài khoản chưa đăng ký bảo mật OTP
                if (securityInfo == null) return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER, _accountSession.Language);
                if (securityInfo.Mobile == null || securityInfo.Mobile.Length < 1 || (!securityInfo.IsMobileActived)) return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_INVALID, _accountSession.Language);


                if (_otp.IsOTP(accountIdTrans))
                {
                    //string serviceData = JsonConvert.SerializeObject(new TransferData
                    //{
                    //    AccountIdTrans = accountIdTrans,
                    //    IpAddress = ipAddress,
                    //    NickNameRecv = nickNameRecv,
                    //    NickNameTrans = nickNameTrans,
                    //    Reason = reason,
                    //    SourceId = sourceId,
                    //    TransferType = transferType,
                    //    TransferValue = transferValue
                    //});

                    //string token = OTPSecurity.CreateOTPToken(accountNameTrans, accountIdTrans, (int)OtpService.OTP_SERVICE_TRANSFER, sourceId, serviceData);
                    //return new ResponseBuilder(ErrorCodes.OK, JsonConvert.SerializeObject(new OTPDataReturn(token)), "OK");
                    //                    DECLARE @_PROCESS_SUCCESS INT = 0;
                    //                    DECLARE @_ERR_TRANSACTIONS INT = -99
                    //                      DECLARE @_ERR_PARTNER_MONEYBETTING_INVALID_MONEY INT = -60;
                    //                    --Số tiền không hợp lệ
                    //                      DECLARE @_ERR_NOT_EXIST_SERVICEID INT = -100--Cap ServiceID/ Key không ton tai
                    //                      DECLARE @_ERR_SOURCEACCOUNT_IS_AGENCY INT = -1001-- TAI KHOAN CHUYEN DA LA DAI LY
                    //                      DECLARE @_ERR_NOT_EXIST_ACCOUNT INT = -50-- - 51 TAI KHOAN KHONG TON TAI
                    //                      DECLARE @_ERR_NOT_ENOUGH_MONEY INT = -51-- - 51 TAI KHOAN KHONG DU TIEN

                    //NLogManager.Info(string.Format("accountIdTrans = {0}, nickNameTrans = {1}, transferValue = {2}, nickNameRecv = {3}, reason = {4}, status = {5}, sourceId = {6}",
                    //accountIdTrans, nickNameTrans, transferValue, nickNameRecv, reason, status, sourceId));

                    var checkOTP = _otp.CheckOTP((int)accountIdTrans, accountNameTrans, otp, otpType, (int)OtpType.OTP_TRANSFER);
                    if (isOtp > 0)
                    {
                        checkOTP = OTPCode.OTP_VERIFY_SUCCESS;
                    }

                    if (checkOTP == OTPCode.OTP_VERIFY_SUCCESS)
                    {
                        long balanceReceive = 0;
                        long receiveAmount = 0;
                        long balance = _coreAPI.Transfer(accountIdTrans, nickNameTrans, transferValue, accountRecvId, nickNameRecv, transferType, ipAddress, sourceId, reason, isAgencyWeb, deviceid, out balanceReceive, out receiveAmount);
                        NLogManager.Info(string.Format( "Tranfer accountIdTrans = {0}, nickNameTrans = {1}, transferValue = {2}, nickNameRecv = {3}, reason = {4}, status = {5}, sourceId = {6}",
                        accountIdTrans, nickNameTrans, transferValue, nickNameRecv, reason, balance, sourceId));
                        if (balance >= 0)
                        {
                            string notificationSend = string.Format("Ngày [{0}] tài khoản [{1}] Chuyển cho bạn số tiền là: {2:0,0} Nội dung {3}", DateTime.Now.ToString("dd/MM HH:mm:ss"), nickNameTrans, receiveAmount, reason);
                            _coreAPI.SendPopup(accountRecvId, notificationSend, balanceReceive, 1);

                            string message = String.Format("Ngày {0} bạn chuyển số tiền -{1:0,0} cho TK [{2}] với nội dung:{3} Số dư {4:0,0}.", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), transferValue, nickNameRecv, reason, balance);
                            _otp.PushMessageToTelegram(accountIdTrans, "", message);
                            message = String.Format("Ngày {0} tài khoản {1} Chuyển khoản cho bạn số tiền: +{2:0,0}. Nội dung: {3}. Số dư hiện tại là: {4:0,0}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), nickNameTrans, receiveAmount, reason, balanceReceive);
                            _otp.PushMessageToTelegram(accountRecvId, "", message);

                            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, balance.ToString());
                        }
                        else
                        {
                            if (balance == -51)
                            {
                                // message = "Tài khoản không đủ số dư.";
                                return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_ENOUGH_MONEY, _accountSession.Language);
                            }
                            else if (balance == -50)
                            {
                                // message = "Tài khoản không tồn tại.";
                                NLogManager.Info("-50");
                                return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_EXIST_ACCOUNT, _accountSession.Language);
                            }
                            else if (balance == -1001)
                            {
                                // message = "TAI KHOAN CHUYEN DA LA DAI LY";
                                NLogManager.Info("-1001");
                                return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_SOURCEACCOUNT_IS_AGENCY, _accountSession.Language);
                            }
                            else if (balance == -100)
                            {
                                // message = "Không tồn tại mã dịch vụ.";
                                NLogManager.Info("-100");
                                return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_EXIST_SERVICEID, _accountSession.Language);
                            }
                            else if (balance == -60)
                            {
                                // message = "Số tiền không hợp lệ.";
                                NLogManager.Info("-60");
                                return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_PARTNER_MONEYBETTING_INVALID_MONEY, _accountSession.Language);
                            }
                            else if (balance == -59)
                            {
                                // message = "Số tiền min 20k.";
                                NLogManager.Info("-59");
                                return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_MIN_20K, _accountSession.Language);
                            }
                            else if (balance == -61)
                            {
                                // message = "Số tiền không hợp lệ.";
                                NLogManager.Info("-60");
                                return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_INVALID, _accountSession.Language);
                            }
                            else if (balance == -99)
                            {
                                // message = "Lỗi hệ thống.";
                                NLogManager.Info("-99");
                                return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_TRANSACTIONS, _accountSession.Language);
                            }
                            else if (balance == -109)
                            {
                                // message = "Số điện thoại của bạn kích hoạt chưa đủ 24h.";
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
                    //return new ResponseBuilder(ErrorCodes.BadRequest, "Mã OTP không đúng", "BadRequest");
                }
                return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language, "Bạn cần đăng ký bảo mật OTP để thực hiện chức năng này!");
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
                
                if (nickNameRecv == null | nickNameRecv.Length<1)
                    return new ResponseBuilder(ErrorCodes.NICKNAME_INVALID, _accountSession.Language);
                if (nickNameTrans.CompareTo(nickNameRecv) == 0)
                    return new ResponseBuilder(ErrorCodes.CAN_NOT_TRANSFER_TO_YOURSELF, _accountSession.Language);

                double rate = 0;
                int res = _coreAPI.GetTransferRate(nickNameTrans, nickNameRecv, out rate);

                if(res == (int) ErrorCodes.SUCCESS)
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

                //if(string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || Captcha.VerifyCaptcha(captcha, captchaToken) < 0)
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
                        long balance = 0;
                        int res = _accountDAO.RegisterOTP((int)_accountSession.AccountID, _accountSession.AccountName, securityInfo.Mobile, (int)RegisterOTPType.REGISTER_OTP, out balance);
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
                        long balance = 0;
                        int res = _accountDAO.RegisterOTP((int)_accountSession.AccountID, _accountSession.AccountName, securityInfo.Mobile, (int)RegisterOTPType.UNREGISTER_OTP, out balance);

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
                //if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(captchaToken) || (Captcha.VerifyCaptcha(captcha, captchaToken) < 0))
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

                if (fullName.ToLower().CompareTo(accountInfo.UserFullname.ToLower()) == 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_CHARACTER_NAME_MUST_DEFFENT_ACCNAME, _accountSession.Language);

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
        [Authorize]
        [HttpGet("GetUserRevenueAffilicate")]
        public ActionResult<ResponseBuilder> GetUserRevenueAffilicate(int typeTime = 1)
        {
            try
            {
                if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
                    return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Sai mã token");
                var ToDay = DateTime.Now;
                DateTime fromDate, toDate;
                
                if (typeTime == 1) //Hôm nay
                {
                    fromDate = ToDay;
                    toDate = ToDay;
                }
                else if (typeTime == 2) //Hôm qua
                {
                    fromDate = ToDay.AddDays(-1);
                    toDate = ToDay.AddDays(-1);
                }
                else if (typeTime == 3) //Tuần này
                {
                    DayOfWeek currentDay = DateTime.Now.DayOfWeek;
                    int daysTillCurrentDay = currentDay - DayOfWeek.Monday;
                    DateTime currentWeekStartDate = DateTime.Now.AddDays(-daysTillCurrentDay);
                    fromDate = currentWeekStartDate;
                    toDate = ToDay;
                }
                else if (typeTime == 4) //Tuần trước
                {
                    DayOfWeek currentDay = DateTime.Now.DayOfWeek;
                    int daysTillCurrentDay = currentDay - DayOfWeek.Monday;
                    DateTime lastWeekStartDate = DateTime.Now.AddDays(-7-daysTillCurrentDay);
                    fromDate = lastWeekStartDate;
                    toDate = lastWeekStartDate.AddDays(6);
                }
                else if (typeTime == 5) //tháng này
                {
                    var firstDayMonth = new DateTime(ToDay.Year, ToDay.Month, 1);
                    fromDate = firstDayMonth;
                    toDate = ToDay;
                }
                else if (typeTime == 6) //tháng trước
                {
                    var lastMonth = ToDay.AddMonths(-1);
                    var firstDayMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    int lastDayMonth = System.DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
                    fromDate = firstDayMonth;
                    toDate = new DateTime(lastMonth.Year, lastMonth.Month,lastDayMonth);
                }
                else
                {
                    fromDate = ToDay;
                    toDate = ToDay;
                }
                var res = _reportDAO.GetUserRevenueAffilicate(_accountSession.AccountID, Convert.ToInt32(fromDate.ToString("yyyyMMdd")), Convert.ToInt32(toDate.ToString("yyyyMMdd")));
                if (res!=null)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language,res);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

		[Authorize]
		[HttpGet("GetInfoRevenue")]
		public ActionResult<ResponseBuilder> GetInfoRevenue()
		{
			try
			{
				if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
					return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Sai mã token");
				var ToDay = DateTime.Now;
				DateTime fromDate, toDate;

				fromDate = ToDay;
			    toDate = ToDay;

				var res = _reportDAO.GetInfoRevenueAffiliate(_accountSession.AccountID, Convert.ToInt32(fromDate.ToString("yyyyMMdd")), Convert.ToInt32(toDate.ToString("yyyyMMdd")));
				if (res != null)
					return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, res);
			}
			catch (Exception e)
			{
				NLogManager.Exception(e);
			}
			return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
		}
		[Authorize]
		[HttpGet("GetHistoryDeductRevenue")]
		public ActionResult<ResponseBuilder> GetHistoryDeductRevenue()
		{
			try
			{
				if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
					return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Sai mã token");
				
				var res = _reportDAO.GetHistoryDeductRevenue(_accountSession.AccountID);
				if (res != null)
					return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, res);
			}
			catch (Exception e)
			{
				NLogManager.Exception(e);
			}
			return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
		}
		[Authorize]
		[HttpGet("GetReferCode")]
		public ActionResult<ResponseBuilder> GetReferCode(string domain)
		{
			try
			{
				if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
					return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Sai mã token");

                var url = string.Concat(domain,"?code=", _accountSession.AccountID.ToString());
                var res = new RefererCodeInfo()
                {
                    RefererCode = _accountSession.AccountID.ToString()
                    ,
                    RefererUrl = url
                };

				if (res != null)
					return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, res);
			}
			catch (Exception e)
			{
				NLogManager.Exception(e);
			}
			return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
		}
		[Authorize]
		[HttpPost("WithdrawAffiliate")]
		public ActionResult<ResponseBuilder> WithdrawAffiliate([FromBody] dynamic data)
		{
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
				int sourceId = data.sourceId; // 4 = web
				int transferType = 1;//data.transferType; // 1 = Chuyen TotalCoin, 2 = chuyen Xu
				string deviceid = "";
				try
				{
					deviceid = data.deviceid;
				}
				catch (Exception e)
				{
					deviceid = "";
				}
				int isOtp = 0;
				if (transferValue < _MIN_BON || transferValue > _MAX_BON)
					return new ResponseBuilder(ErrorCodes.BON_VALUE_INVALID, _accountSession.Language);

				var securityInfo = _accountDAO.GetSecurityInfo(_accountSession.AccountID);

				// Tài khoản chưa đăng ký bảo mật OTP
				if (securityInfo == null) return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER, _accountSession.Language);
				if (securityInfo.Mobile == null || securityInfo.Mobile.Length < 1 || (!securityInfo.IsMobileActived)) return new ResponseBuilder(ErrorCodes.ACCOUNT_MOBILE_INVALID, _accountSession.Language);


				if (_otp.IsOTP(accountIdTrans))
				{
					long balanceNew = 0;
					long balance = _reportDAO.WithdrawAffiliate(accountIdTrans, nickNameTrans, transferValue,ipAddress, sourceId, out balanceNew);
					NLogManager.Info(string.Format("Withdraw affiliate accountIdTrans = {0}, nickNameTrans = {1}, transferValue = {2}, ipAddress = {3}, sourceId = {4}",
					accountIdTrans, nickNameTrans, transferValue, ipAddress, sourceId));
					if (balance >= 0)
					{
						return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, new { BalanceNew = balanceNew});
					}
					else
					{
						if (balance == -51)
						{
							// message = "Tài khoản không đủ số dư.";
							return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_ENOUGH_MONEY, _accountSession.Language);
						}
						else if (balance == -50)
						{
							// message = "Tài khoản không tồn tại.";
							NLogManager.Info("-50");
							return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_EXIST_ACCOUNT, _accountSession.Language);
						}
						else if (balance == -1001)
						{
							// message = "TAI KHOAN CHUYEN DA LA DAI LY";
							NLogManager.Info("-1001");
							return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_SOURCEACCOUNT_IS_AGENCY, _accountSession.Language);
						}
						else if (balance == -100)
						{
							// message = "Không tồn tại mã dịch vụ.";
							NLogManager.Info("-100");
							return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_EXIST_SERVICEID, _accountSession.Language);
						}
						else if (balance == -60)
						{
							// message = "Số tiền không hợp lệ.";
							NLogManager.Info("-60");
							return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_PARTNER_MONEYBETTING_INVALID_MONEY, _accountSession.Language);
						}
						else if (balance == -59)
						{
							// message = "Số tiền min 20k.";
							NLogManager.Info("-59");
							return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_MIN_20K, _accountSession.Language);
						}
						else if (balance == -61)
						{
							// message = "Số tiền không hợp lệ.";
							NLogManager.Info("-60");
							return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_INVALID, _accountSession.Language);
						}
						else if (balance == -99)
						{
							// message = "Lỗi hệ thống.";
							NLogManager.Info("-99");
							return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_TRANSACTIONS, _accountSession.Language);
						}
						else if (balance == -109)
						{
							// message = "Số điện thoại của bạn kích hoạt chưa đủ 24h.";
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
				return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language, "Bạn cần đăng ký bảo mật OTP để thực hiện chức năng này!");
			}
			catch (Exception ex)
			{
				NLogManager.Exception(ex);
			}
			return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Chuyển khoản không thành công, response = " + status);
		}
		[Authorize]
		[HttpGet("GetQRCode")]
		public ActionResult<ResponseBuilder> GetQRCode(int pixelsPerModule)
		{
			try
			{
                dynamic loginData = new ExpandoObject();
                loginData.userName = _appSettings.BlockchainUsername;
                loginData.password = _appSettings.BlockchainPassword;
                var resLogin = _dataService.PostAsync(_appSettings.BlockchainAuthenUrl + "api/Authen/Login", loginData, true).Result;
                var dataLogin = JsonConvert.DeserializeObject<dynamic>(resLogin).data;
                var res = _dataService.GetApiAsync(_appSettings.BlockchainApiUrl + "api/Blockchain/GetWallet?accountID="+ dataLogin.accountID +"&callback=1", dataLogin.accessToken, true,true).Result;
                var data = JsonConvert.DeserializeObject<dynamic>(res);
				if (data.address != null)
                {
					QRCodeGenerator qrGenerator = new QRCodeGenerator();
					QRCodeData qrCodeData = qrGenerator.CreateQrCode(data.address.ToString(), QRCodeGenerator.ECCLevel.Q);
					QRCode qrCode = new QRCode(qrCodeData);
					Bitmap qrCodeImage = qrCode.GetGraphic(pixelsPerModule);
                    MemoryStream stream = new MemoryStream();
                    
					qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    var aabb = stream.ToArray();
					var base64String = Convert.ToBase64String(stream.ToArray());
					return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, new { AddressBase64String = base64String, Address = data.address });
				}
			}
			catch (Exception e)
			{
				NLogManager.Exception(e);
			}
			return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
		}
	}
}
