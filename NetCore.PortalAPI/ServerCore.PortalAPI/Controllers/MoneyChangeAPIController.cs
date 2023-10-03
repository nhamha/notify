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
using Newtonsoft.Json.Linq;
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
        private readonly IAccountDAO _accountDAO;
        private readonly IAgencyDAO _agencyDAO;
        private readonly Captcha _captcha;


        public MoneyChangeAPIController(AccountSession accountSession, OTPSecurity otp, CoreAPI coreAPI, IOptions<AppSettings> options, IGameTransactionDAO coreDAO, IAccountDAO accountDAO, IAgencyDAO agencyDAO, Captcha captchaUtil)
        {
            this._accountSession = accountSession;
            this._otp = otp;
            this._coreAPI = coreAPI;
            this._appSettings = options.Value;
            this._coreDAO = coreDAO;
            this._accountDAO = accountDAO;
            this._agencyDAO = agencyDAO;
            _captcha = captchaUtil;
        }

        #region [Lịch sử giao dịch]
        [Authorize]
        [HttpGet("getTransactionLogs")]
        public ActionResult<ResponseBuilder> GetTransactionLogs(int limit)
        {
            try
            {
                var list = _coreAPI.GetTransactionHistory((int)_accountSession.AccountID, limit);

                if (list != null)
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, list);
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

                //if (Captcha.VerifyCaptcha(captchaText, captchaToken) < 0)
                //    return new ResponseBuilder(ErrorCodes.VERIFY_CODE_INVALID, _accountSession.Language);

                if (string.IsNullOrEmpty(cardType))
                    return new ResponseBuilder(ErrorCodes.PROVIDER_INVALID, _accountSession.Language);

                if (amount > 500000 || amount < 10000)
                    return new ResponseBuilder(ErrorCodes.PRICE_INVALID, _accountSession.Language);

                var account = _coreAPI.GetAccount((int)accountID, accountName);
                if (account.TotalCoin < amount)
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

                else if (resObject.responseStatus == -102)
                {
                    return new ResponseBuilder(ErrorCodes.LIMIT_BUY_IN_DAY, _accountSession.Language, resObject);
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
            if (_accountSession.AccountID <= 0 || _accountSession.NickName == null)
                return new ResponseBuilder((int)ErrorCodes.DATA_INVALID, _accountSession.Language, "Sai mã token");

            int accountID = (int)_accountSession.AccountID;
            string accountName = _accountSession.AccountName;
            string ipAddress = _accountSession.IpAddress;

            try
            {
                //int accountIdTrans = data.accountIdTrans;
                string nickNameTrans = data.nickNameTrans;
                nickNameTrans = nickNameTrans.ToLower();
                int transferValue = data.transferValue;
                string nickNameRecv = data.nickNameRecv;
                nickNameRecv = nickNameRecv.ToLower();
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

                var securityInfo = _accountDAO.GetSecurityInfo(accountID);
                // Tài khoản chưa đăng ký bảo mật OTP
                if (securityInfo == null)
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER_YET, _accountSession.Language, "Bạn chưa đăng ký bảo mật số điện thoại");

                if (securityInfo.Mobile == null || securityInfo.Mobile.Length < 1 || (!securityInfo.IsMobileActived))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_OTP_NOT_REGISTER_YET, _accountSession.Language, "Bạn chưa đăng ký bảo mật số điện thoại");

                var accountInfo = _coreAPI.GetAccount(accountID, accountName);
                if (accountInfo == null)
                    return new ResponseBuilder(ErrorCodes.SEND_ACCOUNT_NOT_EXIST, _accountSession.Language);

                if (accountInfo.UserFullname.ToLower().CompareTo(nickNameTrans) != 0)
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
                if (res == null)
                    return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
                var resObject = JsonConvert.DeserializeObject<object>(res);
                if (resObject.responseStatus == 0)
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
        [HttpGet("getTransferHistory")]
        public ActionResult<ResponseBuilder> GetTransferHistory()
        {
            try
            {
                var list = _coreAPI.GetTransferBonHistory((int)_accountSession.AccountID);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, list);
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
        [HttpGet("GetMomoInfo")]
        public ActionResult<ResponseBuilder> GetMomoInfo()
        {
            try
            {
                string url = string.Format("{0}/GetInfoMomo", _appSettings.ReChargeGetwayUrl);
                var list = HttpUtil.GetStringHttpResponse(url);
                NLogManager.Info("GetMomoInfo" + list);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, list);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        [Authorize]
        [HttpGet("GetBankInfo")]
        public ActionResult<ResponseBuilder> GetBankInfo()
        {
            try
            {
                var list = _agencyDAO.GetAgenciesBank();
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, list);
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
        [HttpPost]
        [Route("iap")]
        public ActionResult<ResponseBuilder> ChargingByIAP([FromBody] ProductIAP data)
        {
            try
            {
                NLogManager.Info("iapData: " + JsonConvert.SerializeObject(data));
                if (!string.IsNullOrEmpty(data.receiptCipheredPayload))
                {
                    int verifyStatus = vertifyRecieptApple(data, true);
                    if(verifyStatus != 0)
                    {
                        return new ResponseBuilder(ErrorCodes.IAP_TOPUP_FAIL, _accountSession.Language);
                    }

                    long accountID = _accountSession.AccountID;
                    string accountName = _accountSession.AccountName;

                    var account = _coreAPI.GetAccount((int)accountID, accountName);
                    string name = data.name;
                    string description = data.description;
                    string clientIP = _accountSession.IpAddress;
                    long balance = 0;
                    int status = -1;
                    name = name.Replace("k", "000");
                    int value = Convert.ToInt32(name);
                    int restatus = _coreDAO.TopupByIAP(accountID, accountName, value, description, clientIP, _accountSession.PlatformID, out balance, out status);
                    NLogManager.Info("restatus: " + restatus);
                    if (restatus >= 0)
                    {
                        var dataRes = new
                        {
                            Status = restatus,
                            Balance = balance,
                            Value = value
                        };
                        return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, dataRes);
                    }

                    return new ResponseBuilder(ErrorCodes.IAP_TOPUP_FAIL, _accountSession.Language);
                }

                return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        private int vertifyRecieptApple(ProductIAP receipt, bool isSandbox)
        {
            try
            {
                string SANBOX_APPLE_VERIFY_RECEIPT = "https://sandbox.itunes.apple.com/verifyReceipt";
                string APPLE_VERIFY_RECEIPT = "https://buy.itunes.apple.com/verifyReceipt";

                string request = "{\"receipt-data\":\"" + receipt.receiptCipheredPayload + "\"}";
                string res = HttpUtil.SendPost(request, isSandbox ? SANBOX_APPLE_VERIFY_RECEIPT : APPLE_VERIFY_RECEIPT);
                NLogManager.Info(res);
                JObject jObject = JObject.Parse(res);
                int status = (int)jObject["status"];
                if (status == 0)
                {
                    string bundleID = (string)jObject["receipt"]["bundle_id"];
                    dynamic in_app = jObject["receipt"]["in_app"][0];
                    string product_id = (string)in_app["product_id"];
                    string transaction_id = (string)in_app["transaction_id"];
                    string purchase_date_ms = (string)in_app["purchase_date_ms"];
                    if(bundleID.CompareTo("online.game.taric") == 0 && 
                        product_id.CompareTo(receipt.id) == 0 &&
                        transaction_id.CompareTo(receipt.transactionID) == 0)
                    {
                        return 0;
                    }
                }
                return status;
            }
            catch (Exception e)
            {
                NLogManager.Error(e.ToString());
                return -1;
            }
        }
    }
}