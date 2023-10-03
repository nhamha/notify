using ServerCore.DataAccess.DTO;
using ServerCore.PortalAPI.Models;
using ServerCore.Utilities.Models;
using System;
using System.Collections.Generic;

namespace ServerCore.DataAccess.DAO
{
    //Cac SP tu PaygateDB: dang nhap va lay thong tin tai khoan
    public interface IAccountDAO
    {
        ExtendAccount GetLikeStatus(long accountid);

        int SetLikeStatus(long accountid, bool like, bool vote);

        long QuickRegister(string username, string password, string ipAddress, int merchantId, int platformId);

        AccountDb Register(string username, string password, string nickName, string ipAddress, int merchantId, int platformId, string campaignSource, string uuid, out int response, int LocationID = 16);

        AccountDb Login(string userName, string password, string ipaddress, string uiid, int merchantId, int platformId, out int response);
        string GetInfoByMobile(string mobile, out long accountID);

        AccountDb VipCodeLogin(int serviceId, string key, string ipAddress, string userName, string authKey, bool isMD5 = false, string UUIID = "");


        //AccountDb LoginMobile(int serviceId, string serviceKey, string ipAddress, string username, string password, bool isMD5 = false);
        int CheckLoginOTP(long accountID, int serviceId, string ip, string otp);

        long Authentication(int serviceid, string servicekey, string accountName, string password, string IPAddress, out int EventCoinPresent, out int isOtp, bool isMD5 = false, string UUIID = "");

//        long VipCodeAuthentication(int serviceid, string servicekey, string accountName, string password, string IPAddress, out int EventCoinPresent, out int isOtp, bool isMD5 = false);

        long Authentication(int serviceid, string servicekey, string accountName, string password, string IPAddress, out int EventCoinPresent, out int isOtp, out int isRedirect, out int isViewPayment, out int isViewHotLine, bool isMD5 = false, string UUIID = "");

        long VipCodeAuthentication(int serviceid, string servicekey, string accountName, string password, string IPAddress, out int EventCoinPresent, out int isOtp, out int isRedirect, out int isViewPayment, out int isViewHotLine, bool isMD5 = false,string UUIID = "");

        //AccountDb GetAccount(long accountId, string username, int isOTP = 0, int eventCoin = 0);
        AccountDb GetAccount(long accountId, string username, int isOTP = 0, int eventCoin = 0, int isGetVcoinBalance = 0, AccountInfoType type = AccountInfoType.ALL);

        ChannelingAccount SelectAccountByFacebook(int partner, string partnerAccountID, string partnerAccount, string listPartnerAccountIDs, int merchantId, int platformId, int serviceID, string passWord, string UUIID = "", string clientIP = "");

        ChannelingAccount SelectAccountByGoogle(int partner, string partnerAccountID, string partnerAccount);

        ChannelingAccount SelectAccountByApple(string appleID, string bundleID, string email, int merchantId, int platformId, string passWord = "", string UUIID = "", string clientIP = "");

        int CreateFacebookAccountEvent(long FacebookId, long accountID, string UserName, string facebookName, string email, int status);

        //int GetBalance_Authenticate(int serviceid, string accessToken, bool isCheck);
        //int GetBalance_Authenticate(int serviceid, string accessToken, int accountId, string accountName, bool isCheck, out long totalGameBalance, out long gameBalance, out int vcoinBalance);

        int GetBalance_Authenticate(long accountId,
                      string accountName, out long starGameBalance, out int vcoinBalance);

        int GetBalance_Authenticate(int serviceid, string accessToken, int accountId, string accountName, bool isCheck);

        long AddCoinToFacebookAccount(long accountId, string accountName);

        /// <summary>
        /// Deprecated by function: AccountDb Login(int serviceId, string serviceKey, string ipAddress, string username, string password);
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        //AccountDb Login(string username, string password);

        //AccountDb GetAccount(long accountId, string username, int eventCoin = 0);

        //long GetAccountID(string username, string password);

        //int Authentication(int serviceid, string ipAddress, string servicekey, string accountName, string password);
        //long Authentication(int serviceid, string servicekey, string accountName, string password, string IPAddress, out int eventCoin, bool isMD5 = false);

        void CheckRedirect(long accountId, out int isRedirect, out int isViewPayment, out int isViewHotline);

        int CheckOTPApp(long accountId, string mobile, string otpAppIn);

        int CheckOTPAppNew(long accountId, string password);

        int CheckOTPSMS(long accountId, string userName, string otp, string mobile, int type = 0);
        //int RegisterOTP(long accountId, string userName, string otp, string mobile, int type = 0);

        int CheckUserInputCard(long accountId);

        AccountEbank Get(long accountId);

        int UpdateAccountFastRegister(AccountEbank account);

        int ChangePassword(long accountId, string password, string passwordNew);

        int LogAccountChangepassWaiting_Insert(long accountId, string accountName, string currentPass, string newPass);

        int CheckAccountExist(string accountName);

        int CheckEmailExist(string email);

        int CheckUpdateAccountFastRegister(long accountId, ref int joinFrom);

        int UpdateUserConfirm(string accountName, int comfirmCode);

        int ResetPassword(string accountName, string password, Int16 logType);

        List<SMSBanking> GetSmsbankingByAccountId(long accountId, ref string userid, ref string username, ref string securitycode, ref int status);

        int CheckOtp(long accountId, string otp, int isOneTime);

        int CreateAccountCampaign(long userId, string userName, int status, string url, int partnerId);

        int UpdateAccountCampaign(long userId, string userName, int status);

        int LogDeviceFingerprint(long accountId, string Ip, string Headers, string PluginHash, string FontHash, string TimeZone, string Video, string ETagIdentity, int ServiceID, int UserEvent);

        bool CheckFrozen(long accountId);

        int EventTarget(int ServiceID, int AccountID, string Username);

        int UpdateUserFullName(string accountName, long accountId, string userFullName, string RefCode = "");
        int UpdateUserFullNameFB(long accountId, string accountName, string password, string userFullName);

        void Logout(long accountId);

        int UpdateInfo(long accountId, string email, string mobile, string passport, int avatar = -1);

        SecurityInfo GetSecurityInfo(long accountId);

        int CreateSecurityEmail(long accountId, string accountName, string email, ref int codeVerify);
        int CreateSecuritySMS(long accountId, string accountName, string mobile, int type, ref int codeVerify);

        int VerifySecurityEmail(long accountId, string accountName, string email, int codeVerify);

        /// <summary>
        /// Đăng ký OTP
        /// </summary>
        /// <param name="type">
        /// type: 1--> Đăng ký OTP; 2--> Hủy OTP
        ///
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        int RegisterOTP(long accountId, string userName, string mobile, int register, out long balance);
        int UnRegisterOTP(long accountId, string accountName, string mobile, int codeVerify);

        int CancelSecurityEmail(long accountId, string accountName, string email, int codeVerify);
        int CancelSecuritySMS(long accountId, string accountName, string mobile, int codeVerify);
        int RegisterLoginOTP(long accountId, string accountName, int registerType, int minValue = 0);

        long Frozen(long accountId, long bonFrozen, int serverId, string serviceKey, bool frozen, int sourceId, string clientIP, out long balance, out long frozenValue);
        long GetFrozenValue(long accountID, out long balance);

        int UpdateMobile(long accountId, string mobile);
        int DeleteMobile(long accountId, string mobile);

        int GetAccountByNickName(string nickName, out string userName, out int accountId);
        AccountDb GetAccountByNickName(string nickName);

        List<OTPInfor> GetSecurityInfoByMobile(string mobile);

        AccountLoyalty GetLoyalty(long accountId);
        int GetChatStatus(long accountID);

        long VipPointTrade(long accountID, string accountName, int vipPoint, out int vipPointCurrent);
        List<RankingVipPoint> GetRankingVip();

        int CheckUpdateMobile(long accountID);

        int ChangeMoneyFishing(long accountID, int amount, string desc, int type, int sourceID, out FishingInfo fishingInfo);
        List<Country> GetLocationRegister();
        List<MobileCode> GetMobileCodes();
    }
}