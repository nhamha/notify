using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using GameServer.Utilities;
using System.Configuration;
using PortalAPI.Libs;
using System.Text;
using PortalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Utils;
using ServerCore.DataAccess.DTO;
using Microsoft.AspNetCore.Authorization;
using ServerCore.Utilities.Models;
using ServerCore.PortalAPI.OTP;
using ServerCore.Utilities.Security;
using ServerCore.Utilities.Sessions;
using ServerCore.PortalAPI.Services;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using ServerCore.Utilities.Captcha;

namespace PortalAPI.Controllers
{
    [Route("OTPAPI")]
    [ApiController]
    public class OTPAPIController : ControllerBase
    {
        private readonly IOTPDAO _iOTPDAO;
        private readonly IAccountDAO _accountDAO;
        private readonly IAgencyDAO _agencyDao;
        private readonly AccountSession _accountSession;
        private readonly OTPSecurity _otpSecurity;
        private readonly IAuthenticateService _authenticateService;
        private readonly Captcha _captcha;

        public OTPAPIController(IOTPDAO iOTPDAO, IAccountDAO accountDAO, IAgencyDAO agencyDAO, AccountSession accountSession, OTPSecurity otpSecurity, IAuthenticateService authenticateService, Captcha captcha)
        {
            this._iOTPDAO = iOTPDAO;
            this._accountDAO = accountDAO;
            this._agencyDao = agencyDAO;
            this._accountSession = accountSession;
            this._otpSecurity = otpSecurity;
            this._authenticateService = authenticateService;
            _captcha = captcha;
        }

        /// <summary>
        /// đăng ký nhận otp khi login - trả cho client verify code
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("SetupAppToken")]
        public ActionResult<ResponseBuilder> SetupAppToken(OTPs.SetupAppTokenInput input)
        {
            NLogManager.Info(JsonConvert.SerializeObject(input));

            int _iAccountID = 0;
            string _strAccountName = string.Empty;
            OtpAppLib otpLib = new OtpAppLib();
            try
            {
                _iAccountID = otpLib.TokenGetAccountID(input.AccountToken);
                _strAccountName = otpLib.TokenGetAccountName(input.AccountToken);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Constants.TOKEN_PARSE_ERROR_CODE;
                responMes.Mes = Constants.TOKEN_PARSE_ERROR_MESSAGE;
                return Ok(responMes);
            }

            //  check accountToken time
            if (input.Status != 9 && !otpLib.TokenCheckTime(input.AccountToken))
            {
                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Constants.SESSION_EXPRIED_CODE;
                responMes.Mes = Constants.SESSION_EXPRIED_MESSAGE;
                return Ok(responMes);
            }

            //  GET MOBILE
            string _strMobile = otpLib.TokenGetMobile(input.AccountToken);
            if (string.IsNullOrEmpty(_strMobile) || _strMobile.Length < 9)
            {
                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(_iAccountID);
                if (securityInfo != null && securityInfo.IsMobileActived && !string.IsNullOrEmpty(securityInfo.Mobile))
                    _strMobile = securityInfo.Mobile;
                else
                {
                    OTPs.ResponMes responMes = new OTPs.ResponMes
                    {
                        Mes = "Số điện thoại không hợp lệ",
                        ResponCode = -29
                    };
                    return Ok(responMes);
                }
            }

            try
            {
                OTPs.SetupAppTokenOutput _dataOutput = new OTPs.SetupAppTokenOutput();

                //_dataOutput.SystemID = int.Parse(ConfigurationManager.AppSettings["SystemID"]);
                //_dataOutput.ServiceID = int.Parse(ConfigurationManager.AppSettings["SERVICEID_OTP_APP"]);
                //_dataOutput.SystemUserName = _strAccountName;
                //_dataOutput.MinAmount = int.Parse(ConfigurationManager.AppSettings["MinAmount"]);

                _dataOutput.AccountID = _iAccountID;
                _dataOutput.Mobile = _strMobile;// input.Mobile;
                _dataOutput.Status = input.Status;
                _dataOutput.TokenKey = input.TokenKey;
                _dataOutput.Description = input.Description;
                _dataOutput.otp = input.otp;
                _dataOutput.VerifyCode = input.VerifyCode;

                NLogManager.Info("SetupAppToken_input : " + JsonConvert.SerializeObject(_dataOutput));

                if (_dataOutput.Status == 1)
                {
                    if (_dataOutput.otp <= 0 || string.IsNullOrEmpty(_dataOutput.VerifyCode))
                    {
                        OTPs.ResponMes responMes = new OTPs.ResponMes();
                        responMes.ResponCode = Constants.INPUT_ERROR_CODE;
                        responMes.Mes = Constants.INPUT_ERROR_MESSAGE;
                        return Ok(responMes);
                    }
                    else
                    {
                        //  check AccountVerifyOTP
                        OTPs.AccountVerifyOTPInput _accountVerifyOtpInput = new OTPs.AccountVerifyOTPInput();
                        _accountVerifyOtpInput.AccountID = _iAccountID;
                        _accountVerifyOtpInput.Username = _strAccountName;
                        _accountVerifyOtpInput.UserSMS = _strMobile;//input.Mobile;
                        _accountVerifyOtpInput.TYPE = 2;
                        _accountVerifyOtpInput.CodeVerifying = input.otp;

                        //int iResultSql = _iOTPDAO.AccountVerifyOTP(_accountVerifyOtpInput);
                        OTPCode code = _otpSecurity.CheckOTP(_iAccountID, _strAccountName, input.otp.ToString(), 1, 2);
                        //if(code != OTPCode.OTP_VERIFY_SUCCESS)
                        //    return HttpUtils.CreateResponse(HttpStatusCode.BadRequest, "Mã OTP không đúng.", "BadRequest");

                        if (code == OTPCode.OTP_VERIFY_SUCCESS)
                        {
                            //  Gửi lại bản tin lần 2
                            OTPs.ResponMes responMes = _iOTPDAO.SetupAppToken(_dataOutput);
                            if (responMes.ResponCode >= 0)
                            {
                                responMes.Mes = "Đăng ký thành công !";
                            }
                            else if (responMes.ResponCode == -12)
                            {
                                responMes.Mes = "Hết hạn !";
                            }
                            else if (responMes.ResponCode == -29)
                            {
                                responMes.Mes = "SĐT không được để trống !";
                            }
                            else if (responMes.ResponCode == -30)
                            {
                                responMes.Mes = "TokenKey khong duoc de trong !";
                            }
                            else if (responMes.ResponCode == -43)
                            {
                                responMes.Mes = "Ma xac nhan khong hop le !";
                            }
                            else if (responMes.ResponCode == -48)
                            {
                                responMes.Mes = "TK bị block !";
                            }
                            else if (responMes.ResponCode == -50)
                            {
                                responMes.Mes = "Tài khoản không tồn tại !";
                            }
                            else if (responMes.ResponCode == -89)
                            {
                                responMes.Mes = "ServiceType khong hop le !";
                            }
                            else if (responMes.ResponCode == -144)
                            {
                                responMes.Mes = "Cập nhật trạng thái không đúng !";
                            }
                            else
                            {
                                responMes.ResponCode = -1;
                                responMes.Mes = "Đăng ký thất bại";
                            }
                            return Ok(responMes);
                        }
                        else
                        {
                            OTPs.ResponMes responMes = new OTPs.ResponMes();
                            responMes.ResponCode = -1;
                            responMes.Mes = "Xác thực tài khoản thất bại";
                            return Ok(responMes);
                        }
                    }
                }
                else if (_dataOutput.Status == 9)
                {
                    //  hủy đk otp app
                    //  check AccountVerifyOTP
                    OTPs.AccountVerifyOTPInput _accountVerifyOtpInput = new OTPs.AccountVerifyOTPInput();
                    _accountVerifyOtpInput.AccountID = _iAccountID;
                    _accountVerifyOtpInput.Username = _strAccountName;
                    _accountVerifyOtpInput.UserSMS = _strMobile;//input.Mobile;
                    _accountVerifyOtpInput.TYPE = 9;
                    _accountVerifyOtpInput.CodeVerifying = input.otp;

                    int iResultSql = 1;//_iOTPDAO.AccountVerifyOTP(_accountVerifyOtpInput); huy DK OTP app thi ko can check OTP

                    if (iResultSql > 0)
                    {
                        //  Gửi lại bản tin lần 2
                        OTPs.ResponMes responMes2 = _iOTPDAO.SetupAppToken(_dataOutput);
                        if (responMes2.ResponCode >= 0)
                        {
                            responMes2.Mes = "Hủy đăng ký OTP APP Thành Công!";
                        }
                        else
                        {
                            responMes2.Mes = "Hủy đăng ký OTP APP Thất Bại";
                        }
                        return Ok(responMes2);
                    }
                    else
                    {
                        OTPs.ResponMes responMes = new OTPs.ResponMes();
                        responMes.ResponCode = iResultSql;
                        responMes.Mes = "Xác thực tài khoản thất bại";
                        return Ok(responMes);
                    }

                }
                else if (_dataOutput.Status == 0)
                {
                    //  lần đầu gửi bản tin
                    OTPs.ResponMes responMes = _iOTPDAO.SetupAppToken(_dataOutput);
                    if (responMes.ResponCode >= 0)
                    {
                        //  GEN-KEY
                        responMes.OTPKey = Security.MD5Encrypt(_strMobile + "." + input.TokenKey + "." + responMes.ResponCode);
                        responMes.Mes = "Lấy thông tin thành công!";

                        // gửi mã xác thực qua email
                        //string email = Security.GetTokenEmail(input.AccountToken);
                        //this.SendEmail(email, _strAccountName, "");
                    }
                    else
                    {
                        responMes.ResponCode = -1;
                        responMes.Mes = "Lấy thông tin thất bại";
                    }

                    return Ok(responMes);
                }
                else
                {
                    OTPs.ResponMes responMes = new OTPs.ResponMes();
                    responMes.ResponCode = Constants.INPUT_ERROR_CODE;
                    responMes.Mes = Constants.INPUT_ERROR_MESSAGE;
                    return Ok(responMes);
                }
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Constants.EXCEPTION_CODE;
                responMes.Mes = Constants.EXCEPTION_MESSAGE;
                return Ok(responMes);
            }
        }


        /// <summary>
        /// kiểm tra khả dụng trong phiên login giữa client - server
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("CheckTokenAvailable")]
        public ActionResult<ResponseBuilder> CheckTokenAvailable(OTPs.TokensCheckActiveInput input)
        {
            try
            {
                //  GET MOBILE
                string _strMobile = new OtpAppLib().TokenGetMobile(input.AccountToken);

                OTPs.CheckActiveSyncTime _checkActive = new OTPs.CheckActiveSyncTime();
                _checkActive.SystemID = int.Parse(ConfigurationManager.AppSettings["SystemID"]);
                _checkActive.SystemUserName = input.SystemUserName;
                _checkActive.Mobile = _strMobile;// input.Mobile;
                _checkActive.TokenKey = input.TokenKey;
                _checkActive.VerifyCode = input.VerifyCode;
                _checkActive.ServiceID = int.Parse(ConfigurationManager.AppSettings["SERVICEID_OTP_APP"]);

                int iCheckActive = _iOTPDAO.TokensCheckActive(_checkActive);

                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = iCheckActive;
                if (responMes.ResponCode >= 0)
                {
                    responMes.Mes = "Active thành công !";
                }
                else
                {
                    responMes.Mes = "Active thất bại !";
                }
                return Ok(responMes);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Constants.EXCEPTION_CODE;
                responMes.Mes = Constants.EXCEPTION_MESSAGE;
                return Ok(responMes);
            }
        }

        /// <summary>
        /// đồng bộ thời gian client - server
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("SyncTime")]
        public ActionResult<ResponseBuilder> SyncTime(OTPs.CheckActiveSyncTimeInput input)
        {
            try
            {
                //  GET MOBILE
                string _strMobile = new OtpAppLib().TokenGetMobile(input.AccountToken);

                OTPs.CheckActiveSyncTime _activeSyncTime = new OTPs.CheckActiveSyncTime();
                _activeSyncTime.SystemID = 1;
                _activeSyncTime.AccountID = _accountSession.AccountID;
                _activeSyncTime.Mobile = _strMobile;//input.Mobile;
                _activeSyncTime.TokenKey = input.TokenKey;
                _activeSyncTime.ClientUnixTimestamp = input.ClientUnixTimestamp;

                int iResultSync = _iOTPDAO.TokensSyncTime(_activeSyncTime);

                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = iResultSync;
                if (responMes.ResponCode > 0)
                {
                    responMes.Mes = "Đồng bộ thành công";
                }
                else
                {
                    responMes.Mes = "Đồng bộ thất bại";
                }
                return Ok(responMes);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Constants.EXCEPTION_CODE;
                responMes.Mes = Constants.EXCEPTION_MESSAGE;
                return Ok(responMes);
            }
        }

        [HttpPost("SyncTimeTest")]
        public ActionResult<ResponseBuilder> SyncTimeTest(string mobile, string tokenKey, long time)
        {
            try
            {
                //  GET MOBILE
                OTPs.CheckActiveSyncTime _activeSyncTime = new OTPs.CheckActiveSyncTime();
                _activeSyncTime.SystemID = 2;
                _activeSyncTime.Mobile = mobile;//input.Mobile;
                _activeSyncTime.TokenKey = tokenKey;
                _activeSyncTime.ClientUnixTimestamp = time;

                int iResultSync = _iOTPDAO.TokensSyncTime(_activeSyncTime);

                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = iResultSync;
                if (responMes.ResponCode >= 0)
                {
                    responMes.Mes = "Đồng bộ thành công !";
                }
                else
                {
                    responMes.Mes = "Đồng bộ thất bại !";
                }
                return Ok(responMes);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Constants.EXCEPTION_CODE;
                responMes.Mes = Constants.EXCEPTION_MESSAGE;
                return Ok(responMes);
            }
        }

        /// <summary>
        /// update sđt
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// ResponCode nhỏ hơn 0 : show Mes | 
        /// ResponCode > 0 : đổi thông tin thành công = > trả về các thông tin
        /// public string PassPort { get; set; } |
        /// public string Email { get; set; } |
        /// public string Mobile { get; set; } |
        /// public bool IsMobileActived { get; set; } |
        /// public bool IsEmailActived { get; set; } |
        /// public bool IsOTP { get; set; } |
        /// public int ResponCode { get; set; } |
        /// </returns>
        [Authorize]
        [HttpPost("UpdateAccountInfo")]
        public ActionResult<ResponseBuilder> UpdateAccountInfo(OTPs.AccountUpdateInfoInput input)
        {
            int _iAccountID = 0;
            string _strAccountName = string.Empty;
            try
            {
                _iAccountID = new OtpAppLib().TokenGetAccountID(input.AccountToken);
                _strAccountName = new OtpAppLib().TokenGetAccountName(input.AccountToken);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language, "TOKEN_PARSE_ERROR_MESSAGE");
            }


            //  check accountToken time
            if (!new OtpAppLib().TokenCheckTime(input.AccountToken))
            {
                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Constants.SESSION_EXPRIED_CODE;
                responMes.Mes = Constants.SESSION_EXPRIED_MESSAGE;
                return Ok(responMes);
            }

            //  kiểm tra số lần gọi update thông tin của 1 tài khoản dựa trên IP trong 5s
            int iCountCache = CacheCounter.AccountIPActionCounter(_iAccountID.ToString(), "UpdateAccountInfo", 5, _accountSession.IpAddress);
            if (iCountCache > 3)
            {
                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Constants.LIMIT_ACTION_ERROR_CODE;
                responMes.Mes = Constants.LIMIT_ACTION_ERROR_MESSAGE;

                return Ok(responMes);
            }

            try
            {
                if (string.IsNullOrEmpty(input.otp))
                {
                    OTPs.UpdateInfo _updateInfo = new OTPs.UpdateInfo();
                    _updateInfo.AccountID = _iAccountID;
                    _updateInfo.Mobile = input.mobile;

                    int _iDBUpdate = _iOTPDAO.UpdateInfo(_updateInfo);

                    if (_iDBUpdate > 0)
                    {
                        OTPs.ResponMes responMes = new OTPs.ResponMes();
                        responMes.ResponCode = _iDBUpdate;
                        responMes.Mes = "Update thông tin thành công";
                        return Ok(responMes);
                    }
                    else
                    {
                        OTPs.ResponMes responMes = new OTPs.ResponMes();
                        responMes.ResponCode = _iDBUpdate;
                        responMes.Mes = "Update thông tin thất bại";
                        return Ok(responMes);
                    }
                }
                else
                {
                    //  UPDATE ACCOUNTINFO SUSSCESS
                    OTPs.AccountVerifySms _accountVerifySms = new OTPs.AccountVerifySms();
                    _accountVerifySms.AccountID = _iAccountID;
                    _accountVerifySms.Username = _strAccountName;
                    _accountVerifySms.TYPE = input.otpType;
                    _accountVerifySms.UserSMS = input.mobile;
                    _accountVerifySms.CodeVerifying = int.Parse(input.otp);

                    int _iSQLResult = _iOTPDAO.AccountVerifySMS(_accountVerifySms);

                    if (_iSQLResult >= 0)
                    {
                        OTPs.ResponMes responMes = new OTPs.ResponMes();
                        responMes.ResponCode = 1;
                        responMes.Mes = "Verify thông tin thành công";
                        return Ok(responMes);
                    }
                    else
                    {
                        OTPs.ResponMes responMes = new OTPs.ResponMes();
                        responMes.ResponCode = -99;
                        responMes.Mes = "Verify thông tin thất bại";
                        return Ok(responMes);
                    }
                }



                //string str_url = ConfigurationManager.AppSettings["URL_API"] + "api/Account/UpdateAccountInfo";

                //AccountUpdateInfoSendPost _inputPost = new AccountUpdateInfoSendPost();

                //DateTime currTime = DateTime.Now;
                //string _str_key_api = Security.MD5Encrypt(ConfigurationManager.AppSettings["KeyApi"] + currTime.Hour + currTime.Day + currTime.Month + currTime.Year + _iAccountID);

                //_inputPost.accountId = _iAccountID;
                //_inputPost.seviceKey = _str_key_api;
                //_inputPost.mobile = input.mobile;
                //_inputPost.otp = input.otp;
                //_inputPost.otpType = input.otpType;

                //var _accPostDataJson = JsonConvert.SerializeObject(_inputPost);

                //NLogManager.LogMessage(string.Format("_inputPost : {0}", JsonConvert.SerializeObject(_inputPost)));

                ////  call api mr Toan
                //var _resultPostUpdateAccountInfo = HttpUtils.SendPostMobile(_accPostDataJson, str_url);
                //var _resultPostUpdateAccountInfoJson = new AccountMobileReturnData();

                //try
                //{
                //    _resultPostUpdateAccountInfoJson = JsonConvert.DeserializeObject<AccountMobileReturnData>(_resultPostUpdateAccountInfo);
                //    _resultPostUpdateAccountInfoJson.ResponCode = 1;
                //    //return Ok(_resultPostUpdateAccountInfoJson);

                //    if (string.IsNullOrEmpty(input.otp))
                //    {
                //        //  return luôn
                //        return Ok(_resultPostUpdateAccountInfoJson);
                //    }
                //    else
                //    {
                //        //  UPDATE ACCOUNTINFO SUSSCESS
                //        AccountVerifySms _accountVerifySms = new AccountVerifySms();
                //        _accountVerifySms.AccountID = _iAccountID;
                //        _accountVerifySms.Username = _strAccountName;
                //        _accountVerifySms.TYPE = input.otpType;
                //        _accountVerifySms.UserSMS = input.mobile;
                //        _accountVerifySms.CodeVerifying = int.Parse(input.otp);

                //        int _iSQLResult = _sql.AccountVerifySMS(_accountVerifySms);

                //        if (_iSQLResult >= 0)
                //        {
                //            ResponMes responMes = new ResponMes();
                //            responMes.ResponCode = 1;
                //            responMes.Mes = "Update thông tin thành công !";
                //            return Ok(responMes);
                //        }
                //        else
                //        {
                //            ResponMes responMes = new ResponMes();
                //            responMes.ResponCode = -99;
                //            responMes.Mes = "Update thông tin thất bại !";
                //            return Ok(responMes);
                //        }
                //    }
                //}
                //catch (Exception e)
                //{
                //    ResponMes responMes = new ResponMes();
                //    responMes.ResponCode = -99;
                //    responMes.Mes = _resultPostUpdateAccountInfo;
                //    return Ok(responMes);
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_BUSY, _accountSession.Language);
            }
        }
        /// <summary>
        /// Kích hoạt xác thực 2 bước cho tài khoản
        /// </summary>
        /// <param name="uiid"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("ActiveTwoFactor")]
        public ActionResult<ResponseBuilder> ActiveTwoFactor(string uiid)
        {
            try
            {
                if (string.IsNullOrEmpty(uiid))
                    return BadRequest("Dữ liệu không hợp lệ");

                Base32Encoder enc = new Base32Encoder();
                string secret = enc.Encode(Encoding.ASCII.GetBytes(_accountSession.AccountName + "." + uiid + "." + Security.RandomPassword()));
                secret = secret.Substring(0, 16);
                OTPs.ActiveFactor activeTwoFactor = new OTPs.ActiveFactor(_accountSession.AccountID, _accountSession.AccountName, uiid, secret, 1);
                int res = _iOTPDAO.ActiveTwoFactor(activeTwoFactor);
                if (res < 0)
                    return BadRequest("Kích hoạt xác thực 2 bước không thành công");

                return Ok(JsonConvert.SerializeObject(new OTPs.ActiveFactorOutput("Hateco", "Kích hoạt thành công", secret)));
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return BadRequest("Kích hoạt xác thực 2 bước không thành công");
        }
        /// <summary>
        /// Xác thực đăng nhập
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("TwoFactorVerify")]
        public ActionResult<ResponseBuilder> TwoFactorVerify(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                    return BadRequest("Mã xác thực không đúng");

                string tokenKey = "";
                string secretKey = "";
                int res = _iOTPDAO.GetTwoFactorInfo(_accountSession.AccountID, out tokenKey, out secretKey);
                if (res < 0)
                {
                    NLogManager.Error("Lấy thông tin xác thực lỗi");
                    return BadRequest("Lỗi hệ thống, vui lòng thử lại");
                }

                if (TimeBasedOneTimePassword.IsValid("GEZDGNBVGY3TQOJQ", code, 5))
                    return Ok("Xác thực thành công");
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return BadRequest("Mã xác thực không đúng");
        }
        #region [Lấy OTP test]
        /// <summary>
        /// Test only
        /// lấy mã otp qua token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("getOtpVerify")]
        public ActionResult<ResponseBuilder> getOtpVerify(int AccountID, string AccountName, string mobile, int type)
        {
            try
            {
                int codeVerify = 0;
                int res = _accountDAO.CreateSecuritySMS(AccountID, AccountName, mobile, type, ref codeVerify);
                if (res > 0)
                {
                    return Ok(codeVerify);
                }
                if (res < 0)
                {
                    NLogManager.Error("Lấy thông tin xác thực lỗi");
                    return BadRequest("Lỗi hệ thống, vui lòng thử lại");
                }
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return BadRequest("Lấy mã thất bại");
        }

        /// <summary>
        /// Test only: CHưa xong
        /// lấy mã otp qua token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("getOtpSMS")]
        public ActionResult<ResponseBuilder> getOtpSMS(dynamic model)
        {

            try
            {
                if (model == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(_accountSession.AccountID);

                if (string.IsNullOrEmpty(securityInfo.Mobile))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                string userName = _accountSession.AccountName;
                string userSMS = securityInfo.Mobile;
                int codeVerify = 0;
                ResponseApiData result = _otpSecurity.CreateSecuritySMSByApi(_accountSession.AccountID, userName, userSMS, _accountSession.IpAddress, ref codeVerify);

                if (result != null && result.code == 0)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, result.data);
                }
                else
                {
                    NLogManager.Error("Lấy thông tin xác thực lỗi");
                    if (result.code == -51)
                        return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_ENOUGH_MONEY, _accountSession.Language);
                    if (result.code == -21)
                        return new ResponseBuilder(ErrorCodes.SEND_OTP_SMS_ERROR, _accountSession.Language);
                    return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
                }
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
            // return BadRequest("Lấy mã thất bại");
        }

        [HttpPost("getOtpSMSEx")]
        public ActionResult<ResponseBuilder> getOtpSMS1(dynamic model)
        {
            // Request.HttpContext.Authentication.

            try
            {
                string captchaText = model.captchaText;
                string captchaToken = model.captchaToken;

               // if (Captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
               //     return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);

                string accessToken = model.accesstoken;
                
                if (accessToken == null || accessToken.Length <=0)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                var otpData =_otpSecurity.ParseToken(accessToken);

                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo((int)otpData.AccountId);

                if (string.IsNullOrEmpty(securityInfo.Mobile))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, "VN");

                string userName = otpData.AccountName;//model.userName;
                long AccountID = otpData.AccountId;
                string userSMS = securityInfo.Mobile;
                int codeVerify = 0;
                ResponseApiData result = _otpSecurity.CreateSecuritySMSByApi(AccountID, userName, userSMS, _accountSession.IpAddress, ref codeVerify);
                if (result!= null && result.code == 0)
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, codeVerify);
                }
                else
                {
                    NLogManager.Error("Lấy thông tin xác thực lỗi");
                    if (result.code == -51)
                        return new ResponseBuilder(ErrorCodes.TRANSFER_ERR_NOT_ENOUGH_MONEY, _accountSession.Language);
                    if (result.code == -21)
                        return new ResponseBuilder(ErrorCodes.SEND_OTP_SMS_ERROR, _accountSession.Language);
                    
                    return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
                }
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
            // return BadRequest("Lấy mã thất bại");
        }
        #endregion

        [HttpPost("LoginVerifyOTP")]
        public ActionResult<ResponseBuilder> LoginVerifyOTP([FromBody] OtpModel model)
        {
            try
            {
                if (model == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                OtpData otpData = _otpSecurity.ParseToken(model.OtpToken);
                if (otpData == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                // 1: SMS verify, 2: App/Tele verify: Mặc định là SMS
                int type = model.OtpType == 0 ? 1 : model.OtpType;

                // Lấy ra số điện thoại từ Token để xác thực
                var securityInfo = _accountDAO.GetSecurityInfo(otpData.AccountId);

                // Xác thực qua API
                int rs = _otpSecurity.VerifyOtpByApi(securityInfo.Mobile, model.Otp, type);

                if (rs != 0) return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                // GenerateToken trả về
                AccountDb account = _accountDAO.GetAccount(otpData.AccountId, otpData.AccountName);
                account.IsAgency = _agencyDao.CheckIsAgency((int)account.AccountID);
                account.IsMobileActived = securityInfo.IsMobileActived;
                AccountInfo accountInfo = new AccountInfo(account);
                accountInfo.AccessToken = _authenticateService.GenerateToken(account);
                accountInfo.AccessTokenFishing = _authenticateService.GenerateTokenFishing(account);


                string message = String.Format("Bạn vừa đăng nhập vào hệ thống Game lúc:{0}! Chúc bạn chơi game vui vẻ", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                _otpSecurity.PushMessageToTelegram(accountInfo.AccountID, accountInfo.Mobile, message);

                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, accountInfo);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpPost("ChangePassVerifyOTP")]
        public ActionResult<ResponseBuilder> ChangePassVerifyOTP([FromBody] OtpModel model)
        {
            try
            {
                if (model == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                OtpData otpData = _otpSecurity.ParseToken(model.OtpToken);
                if (otpData == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                // Lấy account và mật khẩu từ token
                ChangePassWordData data = JsonConvert.DeserializeObject<ChangePassWordData>(otpData.ServiceData);
                // Lấy số điện thoại
                var securityInfo = _accountDAO.GetSecurityInfo(otpData.AccountId);

                // 1: SMS verify, 2: App/Tele verify: Mặc định là SMS
                int type = model.OtpType == 0 ? 1 : model.OtpType;

                // Xác thực OTP truyền lên: Dùng chung với hàm xác thực login: comment 20190815
                //bool rs = _otpSecurity.LoginVerifyOTP(data.AccountName, data.AccountID, model.Otp, securityInfo.Mobile);

                // Xác thực qua API
                int rs = _otpSecurity.VerifyOtpByApi(securityInfo.Mobile, model.Otp, type);
                if (rs != 0) return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                // Cập nhật lại mật khẩu mới
                string newPassEnc = Security.GetPasswordEncryp(data.NewPassWord, data.AccountName);
                string oldPassEnc = Security.GetPasswordEncryp(data.OldPassWord, data.AccountName);

                int response = _accountDAO.ChangePassword(data.AccountID, oldPassEnc, newPassEnc);

                if (response >= 0)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);

                return new ResponseBuilder(ErrorCodes.ACCOUNT_CHANGE_PASSWORD_FAILED, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        private void SendEmail(string toEmail, string userName, string otp)
        {
            if (string.IsNullOrEmpty(toEmail))
                return;

            string fromEmail = Convert.ToString(ConfigurationManager.AppSettings["Email"]);
            string password = Convert.ToString(ConfigurationManager.AppSettings["PasswordEmail"]);
            string emailHost = Convert.ToString(ConfigurationManager.AppSettings["EmailHost"]);
            int emailPort = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);
            string displayName = Convert.ToString(ConfigurationManager.AppSettings["DisplayName"]);
            string subject = "Mã xác thực";

            try
            {
                StringBuilder content = new StringBuilder();
                content.AppendLine("Xin chào tài khoản " + userName);
                content.AppendLine("Mã xác thực để kích hoạt OTP App của bạn là: " + otp);
                content.AppendLine("Mã xác thực có hiệu lực trong vòng 3 phút.");
                content.AppendLine();

                MailUtil.SendEmail(fromEmail, password, emailHost, emailPort, displayName, toEmail, subject, content.ToString());
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }

        [Authorize]
        [HttpPost("VerifyOTP")]
        public ActionResult<ResponseBuilder> VerifyOTP(dynamic model)
        {
            try
            {
                if (model == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(_accountSession.AccountID);

                if (string.IsNullOrEmpty(securityInfo.Mobile))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                string userName = _accountSession.AccountName;
                string userSMS = securityInfo.Mobile;
                string otp = model.otp;
                string type1 = model.type;
                int type = Int32.Parse(type1);
                int rs = _otpSecurity.VerifyOtpByApi(securityInfo.Mobile, otp, type);
                if (rs != 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);
                else
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpPost("TelegramOTP")]
        public ActionResult<ResponseBuilder> TelegramOTP(dynamic model)
        {
            long accountID = 0;
            accountID = _accountSession.AccountID;
            if (accountID <= 0)
                return new ResponseBuilder(ErrorCodes.TOKEN_ERROR, _accountSession.Language);
            try
            {
                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(_accountSession.AccountID);

                if (string.IsNullOrEmpty(securityInfo.Mobile))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                int iCountCache = CacheCounter.AccountIPActionCounter(_accountSession.AccountName, "TelegramOTP", 5, _accountSession.IpAddress);
                if (iCountCache > 3)
                {
                    string captchaText = model.captchaText;
                    string captchaToken = model.captchaToken;

                     if (_captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
                         return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);
                }


                ResponseApiData rs = _otpSecurity.TelegramOTP(_accountSession.AccountID);
                NLogManager.Info("TelegramOTP 1 " + JsonConvert.SerializeObject(rs));
                if (rs != null && rs.code >=0)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, null);
                            
                return new ResponseBuilder(ErrorCodes.TELEGRAM_OTP_CHATID_NULL, _accountSession.Language, null);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
        }

        [HttpPost("TelegramOTPEX")]
        public ActionResult<ResponseBuilder> TelegramOTPEX(dynamic model)
        {
            long accountID = 0;
            string token = model.token;
            if (token != null && token.Length > 0)
            {
                OtpData otpData = _otpSecurity.ParseToken(token);
                if (otpData != null)
                    accountID = otpData.AccountId;
            }

            if (accountID <= 0)
                return new ResponseBuilder(ErrorCodes.TOKEN_ERROR, _accountSession.Language);
            try
            {
                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(accountID);

                if (string.IsNullOrEmpty(securityInfo.Mobile))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_MISSING_MOBILE, _accountSession.Language);

                int iCountCache = CacheCounter.AccountIPActionCounter(accountID + "", "TelegramOTP", 5, _accountSession.IpAddress);
                if (iCountCache > 3)
                {
                    string captchaText = model.captchaText;
                    string captchaToken = model.captchaToken;

                    if (_captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_VERIFY_CODE_INVALID, _accountSession.Language);
                }


                ResponseApiData rs = _otpSecurity.TelegramOTP(accountID);
                NLogManager.Info("TelegramOTP 2 " + JsonConvert.SerializeObject(rs));
                if (rs != null && rs.code >= 0)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, null);

                return new ResponseBuilder(ErrorCodes.TELEGRAM_OTP_CHATID_NULL, _accountSession.Language, null);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
        }
    }
}