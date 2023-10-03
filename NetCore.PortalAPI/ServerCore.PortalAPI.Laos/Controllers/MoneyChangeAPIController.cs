using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PortalAPI.Models;
using PortalAPI.Services;
using ServerCore.DataAccess.DAO;
using ServerCore.PortalAPI.Models;
using ServerCore.PortalAPI.OTP;
using ServerCore.Utilities.Captcha;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Utils;

namespace PortalAPI.Controllers
{
    [Route("MoneyChange")]
    public class MoneyChangeAPIController : ControllerBase
    {
        private static readonly int SERVICE_ID_CHARING = 100000;
        private static readonly int SERVICE_ID_BUYCARD = 100001;
        private static readonly string SERVICE_KEY = "123456";
        private static readonly int MIN_BON = 10000;
        private static readonly int MAX_BON = 1000000000;

        private readonly AccountSession _accountSession;
        private readonly OTPSecurity _otp;
        private readonly CoreAPI _coreAPI;
        private readonly AppSettings _appSettings;
        private readonly IGameTransactionDAO _coreDAO;
        private readonly Captcha _captcha;

        public MoneyChangeAPIController(AccountSession accountSession, OTPSecurity otp, CoreAPI coreAPI, IOptions<AppSettings> options, IGameTransactionDAO coreDAO, Captcha captcha)
        {
            this._accountSession = accountSession;
            this._otp = otp;
            this._coreAPI = coreAPI;
            this._appSettings = options.Value;
            this._coreDAO = coreDAO;
            _captcha = captcha;
        }


        [Authorize]
        [HttpGet("changeQToXu")]
        public ActionResult<ResponseBuilder> TransferQToXu(int bon, string captcha, string verify, int platformId)
        {
            try
            {
                //return new ResponseBuilder(ErrorCodes.BadRequest, "Chức năng này đang tạm khóa \nVui lòng quay lại sau", "BadRequest");

                if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(verify))
                {
                    return new ResponseBuilder(ErrorCodes.CAPTCHA_INVALID, _accountSession.Language) ;
                }
                if (_captcha.VerifyCaptcha(captcha, verify) < 0)
                {
                    return new ResponseBuilder(ErrorCodes.VERIFY_CODE_INVALID, _accountSession.Language);
                }

                long accountId = _accountSession.AccountID;
                string accountName = _accountSession.AccountName;
                string ipAddress = _accountSession.IpAddress;
                
                if (bon <= 0)
                    return new ResponseBuilder(ErrorCodes.TOTAL_COIN_INVALID, _accountSession.Language);

                long totalBac = -1;
                long totalQ = -1;
                long bac = -1;
                if (_coreAPI.TransferQToXu(accountId, ipAddress, bon, out totalBac, out bac, out totalQ))
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(new
                    {
                        TotalBac = totalBac,
                        Bac = bac,
                        TotalQ = totalQ
                    }));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.XU_TOPUP_FAILED, _accountSession.Language);
        }
        #region [Lịch sử giao dịch]
        [Authorize]
        [HttpGet("getTransactionLogs")]
        public ActionResult<ResponseBuilder> GetTransactionLogs(int limit)
        {
            try
            {
                dynamic list = _coreAPI.GetTransactionHistory((int)_accountSession.AccountID, limit);
                var accounthash = JsonConvert.DeserializeObject<List<TransactionLog>>(JsonConvert.SerializeObject(list));

                if (accounthash != null)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, accounthash);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("CashoutHistory")]
        public ActionResult<ResponseBuilder> CashoutHistory()
        {
            try
            {
                int  fromDate = Int32.Parse(DateTime.Now.AddDays(-30).ToString("yyyyMMdd"));
                int toDate = Int32.Parse(DateTime.Now.ToString("yyyyMMdd"));

                var obj = new
                {
                    fromDate,
                    toDate
                };

                // Gọi sang cổng nạp thẻ
                // Lấy ra header token để gửi đi
                var header = new WebHeaderCollection
                {
                    { "Authorization", HttpContext.Request.Headers["Authorization"] }
                };

                var jsonData = JsonConvert.SerializeObject(obj);

                string url = string.Format("{0}/CashoutHistory", _appSettings.ReChargeGetwayUrl);
                var res = HttpUtil.SendPostWithHeader(jsonData, url, header);
                if (res == null)
                    return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);

                var resObject = JsonConvert.DeserializeObject<ResponseBuilder>(res);

                return resObject;
            }
            catch (System.Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("smsHistory")]
        public ActionResult<ResponseBuilder> SmsHistory()
        {
            long accountId = _accountSession.AccountID;
            string accountName = _accountSession.AccountName;

            try
            {
                string fromDate = DateTime.Now.AddDays(-30).ToString("dd-MM-yyyy");
                string toDate = DateTime.Now.ToString("dd-MM-yyyy");
                
                var obj = new
                {
                    accountId = accountId,
                    fromDate = fromDate,
                    toDate = toDate
                };

                using (var client = new HttpClient())
                {
                    var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/sms_history", _appSettings.ReChargeGetwayUrl));
                    var dataRequest = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(obj));
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = dataRequest.Length;
                    request.Headers.Add("X-Charging-API-Key", "1");

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(dataRequest, 0, dataRequest.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();
                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, responseString);
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("chargingHistory")]
        public ActionResult<ResponseBuilder> ChargingHistory()
        {
            try
            {
                int fromDate = Int32.Parse(DateTime.Now.AddDays(-30).ToString("yyyyMMdd"));
                int toDate = Int32.Parse(DateTime.Now.ToString("yyyyMMdd"));

                var obj = new
                {
                    fromDate,
                    toDate
                };

                // Gọi sang cổng nạp thẻ
                // Lấy ra header token để gửi đi
                var header = new WebHeaderCollection
                {
                    { "Authorization", HttpContext.Request.Headers["Authorization"] }
                };
               
                var jsonData = JsonConvert.SerializeObject(obj);

                string url = string.Format("{0}/ChargingHistory", _appSettings.ReChargeGetwayUrl);
                var res = HttpUtil.SendPostWithHeader(jsonData, url, header);
                if (res == null)
                    return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);

                var resObject = JsonConvert.DeserializeObject<ResponseBuilder>(res);

                return resObject;
            }
            catch (System.Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);
        }
        #endregion

        [Authorize]
        [HttpPost("buyCard")]
        public ActionResult<ResponseBuilder> BuyCard([FromBody]dynamic data)
        {
            if (_appSettings.CashoutLock)
                return new ResponseBuilder(ErrorCodes.MC_CASHOUT_LOCK, _accountSession.Language);

            long accountID = _accountSession.AccountID;
            string accountName = _accountSession.AccountName;
            try
            {
                string cardType = data.cardType;//VTT, VNP, VMS
                string captchaText = data.captchaText;
                string captchaToken = data.captchaToken;
                int amount = data.amount;
                int merchantId = data.merchantId;
                int sourceId = Convert.ToInt32(data.sourceId);
                int quantity = data.quantity;

                // Verify OTP trực tiếp
                int otpType = data.otpType;
                string otp = data.otp;

                if (_captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
                    return new ResponseBuilder(ErrorCodes.VERIFY_CODE_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(cardType))
                    return new ResponseBuilder(ErrorCodes.PROVIDER_INVALID, _accountSession.Language);

                if (amount > 500000 || amount < 10000)
                    return new ResponseBuilder(ErrorCodes.PRICE_INVALID, _accountSession.Language);

                var account = _coreAPI.GetAccount((int)accountID, accountName);
                if(account.TotalCoin < amount)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_BALANCE_NOT_ENOUGH, _accountSession.Language);

                // Verify OTP
                var checkOTP = _otp.CheckOTP((int)accountID, accountName, otp, otpType);
                if (checkOTP != OTPCode.OTP_VERIFY_SUCCESS)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_INVALID, _accountSession.Language);

                // Gọi sang cổng nạp thẻ
                // Lấy ra header token để gửi đi
                var header = new WebHeaderCollection
                {
                    { "Authorization", HttpContext.Request.Headers["Authorization"] }
                };

                dynamic cardData = new
                {
                    CardType = cardType,
                    Amount = amount,
                    SourceID = _accountSession.PlatformID
            };
                var jsonData = JsonConvert.SerializeObject(cardData);

                string url = string.Format("{0}/BuyCard", _appSettings.ReChargeGetwayUrl);
                var res = HttpUtil.SendPostWithHeader(jsonData, url, header);
                if (res == null)
                    return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
                var resObject = JsonConvert.DeserializeObject<object>(res);
                if (resObject.responseStatus == 0)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, resObject);
                else if (resObject.responseStatus == -18)
                {
                    return new ResponseBuilder(ErrorCodes.LIMIT_DAY_TOPUP, _accountSession.Language, resObject);
                }

                return new ResponseBuilder(ErrorCodes.TRANSACTION_FAILED, _accountSession.Language, resObject.responseDesc);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.TRANSACTION_FAILED, _accountSession.Language);
        }

        [Authorize]
        [HttpPost("transfer")]
        public ActionResult<ResponseBuilder> Transfer([FromBody]dynamic data)
        {
            //if (Config.TransferLock)
            //    return new ResponseBuilder(ErrorCodes.BadRequest, MAINTAIN_MESSAGE, "BadRequest");

            int accountID = (int)_accountSession.AccountID;
            string accountName = _accountSession.AccountName;
            string ipAddress = _accountSession.IpAddress;

            if (_accountSession.NickName == null || _accountSession.NickName.Length < 5)
                return new ResponseBuilder(ErrorCodes.NICK_NAME_SEND_NOT_MATCH, _accountSession.Language);

            try
            {
                //int accountIdTrans = data.accountIdTrans;
                string nickNameTrans = data.nickNameTrans;
                int transferValue = data.transferValue;
                string nickNameRecv = data.nickNameRecv;
                int sourceId = data.sourceId;
                string captchaText = data.captchaText;
                string captchaToken = data.captchaToken;
                int transferType = data.transferType;
                //int otpType = data.otpType;
                //string otp = data.otp;
                string reason = data.reason;

                if (_captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
                    return new ResponseBuilder(ErrorCodes.VERIFY_CODE_INVALID, _accountSession.Language);

                //if (accountIdTrans != accountID)
                //    return new ResponseBuilder(ErrorCodes.BadRequest, "Tên tài khoản chưa đăng nhập.", "BadRequest");

                if (nickNameTrans.CompareTo(nickNameRecv) == 0)
                    return new ResponseBuilder(ErrorCodes.CAN_NOT_TRANSFER_TO_YOURSELF, _accountSession.Language);

                if (transferValue < MIN_BON || transferValue > MAX_BON)
                    return new ResponseBuilder(ErrorCodes.BON_VALUE_INVALID, _accountSession.Language);

                var accountInfo = _coreAPI.GetAccount(accountID, accountName);
                if (accountInfo == null)
                    return new ResponseBuilder(ErrorCodes.SEND_ACCOUNT_NOT_EXIST, _accountSession.Language);

                if (accountInfo.UserFullname.CompareTo(nickNameTrans) != 0)
                    return new ResponseBuilder(ErrorCodes.NICK_NAME_SEND_NOT_MATCH, _accountSession.Language);

                int accountIdReceiver = -1;
                string accountNameReceive = "";
                int accountReceive = _coreAPI.GetAccountByNickName(nickNameRecv, out accountIdReceiver, out accountNameReceive);
                if (accountReceive <= 0)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_EXIST, _accountSession.Language);

                if (accountInfo.TotalCoin < transferValue)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_BALANCE_NOT_ENOUGH, _accountSession.Language);

                if (_otp.IsOTP(accountID))
                {
                    string serviceData = JsonConvert.SerializeObject(new TransferData
                    {
                        AccountIdTrans = accountID,
                        IpAddress = ipAddress,
                        NickNameRecv = nickNameRecv,
                        NickNameTrans = nickNameTrans,
                        Reason = reason,
                        SourceId = sourceId,
                        TransferType = transferType,
                        TransferValue = transferValue
                    });

                    string token = OTPSecurity.CreateOTPToken(accountName, accountID, (int)OtpServiceCode.OTP_SERVICE_TRANSFER, sourceId, serviceData);
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, new Models.OTPDataReturn(token));
                }
                else
                {
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, new Models.OTPDataReturn());
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.TRANSFER_COIN_FAILED, _accountSession.Language);
        }
        /// <summary>
        /// Nạp thẻ
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("chargingCard")]
        public ActionResult<ResponseBuilder> ChargingCard([FromBody]dynamic data)
        {
            //if (Config.ChargingCardLock)
            //    return new ResponseBuilder(ErrorCodes.BadRequest, MAINTAIN_MESSAGE, "BadRequest");

            if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
                return new ResponseBuilder((int)ErrorCodes.SYSTEM_ERROR, _accountSession.Language, "Sai mã token");

            string cardType = data.cardType; // 
            string cardSerial = data.cardSerial;
            string cardPin = data.cardPin;
            string amount = data.amount;
            string userName = _accountSession.AccountName;
            string captcha = data.captcha;
            string verify = data.verify;
            int os = data.os;

            cardPin = cardPin.Replace("-", "");
            cardPin = cardPin.Replace(" ", "");
            cardSerial = cardSerial.Replace("-", "");
            cardSerial = cardSerial.Replace(" ", "");

            try
            {
                if (_captcha.VerifyCaptcha(captcha, verify) < 0)
                    return new ResponseBuilder(ErrorCodes.VERIFY_CODE_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(cardType) || string.IsNullOrEmpty(cardSerial) || string.IsNullOrEmpty(cardPin) || string.IsNullOrEmpty(userName))
                    return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);

                // Lấy ra header token để gửi đi
                var header = new WebHeaderCollection
                {
                    { "Authorization", HttpContext.Request.Headers["Authorization"] }
                };

                dynamic cardData = new ExpandoObject();
                cardData.CardType = cardType;
                cardData.Amount = amount;
                cardData.CardCode = cardPin;
                cardData.CardSerial = cardSerial;
                cardData.SourceID = _accountSession.PlatformID;
                var jsonData = JsonConvert.SerializeObject(cardData);

                string url = string.Format("{0}/TopupCard", _appSettings.ReChargeGetwayUrl);
                var res = HttpUtil.SendPostWithHeader(jsonData, url, header);
                if(res == null)
                    return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
                var resObject = JsonConvert.DeserializeObject<object>(res);
                if(resObject.responseStatus == 0)
                    return new ResponseBuilder(ErrorCodes.TOPUP_CARD_SUCCESS, _accountSession.Language, resObject);

                return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language, resObject.responseDesc);

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language, ex.Message);
            }
          
        }

        [Authorize]
        [HttpGet("napXuHistory")]
        public ActionResult<ResponseBuilder> GetTopupXuHistory()
        {
            try
            { 

                var list = _coreAPI.GetTopupXuHistory((int)_accountSession.AccountID);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, list);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("getTransferHistory")]
        public ActionResult<ResponseBuilder> GetTransferHistory()
        {
            try
            {
                var list = _coreAPI.GetTransferBonHistory((int)_accountSession.AccountID);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language,list);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }
        /// <summary>
        /// Lấy tỷ lệ đổi thẻ
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetCardRate")]
        public ActionResult<ResponseBuilder> GetCardRate(int type)
        {
            try
            {
                string url = string.Format("{0}/GetCardRate?type={1}", _appSettings.ReChargeGetwayUrl, type);
                var list = HttpUtil.GetStringHttpResponse(url);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.DeserializeObject<IEnumerable<object>>(list));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("GetCardInfo")]
        public ActionResult<ResponseBuilder> GetCardInfo()
        {
            try
            {
                string url = string.Format("{0}/GetCardInfo", _appSettings.ReChargeGetwayUrl);
                var list = HttpUtil.GetStringHttpResponse(url);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.DeserializeObject<object>(list));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpGet]
        [Route("iap")]
        public ActionResult<ResponseBuilder> ChargingByIAP(dynamic data)
        {
            try
            {
                NLogManager.Info("iapData: " + JsonConvert.SerializeObject(data));

                string userName = data.uname;
                string receipt = data.receipt;
                string uuid = data.uuid;

                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, ErrorCodes.IAP_TOPUP_SUCCESS);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }
    }
}