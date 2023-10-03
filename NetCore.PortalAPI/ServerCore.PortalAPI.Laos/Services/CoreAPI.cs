
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using PortalAPI.Models;

using ServerCore.Utilities.Models;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Utils;
using ServerCore.PortalAPI.OTP;
using ServerCore.Utilities.Facebook;
using ServerCore.Utilities.Security;
using ServerCore.PortalAPI.Models;
using Microsoft.Extensions.Options;
using ServerCore.Utilities.Interfaces;
using ServerCore.Utilities;

namespace PortalAPI.Services
{
    public class CoreAPI
    {
        private static readonly int SERVICE_ID_CHARING = 100000;
        private static readonly int SERVICE_ID_BUYCARD = 100001;

        private static readonly int SERVICE_ID_FROZEN = 1000005;
        private static readonly int SERVICE_ID_UNFROZEN = 1000006;

        private static readonly string SERVICE_KEY_FROZEN = "e33f46521d3b6b511f727e29fe13f7da";
        private static readonly string SERVICE_KEY_UNFROZEN = "a49ec813926d7a09ceea4a646f7a962b";

        private static readonly string SERVICE_KEY = "123456";

        private readonly IAccountDAO _accountDAO;
        private readonly IGameTransactionDAO _gameTransactionDAO;
        private readonly IEventDAO _eventDAO;
        private readonly IReportDAO _report;
        private readonly AccountSession _accountSession;
        private readonly AppSettings _appSettings;
        private readonly IAgencyDAO _agencyDAO;


        public CoreAPI(IOptions<AppSettings> options, IEventDAO eventDAO, IAccountDAO accountDAO, IReportDAO report, IGameTransactionDAO gameTransactionDAO, AccountSession accountSession, IAgencyDAO agencyDAO)
        {
            this._appSettings = options.Value;
            this._eventDAO = eventDAO;
            this._report = report;
            this._gameTransactionDAO = gameTransactionDAO;
            this._accountSession = accountSession;
            this._accountDAO = accountDAO;
            this._agencyDAO = agencyDAO;
        }

        public AccountDb Login(int platformId, int productId, string hashKey, string ipAddress, string userName, string authKey, string uiid)
        {
            try
            {
                if(productId <= 0 || string.IsNullOrEmpty(hashKey) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(authKey))
                    return null;

                NLogManager.Info(string.Format("INPUT: productId={0}, key={1}, ipAddress={2}, userName={3}, authKey={4}, uiid={5}, platformId={6}", productId, hashKey, ipAddress, userName, authKey, uiid, platformId));
                int response;
                AccountDb account = _accountDAO.Login(userName, hashKey, ipAddress, uiid, productId, platformId, out response);
                if(account == null || account.AccountID <= 0)
                    return account;

                account.PlatformID = platformId;
                account.ClientIP = ipAddress;
                if (account.IsOtp == 0)
                {
                    account.PlatformID = platformId;
                    account.ClientIP = ipAddress;
                }
                else
                {
                    string serviceData = JsonConvert.SerializeObject(new LoginData
                    {
                        PlatformId = platformId,
                        ProductId = productId,
                        AuthKey = authKey,
                        IpAddress = ipAddress,
                        SecretKey = hashKey,
                        UserName = userName
                    });
                    account.OtpToken = OTPSecurity.CreateOTPToken(account.UserName, (int)account.AccountID, (int)OtpServiceCode.OTP_SERVICE_LOGIN, platformId, serviceData);
                }
                return account;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        public AccountDb Login(LoginData loginData)
        {
            return Login(loginData.PlatformId, loginData.ProductId, loginData.SecretKey, loginData.IpAddress, loginData.UserName, loginData.AuthKey, loginData.UIID);
        }

        public AccountDb LoginFacebook(string accessToken, string facebookId, string facebookEmail, int serviceId, int platformId, int productId, string ip, string uiid = "")
        {
            try
            {
                string graphApi = string.Format("https://graph.facebook.com/{0}?fields=email&access_token={1}", facebookId, accessToken);
                var data = HttpUtil.GetAsync(graphApi);
                if (data == null)
                {
                    NLogManager.Error("Không tìm thấy thông tin tài khoản Facebook");
                    return null;
                }
                FBAccount fbInfor = null;// FacebookUtil.GetFBAccount(data.ToString());
                List<IDs_Business> partnerIDS = null;// FacebookUtil.GetIDsForBusiness(accessToken);

                if (fbInfor.Id != facebookId)
                {
                    NLogManager.Error("Tài khoản Facebook không hợp lệ");
                    return null;
                }

                string listPartnerIDs = "";
                foreach (var item in partnerIDS)
                    listPartnerIDs += ";" + item.id;

                if (listPartnerIDs.IndexOf(";") == 0)
                    listPartnerIDs = listPartnerIDs.Substring(1);

                var accountChanneling = _accountDAO.SelectAccountByFacebook(3, fbInfor.Id, fbInfor.Email, listPartnerIDs, Convert.ToInt32(serviceId), Convert.ToInt32(platformId), productId, uiid);
                if (accountChanneling == null || accountChanneling.ChannelingID <= 0)
                {
                    NLogManager.Error("accountChanneling Error");
                    return null;
                }

                AccountDb account = _accountDAO.GetAccount(accountChanneling.AccountID, accountChanneling.AccountName);
                if (account == null || account.AccountID < 1)
                {
                    NLogManager.Error("Tài khoản không tồn tại");
                    return null;
                }

                account.PlatformID = Convert.ToInt32(platformId);
                account.ClientIP = ip;

                return account;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        public long Transfer(int accountIdTrans, string nickNameTrans, int transferValue, long accountIdRecv, string nickNameRecv, int transferType, string ipAddress, int sourceId, string reason, int isWebAgency, out long balanceReceive, out long receiveAmount)
        {
            balanceReceive = -1;
            receiveAmount = 0;
            try
            {
                long balanceUserTrans = 0;
                int status = _gameTransactionDAO.TransferBon(accountIdTrans, nickNameTrans, transferValue, accountIdRecv, nickNameRecv, transferType, ipAddress, sourceId, reason, isWebAgency, out balanceUserTrans, out balanceReceive, out receiveAmount);
                string log = string.Format("accountIdTrans = {0}, nickNameTrans = {1}, transferValue = {2}, nickNameRecv = {3}, reason = {4}, status = {5}",
                        accountIdTrans, nickNameTrans, transferValue, nickNameRecv, reason, status);
                if(status >= 0)
                    NLogManager.Info(log);
                else
                    NLogManager.Error(log);

                if(status >= 0)
                {
                    //AccountDb accountDb = GetAccountByNickName(nickNameRecv);
                    //if(accountDb != null)
                    //{
                    //    BulkSendSmsTranferAgency bulkSendSmsTranferAgency = new BulkSendSmsTranferAgency
                    //    {
                    //        Amount = transferValue.ToString("#,##0"),
                    //        AmountType = "+",
                    //        Balance = balanceUserTrans.ToString("#,##0"),
                    //        FromNickName = nickNameTrans,
                    //        ToNickName = nickNameRecv,
                    //        ToMobile = accountDb.Mobile
                    //    };
                    //    SendSmsTranferAgency(bulkSendSmsTranferAgency);
                    //}
                    return balanceUserTrans;
                }
                else
                    return status;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountIdTrans"></param>
        /// <param name="nickNameTrans"></param>
        /// <param name="transferValue"></param>
        /// <param name="nickNameRecv"></param>
        /// <param name="transferType"></param>
        /// <param name="ipAddress"></param>
        /// <param name="sourceId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public int GetTransferRate(string nickNameTrans, string nickNameRecv, out double rate)
        {
            rate = 0;
            try
            {
                int type = 0;
                int levelId = 0;
               
                //// Check người gửi: nếu là đại lý thì lấy ra levelId, nếu không thì return
                //AgenciesInfo sourceAgency = _agencyDAO.AgencyGetInfo(nickNameTrans);
                //if (sourceAgency.AgencyID <= 0)
                //    return (int) ErrorCodes.AGENCY_ID_INVALID;
                //levelId = sourceAgency.LevelID;

                //// Check người nhận: nếu không là đại lý => type = 0, nếu là đại lý, type = 1
                //AgenciesInfo recvAgency = _agencyDAO.AgencyGetInfo(nickNameRecv);
                //if (recvAgency.AgencyID <= 0) type = 0; else type = 1;

               
                rate = _gameTransactionDAO.GetTransferRateNew(nickNameTrans, nickNameRecv);
                return (int)ErrorCodes.SUCCESS; 
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return (int)ErrorCodes.SERVER_ERROR;
            }
        }

        /// <summary>
        /// Mua thẻ, Topup cho số điện thoại
        /// </summary>
        /// <param name="money">Số tiền mua thẻ/Topup</param>
        /// <param name="phoneNumber">Số điện thoại cần Topup</param>
        /// <param name="type">type = 0: mua the,type = 1: trả trước, 2: trả sau</param>
        /// <param name="provider">VTT, VNP, VMS</param>
        /// <param name="quantity">Số lượng thẻ</param>
        /// <param name="sourceId">1 = android, 2 = win32, 3 = ios, 4 = web</param>
        public string BuyCard(string userName, string provider, int type, int money, string phoneNumber, int accountId, int merchantId, int sourceId, int quantity)
        {
            try
            {
                var obj = new
                {
                    merchantId = SERVICE_ID_BUYCARD,
                    merchantKey = SERVICE_KEY,
                    provider = provider,
                    amount = money,
                    userName = userName,
                    accountId = accountId,
                    type = type,
                    phoneNumber = phoneNumber,
                    sourceId = sourceId,
                    clientIp = _accountSession.IpAddress,
                    quantity = quantity
                };

                using (var client = new HttpClient())
                {
                    string url = string.Format("{0}/buycard", _appSettings.ReChargeGetwayUrl);
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    string dataFromObj = JsonConvert.SerializeObject(obj);
                    var dataRequest = Encoding.ASCII.GetBytes(dataFromObj);
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
                    NLogManager.Info("INPUT: " + dataFromObj);
                    NLogManager.Info("OUTPUT: " + responseString);
                    return responseString;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        /// <summary>
        /// Đổi TotalCoin sang Xu
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="ipAddress"></param>
        /// <param name="bon">Số TotalCoin muốn đổi</param>
        /// <returns> true nếu thành công, false nếu thất bại</returns>
        public bool TransferQToXu(long accountId, string ipAddress, int bon, out long totalXu, out long xu, out long totalBon)
        {
            totalXu = -1;
            xu = -1;
            totalBon = -1;

            try
            {
                long res = _gameTransactionDAO.TranferStarToCoin(accountId, ipAddress, bon, out totalXu, out xu, out totalBon);
                if (res >= 0)
                    return true;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return false;
        }

        /// <summary>
        /// Cập nhật thông tin bảo mật của tài khoản
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="email"></param>
        /// <param name="mobile"></param>
        /// <param name="passport"></param>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        public bool UpdateSecurityInfo(long accountId, string email, string mobile, string passport, int avatarId)
        {
            try
            {
                int res = _accountDAO.UpdateInfo(accountId, email, mobile, passport, avatarId);
                if (res >= 0)
                    return true;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return false;
        }

        /// <summary>
        /// Cap nhat so dien thoai
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="mobile"></param>
        /// <returns> -102: Số điện thoại đã tồn tại</returns>
        public int UpdateMobile(long accountId, string mobile)
        {
            try
            {
                int res = _accountDAO.UpdateMobile(accountId, mobile);
                NLogManager.Info(string.Format("accountId = {0}, mobile = {1}, res = {2}", accountId, mobile, res));
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1;
        }

        public int DeleteMobile(long accountId, string mobile)
        {
            try
            {
                int res = _accountDAO.DeleteMobile(accountId, mobile);
                NLogManager.Info(string.Format("accountId = {0}, mobile = {1}, res = {2}", accountId, mobile, res));
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1;
        }

        public int UpdateAvatar(long accountId, int avatar)
        {
            try
            {
                int res = _accountDAO.UpdateInfo(accountId, "", "", "", avatar);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1;
        }

        /// <summary>
        /// Thay đổi mật khẩu
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="oldPassWord"></param>
        /// <param name="newPassWord"></param>
        /// <param name="currentPassWord"></param>
        /// <returns> -1: Không tồn tại tài khoản, -2: Mật khẩu cũ không đúng</returns>
        public int ChangePassWord(long accountId, string accountName, string oldPassWord, string newPassWord)
        {
            try
            {
                var account = _accountDAO.Get(accountId);
                if (account == null || account.AccountID <= 0)
                    return -1;

                // oldPass check
                //if (Security.MD5Encrypt(oldPassWord).CompareTo(account.Password) != 0)
                //    return -2;

                string newPassEnc = Security.TripleDESEncrypt(newPassWord, newPassWord);
                int resultChange = _accountDAO.ChangePassword(accountId, Security.TripleDESEncrypt(oldPassWord, oldPassWord), newPassEnc);
                NLogManager.Info("accountId: " + accountId + ", result: " + resultChange);
                if (resultChange >= 0)
                {
                    //Insert thôgn tin đổi mật khẩu vào Log để window service tự động đổi mật khẩu các Game
                    //var writelog = _accountDAO.LogAccountChangepassWaiting_Insert(accountId, accountName, Security.MD5Encrypt(oldPassWord), Security.MD5Encrypt(newPassWord));
                    //if (writelog >= 0)
                    //    return resultChange;

                    // insert thông tin không thành công thì lại cập nhập lại pass cũ
                    //_accountDAO.ChangePassword(accountId, newPassEnc, account.Password);
                }
                return resultChange;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1;
        }

        /// <summary>
        /// Kiem tra Giftcode
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="nickName"></param>
        /// <param name="giftCode"></param>
        /// <param name="merchantId"></param>
        /// <param name="merchantKey"></param>
        /// <param name="sourceId"></param>
        /// <param name="ipAddress"></param>
        /// <param name="balance"></param>
        /// <returns>
        /// ERROR_NOT_EXIST_MERCHANTID INT = -90
        /// ERROR_GIFCODE_USING INT = -100
        /// ERROR_GIFCODE_NOT_EXIST INT = -101
        /// ERROR_GIFCODE_NOT_TIME INT = -102
        /// ERROR_GIFCODE_EXPIRE INT = -103
        /// ERROR_GIFCODE_LOCK INT = -104
        /// ERROR_GIFCODE_OVERLIMIT INT = -105
        /// ERROR_GIFCODE_FAILED_SOURCE INT = -106
        /// </returns>
        public int CheckGiftCode(long accountId, string accountName, string nickName, string giftCode, int merchantId, string merchantKey, int sourceId, string ipAddress, out long balance)
        {
            balance = -1;
            int res = _eventDAO.CheckGiftCode(accountId, accountName, nickName, giftCode, merchantId, merchantKey, sourceId, ipAddress, out balance);
            NLogManager.Info(string.Format("result = {0}, accountId = {1}, accountName = {2}, giftCode = {3}, merchantId = {4}, sourceId = {5}", res, accountId, accountName, giftCode, merchantId, sourceId));
            return res;
        }

        public AccountDb GetAccount(long accountId, string accountName)
        {
            if (accountId <= 0 || string.IsNullOrEmpty(accountName))
                return null;

            return _accountDAO.GetAccount(accountId, accountName);
        }

        public int RegisterOTP(long accountId, string userName, string mobile, int register)
        {
            return _accountDAO.RegisterOTP(accountId, userName, mobile, register);
        }

        /// <summary>
        /// Lấy lịch sử giao dịch của người chơi
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="type"> 1: BON; 2 = XU</param>
        public dynamic GetTransactionHistory(long accountId, int limit)
        {
            if (limit > 100)
                return null;
            try
            {
                using (var client = new HttpClient())
                {
                    string url = string.Format("{0}/account/transaction/top?accountId={1}&top={2}", _appSettings.ReportServiceApiUrl, accountId, limit);
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "application/json";

                    var response = (HttpWebResponse)request.GetResponse();
                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    ResponseApiData rs = JsonConvert.DeserializeObject<ResponseApiData>(responseString);
                    if(rs.code == 0)
                    {
                        return rs.data;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        public List<TopupXuHistory> GetTopupXuHistory(int accountId)
        {
            if (accountId > 0)
                return _gameTransactionDAO.GetTopupXuHistory(accountId);

            return new List<TopupXuHistory>();
        }

        /// <summary>
        /// Đóng/Mở băng số dư tài khoản
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="bonFrozen"> số bon đóng/mở băng</param>
        /// <param name="frozen"> true: đóng băng, false: mở băng</param>
        /// <returns></returns>
        public long Frozen(long accountId, long bonFrozen, bool frozen, int sourceId, string clientIP, out long balance, out long frozenValue)
        {
            long res = _accountDAO.Frozen(accountId, bonFrozen, frozen ? _appSettings.ServiceIdFrozen : SERVICE_ID_UNFROZEN, frozen ? SERVICE_KEY_FROZEN : SERVICE_KEY_UNFROZEN, frozen, sourceId, clientIP, out balance, out frozenValue);
            NLogManager.Info(string.Format("accountId = {0}, bonFrozen = {1}, frozen = {2}, balance = {3}, res = {4}", accountId, bonFrozen, frozen, balance, res));
            return res;
        }

        public long GetFrozenValue(long accountId, out long balance)
        {
            return _accountDAO.GetFrozenValue(accountId, out balance);
        }

        public List<TransferBonHistory> GetTransferBonHistory(int accountId)
        {
            if (accountId > 0)
                return _gameTransactionDAO.GetTransferBonHistory(accountId);

            return new List<TransferBonHistory>();
        }

        public int UpdateFireBaseKey(long accountId, string firebaseKey, string uiid, int productId)
        {
            //NLogManager.Info(string.Format("accountId = {0}, FireBaseKey = {1}, uiid = {2}, productId = {3}", accountId, firebaseKey, uiid, productId));
            return 0;
        }

        public int GetAccountByNickName(string nickName, out int accountId, out string userName)
        {
            return _accountDAO.GetAccountByNickName(nickName, out userName, out accountId);
        }

        public AccountDb GetAccountByNickName(string nickName)
        {
            return _accountDAO.GetAccountByNickName(nickName);
        }

        public int SendSmsTranferAgency(BulkSendSmsTranferAgency bulkSendSmsTranferAgency)
        {
            try
            {
                if(bulkSendSmsTranferAgency == null)
                    return -1;

                using(var client = new HttpClient())
                {
                    //var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/smsapi", appSettings.SmstUrl));
                    string url = string.Format("{0}/sms/BulkSendSmsTranferAgency", _appSettings.SmstUrl);
                    NLogManager.Info("url: " + url);
                    NLogManager.Info("INPUT: " + JsonConvert.SerializeObject(bulkSendSmsTranferAgency));
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    var dataReq = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(bulkSendSmsTranferAgency, Formatting.Indented));
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = dataReq.Length;

                    using(var stream = request.GetRequestStream())
                    {
                        stream.Write(dataReq, 0, dataReq.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();
                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    NLogManager.Info("OUTPUT: " + responseString);

                    //long endTime = DateTime.Now.Ticks;
                    //TimeSpan diffTime = new TimeSpan(endTime - startTime);
                    //NLogManager.Info("Time_ms: " + diffTime.TotalMilliseconds);

                    return 0;
                }
            }
            catch(Exception ex)
            {
                NLogManager.Error(ex.ToString());
            }
            return -1;
        }

        public bool GetChatStatus(long accountID)
        {
            return _accountDAO.GetChatStatus(accountID) == 1 ? true : false;
        }

        public string CheckCardMaintain(string cardType, int topupType)
        {
            return _report.CheckCardMaintain(cardType, topupType);
        }

        /// <summary>
        /// Lấy thông tin vippoint
        /// </summary>
        /// <param name="type">type = 0 (ngày), type = 1 (tháng), = null (all)</param>
        public dynamic GetVpLevel(int type)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = string.Format("{0}/vp-level?type={1}", _appSettings.LoyaltyServiceApiUrl, type);
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "application/json";

                    var response = (HttpWebResponse)request.GetResponse();
                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    ResponseApiData rs = JsonConvert.DeserializeObject<ResponseApiData>(responseString);
                    if (rs.code == 0)
                    {
                        return rs.data;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        /// <summary>
        /// Lấy thông tin vippoint
        /// </summary>
        /// <param name="accountID">AccountID</param>
        public dynamic FindVipPointInfo(long accountID)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = string.Format("{0}/account/vp/info?accountId={1}", _appSettings.LoyaltyServiceApiUrl, accountID);
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "application/json";

                    var response = (HttpWebResponse)request.GetResponse();
                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    ResponseApiData rs = JsonConvert.DeserializeObject<ResponseApiData>(responseString);
                    if (rs.code == 0)
                    {
                        return rs.data;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        /// <summary>
        /// Lấy thông tin vippoint
        /// </summary>
        /// <param name="accountID">AccountID & top</param>
        public dynamic FindTopAccountVpTransaction(long accountID, int top)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = string.Format("{0}/account/vp/top-reward?accountId={1}&top={2}", _appSettings.LoyaltyServiceApiUrl, accountID, top);
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "application/json";

                    var response = (HttpWebResponse)request.GetResponse();
                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    ResponseApiData rs = JsonConvert.DeserializeObject<ResponseApiData>(responseString);
                    if (rs.code == 0)
                    {
                        return rs.data;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        /// <summary>
        /// Lấy thông tin vippoint
        /// </summary>
        /// <param name="accountID">AccountID & top</param>
        public dynamic FindTopRankVipPointLevelInfo(long accountID, int top, int type)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(2);
                    string url = string.Format("{0}/account/vp/rank-top?accountId={1}&top={2}&type={3}", _appSettings.LoyaltyServiceApiUrl, accountID, top, type);
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "application/json";

                    var response = (HttpWebResponse)request.GetResponse();
                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    ResponseApiData rs = JsonConvert.DeserializeObject<ResponseApiData>(responseString);
                    if (rs.code == 0)
                    {
                        return rs.data;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        /// <summary>
        /// Lấy thông tin vippoint
        /// </summary>
        /// <param name="accountID">AccountID & top</param>
        public dynamic SendPopup(long accountID, string content, long balance, int type)
        {
            return null;
            try
            {
                using (var client = new HttpClient())
                {
                    string url = string.Format("{0}?accountId={1}&content={2}&balance={3}&type={4}", _appSettings.PopupServiceApiUrl, accountID, content, balance, type);
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "application/json";

                    var response = (HttpWebResponse)request.GetResponse();
                    //string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    //ResponseApiData rs = JsonConvert.DeserializeObject<ResponseApiData>(responseString);
                    //if (rs.code == 0)
                    //{
                    //    return rs.data;
                    //}
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }
    }
}
