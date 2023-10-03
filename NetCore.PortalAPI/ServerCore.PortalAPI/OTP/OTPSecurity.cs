using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerCore.DataAccess;
using ServerCore.DataAccess.DAO;
using ServerCore.PortalAPI.Models;
using ServerCore.Utilities;
using ServerCore.Utilities.Captcha;
using ServerCore.Utilities.Interfaces;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Security;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace ServerCore.PortalAPI.OTP
{
    public class OTPSecurity
    {
        private static readonly int TOKEN_TIMEOUT = 3; // minutie
        public static readonly string OTP_KEY = "koyCheckOTP@tt.ibon.club)(!@#&*(^@000";
        private static readonly int OTP_ERROR_CODE = -100;

        private readonly AppSettings _appSettings;
        private readonly IAccountDAO _accountDAO;
        private readonly IDataService _dataService;
        private readonly Captcha _captcha;

        public OTPSecurity(IAccountDAO accountDAO, IOptions<AppSettings> options, IDataService dataService, Captcha captchaUtil)
        {
            this._accountDAO = accountDAO;
            this._appSettings = options.Value;
            _dataService = dataService;
            _captcha = captchaUtil;
        }

        public bool VerifyOTP(string otpToken, string otp, int otpType, string mobile, out string accountNameOut, out int accountIdOut, out int platformId)
        {
            accountIdOut = -1;
            accountNameOut = "";
            platformId = -1;

            if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(otpToken) || otpType < 1 || otpType > 2)
                return false;

            if (string.IsNullOrEmpty(mobile))
                return false;

            otpToken = Security.TripleDESDecrypt(OTP_KEY, otpToken);
            var token = otpToken.Split('|');
            if (token.Length <= 0)
                return false;

            int accountId = Convert.ToInt32(token[1]);
            string accountName = token[0];
            long tick = Convert.ToInt64(token[3]);
            platformId = Convert.ToInt32(token[2]);

            if (accountId <= 0 || string.IsNullOrEmpty(accountName))
                return false;

            long tickNow = DateTime.Now.Ticks;
            TimeSpan diffTime = new TimeSpan(tickNow - tick);
            if (diffTime.TotalMinutes > TOKEN_TIMEOUT)
                return false;

            int response = (otpType == 1) ? _accountDAO.CheckOTPSMS(accountId, accountName, otp, mobile) : _accountDAO.CheckOTPApp(accountId, mobile, otp);
            if (response < 0)
                return false;

            accountIdOut = accountId;
            accountNameOut = accountName;
            return true;
        }

        public bool GetAccountInfoFromToken(string otpToken, out int accountId, out string accountName)
        {
            accountName = "";
            accountId = -1;

            string accountInfo = Security.TripleDESDecrypt(OTP_KEY, otpToken);
            var token = accountInfo.Split('|');
            if (token.Length <= 0)
                return false;

            accountId = Convert.ToInt32(token[1]);
            accountName = token[0];

            return true;
        }

        public OtpData ParseToken(string otpToken)
        {
            try
            {
                // tokenInfo = tokenKey;serviceData
                string tokenInfo = Security.TripleDESDecrypt(OTP_KEY, otpToken);
                OtpData otpData = JsonConvert.DeserializeObject<OtpData>(tokenInfo);
                if (otpData != null && !otpData.IsExpired())
                {
                    return null;
                }
                //var token = tokenInfo.Split('|');
                ////var token = tokendesc[0].Split('|');
                //if(token.Length <= 0)
                //    return null;

                //string serviceData = token[5];
                ////string token = Security.TripleDESEncrypt(OTP_KEY, string.Format("{0}|{1}|{2}|{3}|{4}", userName, accountID, serviceId, platform, tick));
                //string accountName = token[0];
                //int accountId = Convert.ToInt32(token[1]);
                //int serviceId = Convert.ToInt32(token[2]);
                //int platformId = Convert.ToInt32(token[3]);
                //long tick = Convert.ToInt64(token[4]);
                //return new OtpData(accountName, accountId, tick, serviceId, platformId, serviceData);
                return otpData;
            }
            catch (Exception ex)
            {
                NLogManager.Info(string.Format("Exception: ", ex.Message));
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountId"></param>
        /// <param name="otp"></param>
        /// <param name="otpType">1: OTP SMS; 2: OTP APP, OTP Telegram BOT</param>
        /// <param name="mobile"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool VerifyOTP(string accountName, long accountId, string otp, int otpType, string mobile, int type = 0)
        {
            if (string.IsNullOrEmpty(accountName) || accountId < 0 || string.IsNullOrEmpty(otp) || otpType < 1 || otpType > 2)
                return false;

            if (string.IsNullOrEmpty(mobile))
                return false;

            int response = (otpType == 1) ? _accountDAO.CheckOTPSMS(accountId, accountName, otp, mobile, type) : _accountDAO.CheckOTPApp(accountId, mobile, otp);
            NLogManager.Info(string.Format("accountName = {0}, otp = {1}, otpType = {2}, mobile = {3}, type = {4}, res = {5}", accountName, otp, otpType, mobile, type, response));
            if (response < 0)
                return false;

            return true;
        }

        public bool LoginVerifyOTP(string accountName, long accountId, string otp, string mobile)
        {
            int response = _accountDAO.CheckOTPSMS(accountId, accountName, otp, mobile, 1);
            NLogManager.Info(string.Format("accountName = {0}, otp = {1}, otpType = {2}, mobile = {3}, type = {4}, res = {5}", accountName, otp, "", mobile, "", response));
            if (response < 0)
                return false;
            return true;
        }
        /// <summary>
        /// Verify OTP qua API 
        /// </summary>
        /// <param name="mobile">Nếu là OTP App thì cần thêm mã nước (+84) ở đầu SĐT</param>
        /// <param name="otp">Mã otp trả về qua app hoặc tele hoặc sms</param>
        /// <param name="type">1: SMS, 2: App - Telegram</param>
        /// <returns></returns>

        public int VerifyOtpByApi(string mobile, string otp, int type)
        {
            // Tạo postData
            dynamic verifyData = new ExpandoObject();
            verifyData.codeVerifying = otp;
            verifyData.userSMS = mobile;
            verifyData.type = type;
            var jsonData = JsonConvert.SerializeObject(verifyData);

            // Gọi api verify
            NLogManager.Info(string.Format("mobile = {0}, otp = {1}, type = {2}", mobile, otp, type));
            //int response = (otpType == 1) ? _accountDAO.CheckOTPSMS(accountId, accountName, otp, mobile) : _accountDAO.CheckOTPApp(accountId, mobile, otp);
            string res = HttpUtil.SendPost(jsonData, _appSettings.OtpServiceApiUrl + "otp/verify");
            ResponseApiData resp = JsonConvert.DeserializeObject<ResponseApiData>(res);

            //Mr. 007, [11.09.19 14:07]
            //-11.OTP Expired
            //- 44 API Interal error
            // Trả về kết quả
            if (resp.code == 0) return (int)ErrorCodes.SUCCESS;
            else if (resp.code == -11)
                return (int)ErrorCodes.ACCOUNT_OTP_EXPIRED;
            else if (resp.code == -44)
                return (int)ErrorCodes.ACCOUNT_OTP_INVALID;
            return (int)ErrorCodes.ACCOUNT_OTP_CANCEL_FAILED;
        }
        public int VerifyOtpByNewApi(string accountName, long accountId, string mobile, string otp, int type)
        {
            // Tạo postData
            //dynamic verifyData = new ExpandoObject();
            //verifyData.codeVerifying = otp;
            //verifyData.userSMS = mobile;
            //verifyData.type = type;
            ///var jsonData = JsonConvert.SerializeObject(verifyData);

            // Gọi api verify
            NLogManager.Info(string.Format("mobile = {0}, otp = {1}, type = {2}", mobile, otp, type));
            int response = _accountDAO.CheckOTPSMS(accountId, accountName, otp, mobile, type);
            //string res = HttpUtil.SendPost(jsonData, _appSettings.OtpServiceApiUrl + "otp/verify");
            //ResponseApiData resp = JsonConvert.DeserializeObject<ResponseApiData>(res);

            //Mr. 007, [11.09.19 14:07]
            //-11.OTP Expired
            //- 44 API Interal error
            // Trả về kết quả
            if (response >0) return (int)ErrorCodes.SUCCESS;
            //else if (resp.code == -11)
            //    return (int)ErrorCodes.ACCOUNT_OTP_EXPIRED;
            //else if (resp.code == -44)
            //    return (int)ErrorCodes.ACCOUNT_OTP_INVALID;
            else return (int)ErrorCodes.ACCOUNT_OTP_CANCEL_FAILED;
        }

        public ResponseApiData TelegramOTP(long accountID)
        {
            // Tạo postData
            dynamic verifyData = new ExpandoObject();
            verifyData.accountId = accountID;
            //var jsonData = JsonConvert.SerializeObject(verifyData);

            // Gọi api verify
            NLogManager.Info(string.Format("AccountID = {0}", accountID));
            var res = HttpUtil.GetAsync(_appSettings.OtpServiceApiUrl + "update/GetOTP?accountId=" + accountID).Result;
            //string res = HttpUtil.SendPost(jsonData, _appSettings.OtpServiceApiUrl + "update/GetOTP?accountId=" + accountID);
            //ResponseApiData resp = JsonConvert.DeserializeObject<ResponseApiData>(res);
            var resp = JsonConvert.DeserializeObject<ResponseApiData>(res);
            if (resp.code != 0)
                return resp;
            return null;
        }
        /// <summary>
        /// Nút resend otp: gửi lại lấy lại otp
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="mobile"></param>
        /// <param name="codeVerify"></param>
        /// <returns></returns>
        public ResponseApiData CreateSecuritySMSByApi(long accountID, string accountName, string mobile, string clientIP, ref int codeVerify)
        {
            // Tạo postData
            string result = "";
            dynamic creatOtpData = new ExpandoObject();
            creatOtpData.accountID = accountID;
            creatOtpData.username = accountName;
            creatOtpData.userSMS = mobile;
            creatOtpData.type = 1;
            creatOtpData.clientIP = clientIP;
            var jsonData = JsonConvert.SerializeObject(creatOtpData);

            // Gọi api create sms
            NLogManager.Info(string.Format("mobile = {0}, accountName = {1}, type = {2}", mobile, accountName, 1));
            result = HttpUtil.SendPost(jsonData, _appSettings.OtpServiceApiUrl + "/otp/sms");
            ResponseApiData resp = JsonConvert.DeserializeObject<ResponseApiData>(result);

            return resp;
        }

        public bool IsOTP(long accountId)
        {
            if (accountId <= 0)
                return false;

            var securityInfo = _accountDAO.GetSecurityInfo(accountId);
            if (securityInfo != null)
                return securityInfo.IsMobileActived;

            return false;
        }

        public void PushMessageToTelegram(long accountID, string mobile, string message)
        {
            // Tạo postData
            dynamic creatOtpData = new ExpandoObject();
            creatOtpData.accountId = accountID;
            creatOtpData.message = message;
            creatOtpData.phone = mobile;


            List<ExpandoObject> lsSend = new List<ExpandoObject>();
            lsSend.Add(creatOtpData);

            var jsonData = JsonConvert.SerializeObject(lsSend);
            // Gọi api create sms

            NLogManager.Info(string.Format("Telegram: {0} - {1}", jsonData, _appSettings.OtpServiceApiUrl + "message"));
            string res = HttpUtil.SendPost(jsonData, _appSettings.OtpServiceApiUrl + "message");
            NLogManager.Info(string.Format("Telegram Result: {0}", res));
            //await HttpUtil.PostAsync(_appSettings.OtpServiceApiUrl + "message", jsonData);
        }
        public async Task PushMessageToTelegramAsync(long accountID, string mobile, string message)
        {
            // Tạo postData
            dynamic creatOtpData = new ExpandoObject();
            creatOtpData.accountId = accountID;
            creatOtpData.message = message;
            creatOtpData.phone = mobile;


            List<ExpandoObject> lsSend = new List<ExpandoObject>();
            lsSend.Add(creatOtpData);
            await _dataService.PostAsync(_appSettings.OtpServiceApiUrl + "message", lsSend);
            //await HttpUtil.PostAsync(_appSettings.OtpServiceApiUrl + "message", jsonData);
        }

        //public OTPCode CheckOTP(int accountId, string accountName, string otpToken, string otp, int otpType, out string data)
        //{
        //    data = "";
        //    var securityInfo = _accountDAO.GetSecurityInfo(accountId);
        //    if(securityInfo != null && securityInfo.IsMobileActived)
        //    {
        //        if(string.IsNullOrEmpty(otpToken))
        //        {
        //            OTP otpObj = new OTP(CreateOTPToken(accountName, accountId), true);
        //            data = JsonConvert.SerializeObject(otpObj);
        //            return OTPCode.OTP_NOT_VERIFY;
        //        }

        //        if(string.IsNullOrEmpty(otpToken) || string.IsNullOrEmpty(otp))
        //        {
        //            OTP otpObj = new OTP(CreateOTPToken(accountName, accountId), true);
        //            data = JsonConvert.SerializeObject(otpObj);
        //            return OTPCode.OTP_VERIFY_ERROR;
        //        }

        //        if(otpType > 2 || otpType < 1)
        //        {
        //            OTP otpObj = new OTP(CreateOTPToken(accountName, accountId), true);
        //            data = JsonConvert.SerializeObject(otpObj);
        //            return OTPCode.OTP_VERIFY_ERROR;
        //        }

        //        string outAccountName = null;
        //        int outAccountID = -1;
        //        int plout = 0;
        //        if(!VerifyOTP(otpToken, otp, otpType, securityInfo.Mobile, out outAccountName, out outAccountID, out plout))
        //        {
        //            OTP otpObj = new OTP(CreateOTPToken(accountName, accountId), true);
        //            data = JsonConvert.SerializeObject(otpObj);
        //            return OTPCode.OTP_VERIFY_ERROR;
        //        }
        //        return OTPCode.OTP_VERIFY_SUCCESS;
        //    }
        //    return OTPCode.NOT_OTP;
        //}

        /// <summary>
        /// Kiểm tra mã OTP SMS
        /// </summary>
        /// <param name="otp">
        /// Mã OTP
        /// </param>
        /// <param name="type">
        /// 0: Kiểm tra mã OTP
        /// 9: Hủy Login OTP
        /// 1: DK OTP
        /// </param>
        /// <param name="otpType">
        /// 1: OTP SMS
        /// 2: OTP APP
        /// </param>
        /// <param name="userName"></param>
        /// <returns>
        /// 3: Thành công
        /// != 3: Thất bại
        /// </returns>
        public OTPCode CheckOTP(long accountId, string accountName, string otp, int otpType, int type = 0)
        {
            var securityInfo = _accountDAO.GetSecurityInfo(accountId);
            if (securityInfo != null)
            {
                if (type != (int)OtpType.OTP_REGISTER)
                {
                    if (!securityInfo.IsMobileActived)
                        return OTPCode.NOT_OTP;
                }

                if (string.IsNullOrEmpty(otp))
                    return OTPCode.OTP_NEED_INPUT;

                if (otpType > 2 || otpType < 1)
                    return OTPCode.OTP_VERIFY_ERROR;
                // Đổi phương thức verify: qua API
                //if(!VerifyOTP(accountName, accountId, otp, otpType, securityInfo.Mobile, type))
                //    return OTPCode.OTP_VERIFY_ERROR;

                if (VerifyOtpByNewApi(accountName, accountId, securityInfo.Mobile, otp, otpType) != 0)
                    return OTPCode.OTP_VERIFY_ERROR;

                return OTPCode.OTP_VERIFY_SUCCESS;
            }
            return OTPCode.OTP_VERIFY_ERROR;
        }

        public OTPCode CheckResetPasswordOTP(long accountId, string accountName, string otp)
        {
            var securityInfo = _accountDAO.GetSecurityInfo(accountId);
            if (securityInfo != null)
            {
                if (!VerifyOTP(accountName, accountId, otp, 1, securityInfo.Mobile, 1))
                    return OTPCode.OTP_VERIFY_ERROR;

                return OTPCode.OTP_VERIFY_SUCCESS;
            }
            return OTPCode.NOT_OTP;
        }

        public OTPCode CheckOTP(long accountId, string accountName, string otp, int otpType, string captcha, string captchaToken)
        {
            if (_captcha.VerifyCaptcha(captcha, captchaToken) < 0)
                return OTPCode.OTP_VERIFY_ERROR;

            var securityInfo = _accountDAO.GetSecurityInfo(accountId);
            if (securityInfo != null && securityInfo.IsMobileActived)
            {
                if (string.IsNullOrEmpty(otp))
                    return OTPCode.OTP_NEED_INPUT;

                if (otpType > 2 || otpType < 1)
                    return OTPCode.OTP_VERIFY_ERROR;

                if (!VerifyOTP(accountName, accountId, otp, otpType, securityInfo.Mobile))
                    return OTPCode.OTP_VERIFY_ERROR;

                return OTPCode.OTP_VERIFY_SUCCESS;
            }
            return OTPCode.NOT_OTP;
        }

        public OTPCode CheckOTP(OtpData otpData, OtpModel otpModel)
        {
            long tickNow = DateTime.Now.Ticks;
            TimeSpan diffTime = new TimeSpan(tickNow - otpData.CreateTime);
            if (diffTime.TotalMinutes > 3)
                return OTPCode.OTP_VERIFY_ERROR;

            if (otpModel.ServiceId != otpData.ServiceId)
                return OTPCode.OTP_VERIFY_ERROR;

            OTPCode code = CheckOTP(otpData.AccountId, otpData.AccountName, otpModel.Otp, otpModel.OtpType);
            return code;
        }

        /// <summary>
        /// Tao OTP token key
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="accountID"></param>
        /// <param name="serviceId"></param>
        /// <param name="platformId"></param>
        /// <returns></returns>
        public static string CreateOTPToken(string userName, long accountID, int serviceId, int platformId, string serviceData)
        {
            OtpData otpData = new OtpData(userName, accountID, DateTime.Now.Ticks, serviceId, platformId, serviceData);
            //long tick = DateTime.Now.Ticks;
            //string token = Security.TripleDESEncrypt(OTP_KEY, string.Format("{0}|{1}|{2}|{3}|{4}|{5}", userName, accountID, serviceId, platformId, tick, serviceData));
            //return token;
            return CreateOTPToken(otpData);
        }

        public static string CreateOTPToken(OtpData otpData)
        {
            return Security.TripleDESEncrypt(OTP_KEY, JsonConvert.SerializeObject(otpData));
        }

        public SecurityInfo GetSucurityInfoWithMask(long accountId, bool isMask = true)
        {
            var securityInfo = _accountDAO.GetSecurityInfo(accountId);
            if (securityInfo != null)
            {
                if (!string.IsNullOrEmpty(securityInfo.Email))
                    securityInfo.Email = StringUtil.MaskEmail(securityInfo.Email);

                if (!string.IsNullOrEmpty(securityInfo.Mobile) && isMask)
                {
                    if (securityInfo.IsMobileActived)
                        securityInfo.Mobile = StringUtil.MaskMobile(securityInfo.Mobile);
                }

                if (!string.IsNullOrEmpty(securityInfo.PassPort))
                    securityInfo.PassPort = StringUtil.MaskMobile(securityInfo.PassPort);
            }
            return securityInfo;
        }

        public static int GetOtpErrorCode()
        {
            return OTP_ERROR_CODE;
        }

        public string CreateOTPTokenFish(string nickName, long accountID, int serviceId, int platformId, string serviceData)
        {
            OtpData otpData = new OtpData(nickName, accountID, DateTime.Now.Ticks, serviceId, platformId, serviceData);
            return Security.TripleDESEncrypt(_appSettings.FishingKey, JsonConvert.SerializeObject(otpData));
        }

        public OtpData ParseOTPTokenFish(string otpToken)
        {
            try
            {
                string tokenInfo = Security.TripleDESDecrypt(_appSettings.FishingKey, otpToken);
                OtpData otpData = JsonConvert.DeserializeObject<OtpData>(tokenInfo);
                return otpData;
            }
            catch (Exception ex)
            {
                NLogManager.Info(string.Format("Exception: ", ex.Message));
            }
            return null;
        }
    }
    #region class
    public class ResponseApiData
    {
        public int code { get; set; }

        public string msg { get; set; }

        public dynamic data { get; set; }

        //public ResponseApiData(int code, string lng, dynamic data)
        //{
        //    Description = MessageBuilder.Build(code, lng);
        //    Code = code;
        //    Data = data;
        //}
    }
    #endregion
}
