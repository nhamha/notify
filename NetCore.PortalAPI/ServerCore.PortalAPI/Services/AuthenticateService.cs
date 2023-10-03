using ServerCore.PortalAPI.Models;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore.Utilities.Security;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess;
using Newtonsoft.Json;
using ServerCore.PortalAPI.OTP;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using ServerCore.Utilities.Facebook;
using ServerCore.DataAccess.DTO;
using ServerCore.Utilities.Models;
using Microsoft.Extensions.Options;
using ServerCore.Utilities.Interfaces;
using ServerCore.Utilities.Sessions;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace ServerCore.PortalAPI.Services
{
    public interface IAuthenticateService
    {
        int Login(LoginAccount loginAccount, out AccountInfo accountInfo);
        Task<(int, AccountInfo)> LoginAsync(LoginAccount loginAccount);
        int GetInfo(long AccountID, string UserName, out AccountInfo accountInfo);

        int ReportLogin(LoginAccount loginAccount, out AccountInfo accountInfo);

        //int LoginFBAsync(LoginAccount loginAccount, out AccountInfo accountInfo);
        Task<AccountInfo> LoginFBAsync(LoginAccount loginAccount);
        Task<AccountInfo> LoginAppleIdAsync(LoginAccount loginAccount);
        int LoginVipCode(LoginAccount loginAccount, out AccountInfo accountInfo);
        int Register(LoginAccount loginAccount, out AccountInfo accountInfo);
        int Validate(string token, out AccountInfo accountInfo);
        string GenerateToken(AccountDb accountDb);
        string GenerateTokenFishing(AccountDb accountDb);
        bool CheckUserNameIsMobileOther(string mobile);

    }

    public class AuthenticateService : IAuthenticateService
    {
        // private readonly IEventDAO _eventDAO;//= AbstractDAOFactory.Instance().CreateEventDAO();
        private readonly IAccountDAO _accountDAO;// = AbstractDAOFactory.Instance().CreateAccountDAO();
        private readonly OTPSecurity _otp; //= OTPSecurity.Instance();
        private readonly IDataService _dataService;

        private readonly AppSettings appSettings;
        private readonly FacebookUtil _facebookUtil;
        private readonly IAgencyDAO _agencyDao;
        private readonly AccountSession _accountSession;
        private readonly IDistributedCache _cache;

        public AuthenticateService(AccountSession accountSession, IOptions<AppSettings> options, IEventDAO eventDAO, IAccountDAO accountDAO, IAgencyDAO acencyDAO, OTPSecurity otpSecurity, IDataService dataService, FacebookUtil facebookUtil, IDistributedCache cache)
        {
            appSettings = options.Value;
            // _eventDAO = eventDAO;
            _agencyDao = acencyDAO;
            _accountDAO = accountDAO;
            _otp = otpSecurity;
            _dataService = dataService;
            _facebookUtil = facebookUtil;
            _accountSession = accountSession;
            _cache = cache;
        }

        public int Login(LoginAccount loginAccount, out AccountInfo accountInfo)
        {
            //var watch = Stopwatch.StartNew();

            int response = (int)ErrorCodes.LOGIN_ERROR;
            accountInfo = null;
            try
            {
                //if (loginAccount == null)
                //{
                //    response = (int)ErrorCodes.ACCOUNT_NAME_INVALID;
                //    accountInfo = null;
                //}
                NLogManager.Info("LOGIN service 200: ");

                AccountDb account = _accountDAO.Login(loginAccount.UserName, Security.GetPasswordEncryp(loginAccount.Password, loginAccount.UserName), loginAccount.IpAddress, loginAccount.Uiid, loginAccount.MerchantId, loginAccount.PlatformId, out response);
                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("LOGIN_LoginServer 1: {0}, {1}", loginAccount.UserName, elapsedMs));
                //if (response < 0)
                //{
                //    loginAccount.UserName = loginAccount.UserName.Replace("+", "");
                //    if (loginAccount.UserName.All(char.IsDigit))
                //    {
                //        if (loginAccount.UserName.StartsWith("84"))
                //        {
                //            loginAccount.UserName = "0" + loginAccount.UserName.Substring(2, loginAccount.UserName.Length - 2);
                //        }

                //        if (!loginAccount.UserName.StartsWith("0"))
                //        {
                //            loginAccount.UserName = "0" + loginAccount.UserName;
                //        }
                //        long accountID = -1;
                //        string userName = _accountDAO.GetInfoByMobile(loginAccount.UserName, out accountID);
                //        if (accountID > 0)
                //        {
                //            account = _accountDAO.Login(userName, Security.GetPasswordEncryp(loginAccount.Password, userName), loginAccount.IpAddress, loginAccount.Uiid, loginAccount.MerchantId, loginAccount.PlatformId, out response);
                //        }
                //    }
                //    NLogManager.Info(string.Format("LOGIN service 10: {0}", elapsedMs));

                //}
                // NLogManager.Info(string.Format("LOGIN service 200: {0}", elapsedMs));

                //watch.Restart();
                if (account == null || response < 0)
                {
                    // Not Exist Account
                    if (response == -50)
                    {
                        response = (int)ErrorCodes.ACCOUNT_NOT_EXIST;
                        return response;
                    }
                    if (response == -53)
                    {
                        response = (int)ErrorCodes.LOGIN_ERROR;
                        return response;
                    }
                    // Account Blocked
                    if (response == -54)
                    {
                        response = (int)ErrorCodes.ACCOUNT_BLOCKED;
                        return response;
                    }
                    // Account Disable
                    if (response == -55)
                    {
                        response = (int)ErrorCodes.ACCOUNT_DISABLED;
                        return response;
                    }
                    return response;
                }

                CacheCounter.AccountActionDelete(loginAccount.UserName, appSettings.LoginActionName);
                //elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("LOGIN service 3: {0}", elapsedMs));
                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(account.AccountID);
                // set trạng thái active mobile 
                if (securityInfo != null)
                    account.IsMobileActived = securityInfo.IsMobileActived;

                if (securityInfo.IsLoginOTP)
                {
                    string serviceData = JsonConvert.SerializeObject(account);
                    OtpData otpData = new OtpData(account.UserName, account.AccountID, DateTime.Now.Ticks, (int)OtpServiceCode.OTP_SERVICE_LOGIN, loginAccount.PlatformId, serviceData);
                    string token = OTPSecurity.CreateOTPToken(otpData);
                    accountInfo = new AccountInfo();
                    accountInfo.IsOtp = true;
                    accountInfo.OtpToken = token;
                    response = (int)ErrorCodes.NEED_OTP_CODE;
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info(string.Format("LOGIN service 4: {0}", elapsedMs));
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info(string.Format("LOGIN_LoginServer 2: {0}, {1}", loginAccount.UserName, elapsedMs));
                }
                else
                {
                    //watch.Restart();
                    account.IsAgency = _agencyDao.CheckIsAgency((int)account.AccountID);
                    accountInfo = new AccountInfo(account);
                    accountInfo.AccessToken = GenerateToken(account);
                    accountInfo.AccessTokenFishing = GenerateTokenFishing(account);
                    //accountInfo.IsEvent = IsShowEvent(account.AccountID);
                    //4: android, 3: ios, 2: windows - pc, 1: web
                    string platform = "Android";
                    if (account.PlatformID == 3)
                        platform = "Ios";
                    if (account.PlatformID == 2)
                        platform = "Window";
                    if (account.PlatformID == 1)
                        platform = "Web";
                    string message = String.Format("Bạn vừa đăng nhập vào hệ thống trên {0} lúc {1}. Chúc bạn chơi game vui vẻ!", platform, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                    _otp.PushMessageToTelegramAsync(account.AccountID, account.Mobile, message);
                    //if (accountInfo.AccountID == 91373)
                    //{
                    //    _otp.PushMessageToTelegram(65, "0762222888", "Đối tượng MrBin2019 vừa đăng nhập, vào cân cửa thôi nào!");
                    //}
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info(string.Format("LOGIN service 5: {0}", elapsedMs));
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info(string.Format("LOGIN_LoginServer 3: {0}, {1}", loginAccount.UserName, elapsedMs));
                    return (int)ErrorCodes.SUCCESS;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return response;
        }

        // for test
        public async Task<(int, AccountInfo)> LoginAsync(LoginAccount loginAccount)
        {
            //var watch = Stopwatch.StartNew();

            int response = (int)ErrorCodes.LOGIN_ERROR;
            AccountInfo accountInfo = null;
            try
            {
                //if (loginAccount == null)
                //{
                //    response = (int)ErrorCodes.ACCOUNT_NAME_INVALID;
                //    accountInfo = null;
                //}
                AccountDb account = _accountDAO.Login(loginAccount.UserName, Security.GetPasswordEncryp(loginAccount.Password, loginAccount.UserName), loginAccount.IpAddress, loginAccount.Uiid, loginAccount.MerchantId, loginAccount.PlatformId, out response);
                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("LOGIN_LoginServer 1: {0}, {1}", loginAccount.UserName, elapsedMs));
                //if (response < 0)
                //{
                //    loginAccount.UserName = loginAccount.UserName.Replace("+", "");
                //    if (loginAccount.UserName.All(char.IsDigit))
                //    {
                //        if (loginAccount.UserName.StartsWith("84"))
                //        {
                //            loginAccount.UserName = "0" + loginAccount.UserName.Substring(2, loginAccount.UserName.Length - 2);
                //        }

                //        if (!loginAccount.UserName.StartsWith("0"))
                //        {
                //            loginAccount.UserName = "0" + loginAccount.UserName;
                //        }
                //        long accountID = -1;
                //        string userName = _accountDAO.GetInfoByMobile(loginAccount.UserName, out accountID);
                //        if (accountID > 0)
                //        {
                //            account = _accountDAO.Login(userName, Security.GetPasswordEncryp(loginAccount.Password, userName), loginAccount.IpAddress, loginAccount.Uiid, loginAccount.MerchantId, loginAccount.PlatformId, out response);
                //        }
                //    }
                //    NLogManager.Info(string.Format("LOGIN service 10: {0}", elapsedMs));

                //}
                // NLogManager.Info(string.Format("LOGIN service 200: {0}", elapsedMs));

                //watch.Restart();
                if (account == null || response < 0)
                {
                    // Not Exist Account
                    if (response == -50)
                    {
                        response = (int)ErrorCodes.ACCOUNT_NOT_EXIST;
                        return (response, accountInfo);
                    }
                    if (response == -53)
                    {
                        response = (int)ErrorCodes.PASSWORD_INVALID;
                        return (response, accountInfo);
                    }
                    // Account Blocked
                    if (response == -54)
                    {
                        response = (int)ErrorCodes.ACCOUNT_BLOCKED;
                        return (response, accountInfo);
                    }
                    // Account Disable
                    if (response == -55)
                    {
                        response = (int)ErrorCodes.ACCOUNT_DISABLED;
                        return (response, accountInfo);
                    }
                    return (response, accountInfo);
                }

                CacheCounter.AccountActionDelete(loginAccount.UserName, appSettings.LoginActionName);
                //elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("LOGIN service 3: {0}", elapsedMs));
                SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(account.AccountID);
                // set trạng thái active mobile 
                if (securityInfo != null)
                    account.IsMobileActived = securityInfo.IsMobileActived;
                if (securityInfo.IsLoginOTP)
                {
                    string serviceData = JsonConvert.SerializeObject(account);
                    OtpData otpData = new OtpData(account.UserName, account.AccountID, DateTime.Now.Ticks, (int)OtpServiceCode.OTP_SERVICE_LOGIN, loginAccount.PlatformId, serviceData);
                    string token = OTPSecurity.CreateOTPToken(otpData);
                    accountInfo = new AccountInfo();
                    accountInfo.IsOtp = true;
                    accountInfo.OtpToken = token;
                    response = (int)ErrorCodes.NEED_OTP_CODE;
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info(string.Format("LOGIN service 4: {0}", elapsedMs));
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info(string.Format("LOGIN_LoginServer 2: {0}, {1}", loginAccount.UserName, elapsedMs));
                }
                else
                {
                    //watch.Restart();
                    account.IsAgency = _agencyDao.CheckIsAgency((int)account.AccountID);
                    accountInfo = new AccountInfo(account);
                    accountInfo.AccessToken = GenerateToken(account);
                    accountInfo.AccessTokenFishing = GenerateTokenFishing(account);
                    //accountInfo.IsEvent = IsShowEvent(account.AccountID);
                    string platform = "Android";
                    if (account.PlatformID == 3)
                        platform = "Ios";
                    if (account.PlatformID == 2)
                        platform = "Window";
                    if (account.PlatformID == 1)
                        platform = "Web";
                    string message = String.Format("Bạn vừa đăng nhập vào hệ thống trên {0} lúc {1}. Chúc bạn chơi game vui vẻ!", platform, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                    await _otp.PushMessageToTelegramAsync(account.AccountID, account.Mobile, message);
                    //if (accountInfo.AccountID == 91373)
                    //{
                    //    _otp.PushMessageToTelegram(65, "0762222888", "Đối tượng MrBin2019 vừa đăng nhập, vào cân cửa thôi nào!");
                    //}
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info(string.Format("LOGIN service 5: {0}", elapsedMs));
                    //watch.Stop();
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info(string.Format("LOGIN_LoginServer 3: {0}, {1}", loginAccount.UserName, elapsedMs));
                    return ((int)ErrorCodes.SUCCESS, accountInfo);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return (response, accountInfo);
        }

        public int GetInfo(long AccountID, string UserName, out AccountInfo accountInfo)
        {
            accountInfo = null;
            try
            {
                AccountDb account = _accountDAO.GetAccount(AccountID, UserName);
                CacheCounter.AccountActionDelete(_accountSession.AccountName, appSettings.LoginActionName);
                if (account != null)
                {
                    SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(account.AccountID);
                    if (securityInfo != null)
                        account.IsMobileActived = securityInfo.IsMobileActived;
                    account.IsAgency = _agencyDao.CheckIsAgency((int)account.AccountID);
                    accountInfo = new AccountInfo(account);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                accountInfo = null;
                NLogManager.Exception(ex);
            }
            accountInfo = null;
            return -1;
        }

        public int ReportLogin(LoginAccount loginAccount, out AccountInfo accountInfo)
        {
            int response = (int)ErrorCodes.LOGIN_ERROR;
            accountInfo = null;
            try
            {
                if (loginAccount == null)
                {
                    response = (int)ErrorCodes.ACCOUNT_NAME_INVALID;
                    accountInfo = null;
                }
                AccountDb account = _accountDAO.Login(loginAccount.UserName, Security.GetPasswordEncryp(loginAccount.Password, loginAccount.UserName), loginAccount.IpAddress, loginAccount.Uiid, loginAccount.MerchantId, loginAccount.PlatformId, out response);
                if (account == null || response < 0)
                {
                    return (int)ErrorCodes.LOGIN_ERROR;
                }

                CacheCounter.AccountActionDelete(loginAccount.UserName, appSettings.LoginActionName);
                // Bỏ qua check login 2 bước qua OTP
                //SecurityInfo securityInfo = _accountDAO.GetSecurityInfo(account.AccountID);
                //if (securityInfo.IsLoginOTP)
                //{
                //    string serviceData = JsonConvert.SerializeObject(account);
                //    OtpData otpData = new OtpData(account.UserName, account.AccountID, DateTime.Now.Ticks, (int)OtpServiceCode.OTP_SERVICE_LOGIN, loginAccount.PlatformId, serviceData);
                //    string token = OTPSecurity.CreateOTPToken(otpData);
                //    accountInfo = new AccountInfo();
                //    accountInfo.IsOtp = true;
                //    accountInfo.OtpToken = token;
                //    response = (int)ErrorCodes.NEED_OTP_CODE;
                //}
                //else
                //{
                accountInfo = new AccountInfo(account);
                accountInfo.AccessToken = GenerateToken(account);
                accountInfo.AccessTokenFishing = GenerateTokenFishing(account);
                return (int)ErrorCodes.SUCCESS;
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return response;
        }

        public async Task<AccountInfo> LoginFBAsync(LoginAccount fbData)
        {
            AccountInfo accountInfo = new AccountInfo();
            if (appSettings.IsMaintain)
            {
                accountInfo.ErrorCode = (int)ErrorCodes.SERVER_MAINTAIN;
                return accountInfo;
            }

            string accesstoken = string.Empty;
            string fbId = string.Empty;
            int platformId = 0;
            int merchantId = 0;
            string uiid = string.Empty;
            uiid = fbData.Uiid;
            //int productID = 1;
            //int friends = 0;

            try
            {
                accesstoken = fbData.FacebookToken;
                fbId = fbData.FacebookId;
                platformId = fbData.PlatformId;
                merchantId = fbData.MerchantId;
                uiid = fbData.Uiid;
                //friends = fbData.friends;

                if (platformId > 4 || platformId < 0)
                {
                    accountInfo.ErrorCode = (int)ErrorCodes.PLATFORM_INVALID;
                    return accountInfo;
                }

                // Validate arguments
                if (string.IsNullOrEmpty(accesstoken))
                {
                    accountInfo.ErrorCode = (int)ErrorCodes.TOKEN_ERROR;
                    return accountInfo;
                }

                if (string.IsNullOrEmpty(fbId))
                {
                    accountInfo.ErrorCode = (int)ErrorCodes.FACEBOOK_INVALID;
                    return accountInfo;
                }

                //NLogManager.Info(string.Format("facebookId = {0}, accessToken = {1}, platformId = {2}", fbId, accesstoken, platformId));
                //Validate facebook access token
                //string graphApi = string.Format("https://graph.facebook.com/{0}?fields=email&access_token={1}", fbId, accesstoken);
                //string graphApi = string.Format("https://graph.facebook.com/me?access_token={0}", accesstoken);
                string url = string.Format(appSettings.GraphApiFbMe, accesstoken);
                //var data = await _dataService.GetAsync(url, true);


                //if (data == null)
                //{
                //    NLogManager.Error(string.Format("Facebook_error: facebookId = {0}, accessToken = {1}", fbId, accesstoken));
                //    accountInfo.ErrorCode = (int)ErrorCodes.FACEBOOK_INVALID;
                //    return accountInfo;
                //}


                //var fbInfor = _facebookUtil.GetFBAccount(data.ToString());

                NLogManager.Info(string.Format("facebookName={0}, facebookId={1}, accessToken={2}, platformId={3}", "fbInfor.Name", fbId, accesstoken, platformId));

                //var watch = Stopwatch.StartNew();

                string partnerIDS = await _facebookUtil.GetIDsForBusiness(fbId, accesstoken, appSettings.GraphApiFbBusiness);

                //var elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("GetIDsForBusiness: {0}", elapsedMs));
                //if (fbInfor.Id != fbId)
                //{
                //    NLogManager.Error("Không tìm thấy thông tin tài khoản Facebook: id = " + fbId + "; name = " /*+ fbInfor.Name*/);
                //    accountInfo.ErrorCode = (int)ErrorCodes.FACEBOOK_INVALID;
                //    return accountInfo;
                //}



                string passWord = Security.GetPasswordEncryp("6b4b2bf6d0d1cf8aecdcc74a01cdac31", "FB." + fbId);

                ChannelingAccount accountChanneling = _accountDAO.SelectAccountByFacebook(3, fbId, "", partnerIDS, merchantId, platformId, 0, passWord, uiid, fbData.IpAddress);

                //elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("SelectAccountByFacebook: {0}", elapsedMs));

                if (accountChanneling == null || accountChanneling.ChannelingID <= 0)
                {
                    NLogManager.Info("accountChanneling_Error: " + fbId);
                    accountInfo.ErrorCode = (int)ErrorCodes.LOGIN_ERROR;
                    return accountInfo;
                    //string partnerAccountIds = "";
                    //List<IDs_Business> lstPartnerAccountIds = FacebookUtil.GetIDsForBusiness(accesstoken);
                    //if(lstPartnerAccountIds != null && lstPartnerAccountIds.Count > 0)
                    //    foreach(IDs_Business partnerId in lstPartnerAccountIds)
                    //        partnerAccountIds += ";" + partnerId.id;

                    //if(partnerAccountIds.IndexOf(";") == 0)
                    //    partnerAccountIds = partnerAccountIds.Substring(1);

                    //accountChanneling = _accountDAO.SelectAccountByFacebook(3, fbInfor.Id, fbInfor.Email, partnerAccountIds, Convert.ToInt32(serviceId), Convert.ToInt32(platformId), productID);
                }
                AccountDb account = _accountDAO.GetAccount(accountChanneling.AccountID, accountChanneling.AccountName);
                //elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info("GetAccount Duration: " + elapsedMs);
                if (account == null || account.AccountID < 1)
                {
                    accountInfo.ErrorCode = (int)ErrorCodes.ACCOUNT_NOT_EXIST;
                    return accountInfo;
                }

                //if (res >= 0)
                //    account.UserFullname = fbName;
                account.PlatformID = platformId;
                account.ClientIP = fbData.IpAddress;
                SecurityInfo securityInfo = _otp.GetSucurityInfoWithMask((int)account.AccountID, false);
                if (securityInfo != null)
                {
                    account.IsOtp = securityInfo.IsLoginOTP ? 1 : 0;
                    account.IsMobileActived = securityInfo.IsMobileActived;
                }
                accountInfo = new AccountInfo(account);
                if (account.IsOtp == 0)
                {
                    //SetAuthCookie(account);
                    //NLogManager.Info("Login_Facebook_OK: " + fbId + ", FacebookName: " + fbInfor.Name);
                    account.IsAgency = _agencyDao.CheckIsAgency((int)account.AccountID);
                    accountInfo.AccessToken = GenerateToken(account);
                    accountInfo.AccessTokenFishing = GenerateTokenFishing(account);
                    accountInfo.ErrorCode = (int)ErrorCodes.SUCCESS;
                    //accountInfo.IsEvent = IsShowEvent(account.AccountID);

                    string platform = "Android";
                    if (account.PlatformID == 3)
                        platform = "Ios";
                    if (account.PlatformID == 2)
                        platform = "Window";
                    if (account.PlatformID == 1)
                        platform = "Web";
                    string message = String.Format("Bạn vừa đăng nhập vào hệ thống trên {0} lúc {1}. Chúc bạn chơi game vui vẻ!", platform, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                    _otp.PushMessageToTelegramAsync(accountInfo.AccountID, accountInfo.Mobile, message);
                    //if (accountInfo.AccountID == 91373)
                    //{
                    //    _otp.PushMessageToTelegram(65, "0762222888", "Đối tượng MrBin2019 vừa đăng nhập, vào cân cửa thôi nào!");
                    //}
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info("account.IsOtp  Duration: " + elapsedMs);
                    //watch.Stop();
                    return accountInfo;
                }
                else
                {
                    string serviceData = JsonConvert.SerializeObject(account);
                    OtpData otpData = new OtpData(account.UserName, account.AccountID, DateTime.Now.Ticks, (int)OtpServiceCode.OTP_SERVICE_LOGIN, platformId, serviceData);
                    string token = OTPSecurity.CreateOTPToken(otpData);
                    accountInfo = new AccountInfo();
                    accountInfo.IsOtp = true;
                    accountInfo.OtpToken = token;
                    accountInfo.ErrorCode = (int)ErrorCodes.NEED_OTP_CODE;

                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info("Okay  Duration: " + elapsedMs);
                    //watch.Stop();
                    return accountInfo;
                }
                //return HttpUtils.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(new AccountDataReturn(account)), "Account");
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                accountInfo.ErrorCode = (int)ErrorCodes.SERVER_ERROR;
                return accountInfo;
            }
        }


        //public int LoginFBAsync(LoginAccount fbData, out AccountInfo accountInfo)
        //{
        //    accountInfo = null;
        //    if (appSettings.IsMaintain)
        //        return (int)ErrorCodes.SERVER_MAINTAIN;

        //    string accesstoken = string.Empty;
        //    string fbId = string.Empty;
        //    int platformId = 0;
        //    int merchantId = 0;
        //    string uiid = string.Empty;
        //    //int productID = 1;
        //    //int friends = 0;

        //    try
        //    {
        //        accesstoken = fbData.FacebookToken;
        //        fbId = fbData.FacebookId;
        //        platformId = fbData.PlatformId;
        //        merchantId = fbData.MerchantId;
        //        uiid = fbData.Uiid;
        //        //friends = fbData.friends;

        //        if (platformId > 4 || platformId < 0)
        //        {
        //            return (int)ErrorCodes.PLATFORM_INVALID;
        //        }

        //        // Validate arguments
        //        if (string.IsNullOrEmpty(accesstoken))
        //        {
        //            return (int)ErrorCodes.TOKEN_ERROR;
        //        }

        //        if (string.IsNullOrEmpty(fbId))
        //        {
        //            return (int)ErrorCodes.FACEBOOK_INVALID;
        //        }

        //        //NLogManager.Info(string.Format("facebookId = {0}, accessToken = {1}, platformId = {2}", fbId, accesstoken, platformId));
        //        //Validate facebook access token
        //        //string graphApi = string.Format("https://graph.facebook.com/{0}?fields=email&access_token={1}", fbId, accesstoken);
        //        //string graphApi = string.Format("https://graph.facebook.com/me?access_token={0}", accesstoken);
        //        string url = string.Format(appSettings.GraphApiFbMe, accesstoken);
        //        var data = HttpUtil.GetAsync(url);
        //        if (data == null)
        //        {
        //            NLogManager.Error(string.Format("Facebook_error: facebookId = {0}, accessToken = {1}", fbId, accesstoken));
        //            return (int)ErrorCodes.FACEBOOK_INVALID;
        //        }

        //        var fbInfor = _facebookUtil.GetFBAccount(data.ToString());
        //        NLogManager.Info(string.Format("facebookName={0}, facebookId={1}, accessToken={2}, platformId={3}", fbInfor.Name, fbId, accesstoken, platformId));

        //        var partnerIDS = _facebookUtil.GetIDsForBusiness(accesstoken, appSettings.GraphApiFbBusiness);
        //        if (fbInfor.Id != fbId)
        //        {
        //            NLogManager.Error("Không tìm thấy thông tin tài khoản Facebook: id = " + fbId + "; name = " + fbInfor.Name);
        //            return (int)ErrorCodes.FACEBOOK_INVALID;
        //        }

        //        string listPartnerIDs = "";
        //        foreach (var item in partnerIDS)
        //            listPartnerIDs += ";" + item.id;

        //        if (listPartnerIDs.IndexOf(";") == 0)
        //            listPartnerIDs = listPartnerIDs.Substring(1);

        //        ChannelingAccount accountChanneling = _accountDAO.SelectAccountByFacebook(3, fbInfor.Id, fbInfor.Name, listPartnerIDs, merchantId, platformId, 0, uiid);
        //        if (accountChanneling == null || accountChanneling.ChannelingID <= 0)
        //        {
        //            NLogManager.Info("accountChanneling_Error: " + fbId);
        //            return (int)ErrorCodes.LOGIN_ERROR;
        //            //string partnerAccountIds = "";
        //            //List<IDs_Business> lstPartnerAccountIds = FacebookUtil.GetIDsForBusiness(accesstoken);
        //            //if(lstPartnerAccountIds != null && lstPartnerAccountIds.Count > 0)
        //            //    foreach(IDs_Business partnerId in lstPartnerAccountIds)
        //            //        partnerAccountIds += ";" + partnerId.id;

        //            //if(partnerAccountIds.IndexOf(";") == 0)
        //            //    partnerAccountIds = partnerAccountIds.Substring(1);

        //            //accountChanneling = _accountDAO.SelectAccountByFacebook(3, fbInfor.Id, fbInfor.Email, partnerAccountIds, Convert.ToInt32(serviceId), Convert.ToInt32(platformId), productID);
        //        }

        //        AccountDb account = _accountDAO.GetAccount(accountChanneling.AccountID, accountChanneling.AccountName);
        //        //int res = _accountDAO.UpdateUserFullName(accountChanneling.Mobile, accountChanneling.AccountID, fbName);
        //        if (account == null || account.AccountID < 1)
        //            return (int)ErrorCodes.ACCOUNT_NOT_EXIST;

        //        //if (res >= 0)
        //        //    account.UserFullname = fbName;
        //        account.PlatformID = platformId;
        //        account.ClientIP = fbData.IpAddress;
        //        accountInfo = new AccountInfo(account);

        //        SecurityInfo securityInfo = _otp.GetSucurityInfoWithMask((int)account.AccountID, false);
        //        if (securityInfo != null)
        //        {
        //            account.IsOtp = securityInfo.IsLoginOTP ? 1 : 0;
        //        }

        //        //NLogManager.LogMessageAuthen(string.Format("Mobile: {0} Login FB - PlatformID: {1} - IP Addess: {2}", account.UserName, account.PlatformID, account.ClientIP));
        //        if (account.IsOtp == 0)
        //        {
        //            //SetAuthCookie(account);
        //            NLogManager.Info("Login_Facebook_OK: " + fbId + ", FacebookName: " + fbInfor.Name);
        //            accountInfo.AccessToken = GenerateToken(account);
        //            return (int)ErrorCodes.SUCCESS;
        //        }
        //        else
        //        {
        //            string serviceData = JsonConvert.SerializeObject(account);
        //            OtpData otpData = new OtpData(account.UserName, account.AccountID, DateTime.Now.Ticks, (int)OtpServiceCode.OTP_SERVICE_LOGIN, platformId, serviceData);
        //            string token = OTPSecurity.CreateOTPToken(otpData);
        //            accountInfo = new AccountInfo();
        //            accountInfo.IsOtp = true;
        //            accountInfo.OtpToken = token;
        //            return (int)ErrorCodes.NEED_OTP_CODE;
        //        }
        //        //return HttpUtils.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(new AccountDataReturn(account)), "Account");
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Exception(ex);
        //        return (int)ErrorCodes.SERVER_ERROR;
        //    }
        //}

        public int LoginVipCode(LoginAccount loginAccount, out AccountInfo accountInfo)
        {
            throw new NotImplementedException();
        }

        public bool CheckUserNameIsMobileOther(string mobile)
        {
            string mobileFind = Utils.FormatMobile(mobile);
            long accountID = -1;
            string userName = _accountDAO.GetInfoByMobile(mobileFind, out accountID);
            if (accountID > 0)
                return true;

            return false;
        }
        public int Register(LoginAccount loginAccount, out AccountInfo accountInfo)
        {
            string username = loginAccount.UserName.Trim().ToLower();
            string password = loginAccount.Password;
            string nickName = "";
            string captchaVerify = loginAccount.CaptchaToken;
            string captchaText = loginAccount.CaptchaText;
            int platformId = loginAccount.PlatformId;
            int merchantId = loginAccount.MerchantId;
            string ipAddress = loginAccount.IpAddress;
            string uuid = loginAccount.Uiid;
            string campaignSource = loginAccount.CampaignSource;
            int locationID = loginAccount.LocationID;
            int response = (int)ErrorCodes.REGISTER_ACCOUNT_ERROR;
            accountInfo = null;
            if (locationID <= 0)
                locationID = 16;

            try
            {
                string passwordEncryp = Security.GetPasswordEncryp(password, username);
                AccountDb accountDb = _accountDAO.Register(username, passwordEncryp, nickName, ipAddress, merchantId, platformId, campaignSource, uuid, out response, locationID);
                if (accountDb != null)
                {
                    accountInfo = new AccountInfo(accountDb);
                    accountInfo.AccessToken = GenerateToken(accountDb);
                    accountInfo.AccessTokenFishing = GenerateTokenFishing(accountDb);
                   // accountInfo.IsEvent = IsShowEvent(accountDb.AccountID);
                    return response;
                }
                else
                {
                    string content = "";
                    if (response == (int)ErrorCodes.ACCOUNT_EXIST)
                        content = "Tài khoản đã tồn tại";
                    else if (response == (int)ErrorCodes.NICKNAME_EXIST)
                        content = "Nick name đã tồn tại";
                    else if (response == (int)ErrorCodes.IP_BLOCK)
                        content = "IP bị khóa";

                    NLogManager.Error(content + ": UserName=" + username + ",NickName=" + nickName + ",IP=" + ipAddress);
                    return response;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return response;
        }

        public int Validate(string token, out AccountInfo accountInfo)
        {
            accountInfo = null;
            int response = (int)ErrorCodes.VALIDATE_ERROR;
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var expDate = jwtToken.ValidTo;
                    if (expDate > DateTime.Now.AddHours(appSettings.TokenExpire))
                        response = (int)ErrorCodes.TOKEN_ERROR;

                    object accountIdObj;
                    object userNameObj;
                    object nickNameObj;

                    jwtToken.Payload.TryGetValue(appSettings.ClaimTypeAccountId, out accountIdObj);
                    jwtToken.Payload.TryGetValue(appSettings.ClaimTypeUserName, out userNameObj);
                    jwtToken.Payload.TryGetValue(appSettings.ClaimTypeNickName, out nickNameObj);

                    if (accountIdObj != null && userNameObj != null && nickNameObj != null)
                    {
                        long accountID = Int32.Parse(accountIdObj.ToString());
                        string userName = userNameObj.ToString();
                        string nickName = nickNameObj.ToString();

                        accountInfo = new AccountInfo();
                        accountInfo.AccountID = accountID;
                        accountInfo.UserName = userName;
                        accountInfo.NickName = nickName;

                        response = (int)ErrorCodes.SUCCESS;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return response;
        }

        public string GenerateToken(AccountDb accountDb)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(appSettings.JwtKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(appSettings.ClaimTypeAccountId, accountDb.AccountID.ToString()),
                        new Claim(appSettings.ClaimTypeUserName, accountDb.UserName),
                        new Claim(appSettings.ClaimTypeNickName, accountDb.UserFullname),
                        new Claim(appSettings.ClaimTypePlatformId, accountDb.PlatformID.ToString()),
                        new Claim(appSettings.ClaimTypeMerchantId, accountDb.MerchantID.ToString()),
                        new Claim(appSettings.ClaimTypeAgency, accountDb.IsAgency.ToString()),
                        new Claim(appSettings.ClaimTypeOTP, accountDb.IsMobileActived.ToString()),
                        new Claim(appSettings.ClaimLocationId, accountDb.LocationID.ToString()),
                        new Claim(appSettings.ClaimPreFix, (string.IsNullOrEmpty(accountDb.PreFix)?"GB":accountDb.PreFix.ToString()))
                        //new Claim(appSettings.ClaimRefCode, accountDb.RefCode.ToString())
                    }),
                    Expires = DateTime.Now.AddHours(appSettings.TokenExpire),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
        }

        public string GenerateTokenFishing(AccountDb accountDb)
        {
            if (accountDb.IsAgency > 0)
                return "";
            string token = _otp.CreateOTPTokenFish(accountDb.UserFullname, accountDb.AccountID, 101, accountDb.PlatformID, JsonConvert.SerializeObject(accountDb));
            return token;
        }

        public async Task<AccountInfo> LoginAppleIdAsync(LoginAccount appleData)
        {
            AccountInfo accountInfo = new AccountInfo();
            if (appSettings.IsMaintain)
            {
                accountInfo.ErrorCode = (int)ErrorCodes.SERVER_MAINTAIN;
                return accountInfo;
            }

            string appleToken = string.Empty;
            //string appleId = string.Empty;
            int platformId = 0;
            int merchantId = 0;
            string uiid = string.Empty;
            //string email = string.Empty;
            //int productID = 1;
            //int friends = 0;

            try
            {
                appleToken = appleData.AppleToken;
                platformId = appleData.PlatformId;
                merchantId = appleData.MerchantId;
                uiid = appleData.Uiid;

                if (platformId > 4 || platformId < 0)
                {
                    accountInfo.ErrorCode = (int)ErrorCodes.PLATFORM_INVALID;
                    return accountInfo;
                }

                // Validate arguments
                if (string.IsNullOrEmpty(appleToken))
                {
                    accountInfo.ErrorCode = (int)ErrorCodes.TOKEN_ERROR;
                    return accountInfo;
                }

                NLogManager.Info(string.Format("appleToken={0}", appleToken));

                //var watch = Stopwatch.StartNew();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("GetIDsForBusiness: {0}", elapsedMs));

                string keysJson = await new HttpClient().GetStringAsync("https://appleid.apple.com/auth/keys");
                JsonWebKeySet jsonWebKeySet = JsonWebKeySet.Create(keysJson);

                var parameters = new TokenValidationParameters()
                {
                    ValidAudience = appSettings.BundleID,
                    ValidIssuer = "https://appleid.apple.com",
                    IssuerSigningKeys = jsonWebKeySet.Keys
                };

                SecurityToken securityToken = null;
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(appleToken, parameters, out securityToken);
                if (securityToken != null)
                {
                    string bundleId = ((JwtSecurityToken)securityToken).Payload["aud"].ToString();
                    string appleId = ((JwtSecurityToken)securityToken).Payload["sub"].ToString();
                    string email = ((JwtSecurityToken)securityToken).Payload["email"].ToString();
                    string passWord = Security.GetPasswordEncryp("6b4b2bf6d0d1cf8aecdcc74a01cdac31", "Apple." + appleId);
                    NLogManager.Info(string.Format("appleId={0}, email={1}", appleId, email));

                    ChannelingAccount accountChanneling = _accountDAO.SelectAccountByApple(appleId, bundleId, email, merchantId, platformId, uiid, appleData.IpAddress);
                    if (accountChanneling == null || accountChanneling.AccountID <= 0)
                    {
                        NLogManager.Error("Login apple error");
                        accountInfo.ErrorCode = (int)ErrorCodes.LOGIN_ERROR;
                        return accountInfo;
                    }
                    AccountDb account = _accountDAO.GetAccount(accountChanneling.AccountID, accountChanneling.AccountName);
                    //elapsedMs = watch.ElapsedMilliseconds;
                    //NLogManager.Info("GetAccount Duration: " + elapsedMs);
                    if (account == null || account.AccountID < 1)
                    {
                        accountInfo.ErrorCode = (int)ErrorCodes.ACCOUNT_NOT_EXIST;
                        return accountInfo;
                    }

                    //if (res >= 0)
                    //    account.UserFullname = fbName;
                    account.PlatformID = platformId;
                    account.ClientIP = appleData.IpAddress;
                    SecurityInfo securityInfo = _otp.GetSucurityInfoWithMask((int)account.AccountID, false);
                    if (securityInfo != null)
                    {
                        account.IsOtp = securityInfo.IsLoginOTP ? 1 : 0;
                        account.IsMobileActived = securityInfo.IsMobileActived;
                    }
                    accountInfo = new AccountInfo(account);
                    if (account.IsOtp == 0)
                    {
                        //SetAuthCookie(account);
                        //NLogManager.Info("Login_Facebook_OK: " + fbId + ", FacebookName: " + fbInfor.Name);
                        account.IsAgency = _agencyDao.CheckIsAgency((int)account.AccountID);
                        accountInfo.AccessToken = GenerateToken(account);
                        accountInfo.AccessTokenFishing = GenerateTokenFishing(account);
                        accountInfo.ErrorCode = (int)ErrorCodes.SUCCESS;
                        //accountInfo.IsEvent = IsShowEvent(account.AccountID);

                        string message = String.Format("Bạn vừa đăng nhập vào hệ thống trên ios lúc {0}. Chúc bạn chơi game vui vẻ!", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                        _otp.PushMessageToTelegramAsync(accountInfo.AccountID, accountInfo.Mobile, message);
                        //if (accountInfo.AccountID == 91373)
                        //{
                        //    _otp.PushMessageToTelegram(65, "0762222888", "Đối tượng MrBin2019 vừa đăng nhập, vào cân cửa thôi nào!");
                        //}
                        //elapsedMs = watch.ElapsedMilliseconds;
                        //NLogManager.Info("account.IsOtp  Duration: " + elapsedMs);
                        //watch.Stop();
                        return accountInfo;
                    }
                    else
                    {
                        string serviceData = JsonConvert.SerializeObject(account);
                        OtpData otpData = new OtpData(account.UserName, account.AccountID, DateTime.Now.Ticks, (int)OtpServiceCode.OTP_SERVICE_LOGIN, platformId, serviceData);
                        string token = OTPSecurity.CreateOTPToken(otpData);
                        accountInfo = new AccountInfo();
                        accountInfo.IsOtp = true;
                        accountInfo.OtpToken = token;
                        accountInfo.ErrorCode = (int)ErrorCodes.NEED_OTP_CODE;

                        //elapsedMs = watch.ElapsedMilliseconds;
                        //NLogManager.Info("Okay  Duration: " + elapsedMs);
                        //watch.Stop();
                        return accountInfo;
                    }
                }

                NLogManager.Error("Login apple error");
                accountInfo.ErrorCode = (int)ErrorCodes.APPLE_INVALID;
                return accountInfo;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                accountInfo.ErrorCode = (int)ErrorCodes.SERVER_ERROR;
                return accountInfo;
            }
        }

        private bool IsShowEvent(long accountId)
        {
            bool isEvent = false;
            try
            {
                string keyEvent = accountId.ToString() + "_event";
                string eventTime = _cache.GetString(keyEvent);
                if (string.IsNullOrEmpty(eventTime))
                {
                    isEvent = true;
                    long currTime = DateTime.Now.Ticks;
                    var cacheEntryOptions = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(24));
                    _cache.SetString(keyEvent, currTime.ToString(), cacheEntryOptions);
                }
                else
                {
                    var dateCache = new DateTime(Int64.Parse(eventTime));
                    var currDate = DateTime.Now;
                    if(dateCache.Day == currDate.Day && dateCache.Year == currDate.Year && dateCache.Month == currDate.Month)
                    {
                        isEvent = false;
                    }
                    else
                    {
                        isEvent = true;
                        var cacheEntryOptions = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(24));
                        _cache.SetString(keyEvent, currDate.Ticks.ToString(), cacheEntryOptions);
                    }
                }
            }
            catch(Exception ex)
            {
                NLogManager.Info(ex.ToString());
            }
            return isEvent;
        }
    }
}
