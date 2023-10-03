using Newtonsoft.Json;
using PortalAPI.Models;
using System;
using Microsoft.AspNetCore.Mvc;
using ServerCore.PortalAPI.OTP;
using PortalAPI.Services;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Utils;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Security;
using ServerCore.Utilities.Models;

namespace PortalAPI.Controllers
{
    [Route("Privacy")]
    [ApiController]
    public class PrivacyController : ControllerBase
    {
        private readonly AccountSession _accountSession;
        private readonly OTPSecurity _otpSecurity;
        private readonly CoreAPI _coreAPI;
        private IAccountDAO _accountDAO;

        public PrivacyController(OTPSecurity otp, IAccountDAO accountDAO, CoreAPI coreAPI, AccountSession accountSession)
        {
            this._otpSecurity = otp;
            this._accountDAO = accountDAO;
            this._coreAPI = coreAPI;
            this._accountSession = accountSession;
        }

        [HttpPost("VerifyOTP")]
        public ActionResult<ResponseBuilder> VerifyOTP(OtpModel model)
        {
            try
            {
                if (model == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID,_accountSession.Language);

                if (string.IsNullOrEmpty(model.OtpToken))
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                //if(CaptchaUtil.VerifyCaptcha(model.Captcha, model.CaptchaToken) <= 0)
                //    return new ResponseBuilder(ErrorCodes.BadRequest, "Mã xác nhận không đúng.", "BadRequest");

                OtpData otpData = _otpSecurity.ParseToken(model.OtpToken);
                if (otpData == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                if (otpData.ServiceId != (int)OtpServiceCode.OTP_SERVICE_LOGIN)
                {
                    if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                        return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);
                }

                if (!otpData.IsExpired())
                    return new ResponseBuilder(ErrorCodes.TRANSACTION_EXPIRED, _accountSession.Language);

                if (!otpData.IsServiceOK(model.ServiceId))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_DATA_NOT_MATCH, _accountSession.Language);

                int type = 0;
                if(model.ServiceId == (int)OtpServiceCode.OTP_SERVICE_TRANSFER)
                    type = (int)OtpType.OTP_TRANSFER;

                OTPCode code = _otpSecurity.CheckOTP(otpData.AccountId, otpData.AccountName, model.Otp, model.OtpType, type);
                if (code != OTPCode.OTP_VERIFY_SUCCESS)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                //NLogManager.Info(string.Format("Otp = {0}, OtpType = {1}, ServiceId = {2}", model.Otp, model.OtpType, model.ServiceId));
                switch ((OtpServiceCode)model.ServiceId)
                {
                    //case OtpService.OTP_SERVICE_LOGIN:
                    //    {
                    //        //AccountDb accountDB = CoreAPI.Instance().Login(JsonConvert.DeserializeObject<LoginData>(otpData.ServiceData));
                    //        LoginInfo loginData = JsonConvert.DeserializeObject<LoginInfo>(otpData.ServiceData);
                    //        AccountDb account = _accountDAO.GetAccount(otpData.AccountId, loginData.UserName);
                    //        //  GET ACCOUNT-INFO VIA ACCOUNTID
                    //        SecurityInfo securityInfo = _otpSecurity.GetSucurityInfoWithMask((int)account.AccountID, false);
                    //        if(securityInfo == null)
                    //        {
                    //            return new ResponseBuilder(ErrorCodes.BadRequest, "Không lấy được dữ liệu của người chơi, mời thử lại sau !", "BadRequest");
                    //        }
                    //        //  END

                    //        string _strToken = Security.GetVerifyTokenOTPLogin(6, (int)account.AccountID, account.UserName, securityInfo.Mobile);
                    //        account.AccountToken = _strToken;
                    //        account.IsMobileActived = securityInfo.IsMobileActived;
                    //        if(!string.IsNullOrEmpty(securityInfo.Mobile))
                    //        {
                    //            account.Mobile = StringUtil.MaskMobile(securityInfo.Mobile);
                    //        }

                    //        _coreAPI.SetAuthCookie(account);

                    //        var acc = new AccountDataReturn(account);
                    //        acc.IsAllowChat = _coreAPI.GetChatStatus(account.AccountID);
                    //        return new ResponseBuilder(ErrorCodes.OK, JsonConvert.SerializeObject(acc), "OK");
                    //    }

                    case OtpServiceCode.OTP_SERVICE_BUY_CARD:
                        {
                            BuyCardData data = JsonConvert.DeserializeObject<BuyCardData>(otpData.ServiceData);
                            string res = _coreAPI.BuyCard(data.UserName, data.Provider, data.Type, data.Money, data.PhoneNumber, data.AccountId, data.MerchantId, data.SourceId, data.Quantity);
                            if(res != null)
                            {
                                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, res);
                            }
                            else
                                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
                        }

                    case OtpServiceCode.OTP_SERVICE_TRANSFER:
                        {
                            TransferData data = JsonConvert.DeserializeObject<TransferData>(otpData.ServiceData);
                            long balanceReceive = 0, receiveAmount = 0;
                            long balance = _coreAPI.Transfer(data.AccountIdTrans, data.NickNameTrans, data.TransferValue, data.AccountIdRecv, data.NickNameRecv, data.TransferType, data.IpAddress, data.SourceId, data.Reason, 1, "", out balanceReceive, out receiveAmount);
                            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, balance.ToString());
                        }

                    case OtpServiceCode.OTP_SERVICE_CHANGE_PASSWORD:
                        {
                            ChangePassWordData data = JsonConvert.DeserializeObject<ChangePassWordData>(otpData.ServiceData);
                            string newPassEnc = Security.GetPasswordEncryp(data.NewPassWord, data.AccountName);
                            string oldPassEnc = Security.GetPasswordEncryp(data.OldPassWord, data.AccountName);
                            int res = _accountDAO.ChangePassword(data.AccountID, oldPassEnc, newPassEnc);
                            string content = "";
                            if(res == -1)
                                return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);
                            else if(res == -2)
                                return new ResponseBuilder(ErrorCodes.PASSWORD_OLD_INVALID, _accountSession.Language);
                            else if(res >= 0)
                            {
                                NLogManager.Info("User: " + data.AccountName + " ChangePass success");
                                content = "Đổi mật khẩu thành công.\nVui lòng thoát game và đăng nhập lại.";
                                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, content);

                            }
                            else
                                return new ResponseBuilder(ErrorCodes.ACCOUNT_CHANGE_PASSWORD_FAILED, _accountSession.Language);

                        }

                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [HttpPost("ReportVerifyOTP")]
        public ActionResult<ResponseBuilder> ReportVerifyOTP([FromBody] OtpModel model)
        {
            try
            {
                if (model == null)
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                OtpData otpData = _otpSecurity.ParseToken(model.OtpToken);

                if (otpData.ServiceId == (int)OtpServiceCode.OTP_SERVICE_LOGIN)
                {
                    if (!PolicyUtil.IsServiceKeyLogin(model.ServiceKey))
                        return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);
                }
                else
                {
                    if (!PolicyUtil.IsServiceKeyOk(model.ServiceKey, otpData.AccountId))
                        return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);
                }

                if (!otpData.IsExpired())
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_EXPIRED, _accountSession.Language);

                if (!otpData.IsServiceOK(model.ServiceId))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_DATA_NOT_MATCH, _accountSession.Language);

                OTPCode code = _otpSecurity.CheckOTP(otpData.AccountId, otpData.AccountName, model.Otp, model.OtpType);
                if (code != OTPCode.OTP_VERIFY_SUCCESS)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_MATCH, _accountSession.Language);

                switch (model.ServiceId)
                {
                    case (int)OtpServiceCode.OTP_SERVICE_LOGIN:
                        LoginData loginData = JsonConvert.DeserializeObject<LoginData>(otpData.ServiceData);
                        AccountDb account = _accountDAO.GetAccount(otpData.AccountId, otpData.AccountName);
                        //_coreAPI.SetAuthCookie(account);
                        return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, new AccountDataReturn(account));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }
    }
}