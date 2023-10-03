using Microsoft.Extensions.Options;
using NetCore.Utils.Interfaces;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess.DTO;
using ServerCore.PortalAPI.Models;
using ServerCore.Utilities.Database;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Security;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ServerCore.DataAccess.DAOImpl
{
    public class AccountDAOImpl : IAccountDAO
    {
        private readonly AppSettings appSettings;
        private readonly IDBHelper _dbHepler;

        public AccountDAOImpl(IDBHelper dbHepler, IOptions<AppSettings> options)
        {
            appSettings = options.Value;
            _dbHepler = dbHepler;
        }

        #region Đăng nhập, đăng ký, các api gốc

        /// <summary>
        /// Function QuickRegister
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="ipAddress"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public long QuickRegister(string username, string password, string ipAddress, int merchantId, int platformId)
        {
            try
            {
                var pars = new SqlParameter[13];
                pars[0] = new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey);
                pars[1] = new SqlParameter("@_AccountName", username);
                pars[2] = new SqlParameter("@_Password", password);
                pars[3] = new SqlParameter("@_Email", DBNull.Value);
                pars[4] = new SqlParameter("@_Passport", DBNull.Value);
                pars[5] = new SqlParameter("@_Mobile", DBNull.Value);
                pars[6] = new SqlParameter("@_QuestionID", DBNull.Value);
                pars[7] = new SqlParameter("@_Answer", DBNull.Value);
                pars[8] = new SqlParameter("@_Status", 1);
                pars[9] = new SqlParameter("@_AccountTypeID", platformId); // os(int)
                pars[10] = new SqlParameter("@_JoinFrom", merchantId); // merchant(int)
                pars[11] = new SqlParameter("@_ClientIP", ipAddress);
                pars[12] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var sqlCommand = new SqlCommand
                    {
                        CommandText = "SP_Account_CreateAccounts",
                        CommandType = CommandType.StoredProcedure
                    };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuery(sqlCommand, pars);

                return Convert.ToInt32(pars[12].Value);
            }
            catch (Exception exception)
            {
                NLogManager.Exception(exception);
                return -99;
            }
        }

        /// <summary>
        /// Function QuickRegister
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="ipAddress"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public AccountDb Register(string username, string password, string nickName, string ipAddress, int merchantId, int platformId, string campaignSource, string uuid, out int response, int LocationID)
        {
            //PROCEDURE[dbo].[SP_Account_CreateAccounts_1]
            //    @_AccountName VARCHAR(30)
            //   ,@_Password VARCHAR(120)
            //   ,@_NickName VARCHAR(30)
            //   ,@_Email VARCHAR(150)
            //   ,@_AccountTypeID INT = 0-- 0: tai khoan thuong, 1: tai khoan Facebook
            //   ,@_MerchantId INT = 0
            //   , @_PlatformId        INT = 0-- 0: android, 1: ios, 2: windows - pc, 3: web
            //   ,@_ClientIP VARCHAR(30) = ''
            //   ,@_AccountID BIGINT OUTPUT
            //   ,@_TotalXu BIGINT OUTPUT
            //   ,@_TotalCoin BIGINT OUTPUT
            //   ,@_Avatar INT OUTPUT
            //   ,@_ResponseStatus INT OUTPUT
            response = (int)ErrorCodes.SERVER_ERROR;
            try
            {
                var pars = new SqlParameter[17];
                pars[0] = new SqlParameter("@_AccountName", username);
                pars[1] = new SqlParameter("@_Password", password);
                pars[2] = new SqlParameter("@_NickName", nickName);
                pars[3] = new SqlParameter("@_Email", DBNull.Value);
                pars[4] = new SqlParameter("@_AccountTypeID", 0);
                pars[5] = new SqlParameter("@_MerchantId", merchantId);
                pars[6] = new SqlParameter("@_PlatformId", platformId);
                pars[7] = new SqlParameter("@_ClientIP", ipAddress);
                pars[8] = new SqlParameter("@_CampaignSource", campaignSource);
                pars[9] = new SqlParameter("@_DeviceID", uuid);
                pars[10] = new SqlParameter("@_AccountID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_TotalXu", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[12] = new SqlParameter("@_TotalCoin", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[13] = new SqlParameter("@_Avatar", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[14] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[15] = new SqlParameter("@_LocationID", LocationID);
                pars[16] = new SqlParameter("@_PreFix", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output };

                //AccountDb accountDb = db.GetInstanceSP<AccountDb>("SP_Account_CreateAccounts_1", pars);

                var sqlCommand = new SqlCommand
                {
                    CommandText = "SP_Account_CreateAccounts",
                    CommandType = CommandType.StoredProcedure
                };
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHepler.ExecuteNonQuery(sqlCommand, pars);

                AccountDb accountDb = null;
                NLogManager.Info("ResponseStatus: " + pars[14].Value);
                response = Convert.ToInt32(pars[14].Value);
                if (response < 0)
                {
                    if (response == -1)
                    {
                        response = (int)ErrorCodes.ACCOUNT_EXIST;
                        return accountDb;
                    }

                    if (response == -2)
                    {
                        response = (int)ErrorCodes.NICKNAME_EXIST;
                        return accountDb;
                    }

                    if (response == -3)
                    {
                        response = (int)ErrorCodes.IP_BLOCK;
                        return accountDb;
                    }


                    response = (int)ErrorCodes.REGISTER_ACCOUNT_ERROR;
                    return accountDb;
                }
            
                if(response >= (int)ErrorCodes.SUCCESS)
                {
                    accountDb = new AccountDb();
                    accountDb.AccountID = Convert.ToInt64(pars[10].Value);
                    accountDb.TotalCoin = Convert.ToInt64(pars[12].Value);
                    accountDb.TotalXu = Convert.ToInt64(pars[11].Value);
                    accountDb.UserName = username;
                    accountDb.UserFullname = nickName;
                    accountDb.Avatar = Convert.ToInt32(pars[13].Value);
                    accountDb.PlatformID = platformId;
                    accountDb.LocationID = LocationID;
                    accountDb.PreFix = Convert.ToString(pars[16].Value);
                    return accountDb;
                }
                return null;
            }
            catch(Exception exception)
            {
                NLogManager.Exception(exception);
                return null;
            }
        }

        public AccountDb Login(string userName, string password, string ipaddress, string uiid, int merchantId, int platformId, out int response)
        {
            //var watch = Stopwatch.StartNew();

            /*
             ALTER PROCEDURE [dbo].[SP_BettingGame_Authenticate_2]
                @_Username 			VARCHAR(30), 
                @_Password 			VARCHAR(50),
                @_ClientIP			VARCHAR(20) = '',
	            @_UIID				VARCHAR( 150) = '',
	            @_PlatformId		INT = 0, -- 1: android, 2: ios, 3: windows-pc, 4: web
	            @_MerchantId		INT = 0,
                @_ResponseStatus 	INT OUTPUT 
             */
            response = -1;
            try
            {
           
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_Username", userName);
                pars[1] = new SqlParameter("@_Password", password);
                pars[2] = new SqlParameter("@_ClientIP", ipaddress);
                pars[3] = new SqlParameter("@_UIID", uiid);
                pars[4] = new SqlParameter("@_MerchantId", merchantId);
                pars[5] = new SqlParameter("@_PlatformId", platformId);
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                NLogManager.Info(string.Format("LOGINDB_DB: {0}, {1}", userName, appSettings.BillingAuthenticationAPIConnectionString));
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                AccountDb accountDb = _dbHepler.GetInstanceSP<AccountDb>("SP_BettingGame_Authenticate_2", pars);
                response = Convert.ToInt32(pars[6].Value);

                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("LOGINDB_DB: {0}, {1}", userName, elapsedMs));

                if (response == (int)ErrorCodes.SUCCESS)
                {
                    accountDb.PlatformID = platformId;
                    return accountDb;
                }
               
                return null;
            }
            catch(Exception exception)
            {
                NLogManager.Exception(exception);
                response = (int)ErrorCodes.SERVER_ERROR;
                return null;
            }
           
        }

        public string GetInfoByMobile(string mobile, out long accountID)
        {
            accountID = -1;
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_Mobile", mobile);
                pars[1] = new SqlParameter("@_AccountID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_UserName", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                // SQLAccess.getAuthen().ExecuteNonQuerySP("SP_Account_GetAccountInfoByMobile", pars);
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_GetAccountInfoByMobile", pars);
                accountID = Convert.ToInt64(pars[1].Value);
                if (accountID >0 )
                {
                    return pars[2].Value.ToString();
                }

                return null;
            }
            catch (Exception exception)
            {
                NLogManager.Exception(exception);
                accountID = (int)ErrorCodes.SERVER_ERROR;
                return null;
            }
        }

        public AccountDb VipCodeLogin(int serviceId, string hashKey, string ipAddress, string userName, string uniKey, bool isMD5 = false, string UUIID = "")
        {
            long accountId = -1;
            int eventCoin = 0;
            int isOTP = 0;
            int isRedirect = 0;
            int isViewPayment = 0;
            int isViewHotLine = 0;
            AccountDb accDb = null;
            if ((accountId = VipCodeAuthentication(serviceId, hashKey, userName, uniKey, ipAddress, out eventCoin, out isOTP, out isRedirect, out isViewPayment, out isViewHotLine, isMD5)) > 0)
            {
                //NLogManager.Info("accountId: " + accountId + ",EventCoin: " + eventCoin);
                accDb = GetAccount(accountId, userName, isOTP, eventCoin, 0);
                if (accDb != null)
                {
                    accDb.ClientIP = ipAddress;
                }
            }
            return accDb;
        }

        ///// <summary>
        ///// Function Login -- Duplicate
        ///// </summary>
        ///// <param name="serviceId"></param>
        ///// <param name="serviceKey"></param>
        ///// <param name="ipAddress"></param>
        ///// <param name="username"></param>
        ///// <param name="password"></param>
        ///// <param name="isMD5"></param>
        ///// <returns></returns>
        //public AccountDb LoginMobile(int serviceId, string serviceKey, string ipAddress, string username, string password, bool isMD5 = true)
        //{
        //    long accountId = -1;
        //    int eventCoin = 0;
        //    int isOTP = 0;
        //    AccountDb accDb = null;

        //    if ((accountId = Authentication(serviceId, serviceKey, username, password, ipAddress, out eventCoin, out isOTP, isMD5)) > 0)
        //    {
        //        //NLogManager.LogMessage("accountId:" + accountId + "-- EventCoin:" + eventCoin);
        //        accDb = GetAccount(accountId, username, isOTP, eventCoin);
        //        accDb.RemoteIP = ipAddress;
        //    }

        //    return accDb;
        //}

        /// <summary>
        /// Kiem tra OTP cua TK dang ky dich vu OTP
        ///
        /// "@_ResponseStatus
        ///   >=0: Thành công
        ///   -2: Không có quyền truy cập SP này
        ///   -6: OTP quá hạn
        ///   -7: OTP không chính xác
        ///   -51: TK ko tồn tại
        ///   -99: Lỗi không xác định"
        ///
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="serviceId"></param>
        /// <param name="ip"></param>
        /// <param name="otp"></param>
        /// <returns></returns>
        public int CheckLoginOTP(long accountID, int serviceId, string ip, string otp)
        {
            try
            {
                var pars = new SqlParameter[6];

                pars[0] = new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey);
                pars[1] = new SqlParameter("@_AccountID", accountID);
                pars[2] = new SqlParameter("@_ServiceID", serviceId);
                pars[3] = new SqlParameter("@_ClientIP", ip);
                pars[4] = new SqlParameter("@_OTP", otp);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_AuthenticateOTP", pars);

                int responseStatus;
                responseStatus = Convert.ToInt32((pars[5].Value.ToString()));
                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        /// <summary>
        /// Kiểm tra mã OTP App
        /// Hoang Duc Tuan
        /// 14-04-2014
        /// </summary>
        /// <param name="serviceId">
        /// 402: OTP App
        /// </param>
        /// <param name="systemId">
        /// VTCID =2
        /// </param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>
        /// >=0: Thành công
        /// <0 : Thất bại
        /// </returns>
        public int CheckOTPApp(long accountId, string mobile, string otpAppIn)
        {
         //   ALTER PROCEDURE[dbo].[SP_OTPApp_GenOTPApp]
         //   @_AccountID   INT,
         //   @_Mobile      VARCHAR(20),
	     //   @_OTPApps     VARCHAR(100) OUTPUT
         //   AS

            //int res = -100;
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Mobile", mobile);
                pars[2] = new SqlParameter("@_OTPApps", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_OTPApp_GenOTPApp", pars);

                string otpApp = "";

                if(!DBNull.Value.Equals(pars[2].Value))
                    otpApp = pars[2].Value.ToString();

                if(!string.IsNullOrEmpty(otpApp))
                {
                    string[] otpSplit = otpApp.Split(',');
                    foreach(string it in otpSplit)
                    {
                        if(it.Equals(otpAppIn))
                            return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
          
            return -1;
        }

        public int CheckOTPAppNew(long accountId, string password)
        {
         //   ALTER PROCEDURE[dbo].[SP_AppOTP_GenOTPApp]
         //   @_AccountID INT,
         //   @_OTP   VARCHAR(100) OUTPUT,
	     //   @_ResponseStatus INT OUTPUT
         //   AS

            int res = -100;
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_OTP", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_AppOTP_GenOTPApp", pars);
                res = Convert.ToInt32((pars[2].Value.ToString()));

                if(res >= 0)
                {
                    string otp = pars[1].Value.ToString();
                    string[] otpSplit = otp.Split(',');
                    foreach(string it in otpSplit)
                    {
                        if(it.Equals(password))
                            return 1;
                    }
                }

                return -1;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
                res = -99;
            }
           
            return res;
        }

        /// <summary>
        /// Kiểm tra mã OTP SMS
        /// </summary>
        /// <param name="otp"> Mã OTP </param>
        /// <param name="type">
        /// 0: Kiểm tra mã OTP, đăng ký OTP nếu chưa đăng ký OTP
        /// 9: Hủy Login OTP
        /// 11: Hủy OTP, Hủy Login OTP
        /// </param>
        /// <param name="accountId"></param>
        /// <param name="userName"></param>
        /// <returns>
        /// >=0: Thành công
        /// <0 : Thất bại
        /// </returns>
        public int CheckOTPSMS(long accountId, string userName, string otp, string mobile, int type = 0)
        {
            //ALTER PROCEDURE[dbo].[SP_Account_VerifyOTP]
            //@_AccountID INT = NULL OUTPUT,
            //@_Username VARCHAR(30) = NULL ,
            //@_UserSMS NVARCHAR(20) = NULL,
            //@_TYPE INT = 0, -- sms 0,1,3 dk get otp , 2 APP, 9 huy
            //@_CodeVerifying     INT = 0 ,
            //@_ResponseStatus INT  OUTPUT
            //AS

            int res = -1;
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", userName);
                pars[2] = new SqlParameter("@_UserSMS", mobile);
                pars[3] = new SqlParameter("@_TYPE", type); 
                pars[4] = new SqlParameter("@_CodeVerifying", Convert.ToInt32(otp));
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_VerifyOTP", pars);
                res = Convert.ToInt32(pars[5].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
                res = -99;
            }
           
            return res;
        }

        /// <summary>
        /// Kiểm tra mã OTP SMS
        /// </summary>
        /// <param name="otp">
        /// Mã OTP
        /// </param>
        /// <param name="type">
        /// 0: Kiểm tra mã OTP, đăng ký OTP nếu chưa đăng ký OTP
        /// 9: Hủy Login OTP
        /// 11: Hủy OTP, Hủy Login OTP
        /// 10: Hủy So dien thoai
        /// </param>
        /// <param name="accountId"></param>
        /// <param name="userName"></param>
        /// <returns>
        /// >=0: Thành công
        /// <0 : Thất bại
        /// </returns>
        public int RegisterOTP(long accountId, string userName, string mobile, int register,out long balance)
        {
            //   PROCEDURE[dbo].[SP_Account_RegisterOTP]
            //   @_AccountID INT = NULL,
            //   @_UserName          VARCHAR(30) = NULL ,
            //   @_Register INT = 0, --0: UnRegister, 1: Register
            //   @_UserMobile        NVARCHAR(20),
            //   @_ResponseStatus INT  OUTPUT
            balance = 0;

            int res = -1;
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_UserName", userName);
                pars[2] = new SqlParameter("@_Register", register);
                pars[3] = new SqlParameter("@_UserMobile", mobile);
                pars[4] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_RegisterOTP", pars);
                res = Convert.ToInt32(pars[5].Value);
                if (res >= 0)
                    balance = Convert.ToInt64(pars[4].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return res;
        }

        public ExtendAccount GetLikeStatus(long accountid)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var res = _dbHepler.GetInstanceSP<ExtendAccount>("SP_LinkedAccount_Get", pars);

                return res == null ? new ExtendAccount() : res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ExtendAccount();
            }
        }

        public int SetLikeStatus(long accountid, bool like, bool vote)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_isLiked", like);
                pars[2] = new SqlParameter("@_isVoted", vote);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_LinkedAccount_Set", pars);
                return Convert.ToInt32(pars[3].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        public int CreateFacebookAccountEvent(long FacebookId, long accountID, string UserName, string facebookName, string email, int status)
        {
            try
            {
                var pars = new SqlParameter[8];
                pars[0] = new SqlParameter("@_FacebookID", FacebookId);
                pars[1] = new SqlParameter("@_UserID", accountID);
                pars[2] = new SqlParameter("@_UserName", UserName);
                pars[3] = new SqlParameter("@_FacebookName", facebookName);
                pars[4] = new SqlParameter("@_Email", email);
                pars[5] = new SqlParameter("@_Status", status);
                pars[6] = new SqlParameter("@_IP", "");
                pars[7] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                // db = new DBHelper(appSettings.EventConnectionString);
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Events_CreateFacebookAccount", pars);

                int responseStatus;
                responseStatus = Convert.ToInt32((pars[7].Value.ToString()));
                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
           
        }

        public ChannelingAccount SelectAccountByFacebook(int partner, string partnerAccountID, string partnerAccount, string listPartnerAccountIDs, int merchantId, int platformId, int serviceID = 1, string passWord = "", string UUIID = "", string clientIP = "")
        {
           try
            {
                NLogManager.Info(string.Format("partner = {0}, partnerAccountID = {1}, partnerAccount = {2}, listPartnerAccountIDs = {3}", partner, partnerAccountID, partnerAccount, listPartnerAccountIDs));
                SqlParameter[] pars = new SqlParameter[10];

                pars[0] = new SqlParameter("@Partner", partner); //3
                pars[1] = new SqlParameter("@PartnerAccountID", partnerAccountID); //
                pars[2] = new SqlParameter("@PartnerAccount", partnerAccount); //
                pars[3] = new SqlParameter("@Merchant", merchantId); //1
                pars[4] = new SqlParameter("@PartnerAccountIDs", listPartnerAccountIDs); //1153946827954259;1228904007125207
                pars[5] = new SqlParameter("@Os", platformId);
                pars[6] = new SqlParameter("@ServiceID", serviceID);
                pars[7] = new SqlParameter("@PassWord", passWord);
                pars[8] = new SqlParameter("@UIID", UUIID); //@UIID @PassWord
                pars[9] = new SqlParameter("@ClientIP", clientIP); //IP
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                var result = _dbHepler.GetInstanceSP<ChannelingAccount>("SP_Channeling_SelectByPartner", pars);
                
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ChannelingAccount();
            }
        }

        public ChannelingAccount SelectAccountByGoogle(int partner, string partnerAccountID, string partnerAccount)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@Partner", partner);
                pars[1] = new SqlParameter("@PartnerAccountID", partnerAccountID);
                pars[2] = new SqlParameter("@PartnerAccount", partnerAccount);
                pars[3] = new SqlParameter("@SysPartnerKey", appSettings.SysPartnerKey);

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var result = _dbHepler.GetInstanceSP<ChannelingAccount>("SP_Channeling_SelectByPartner", pars);

                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ChannelingAccount();
            }
        }

        public ChannelingAccount SelectAccountByApple(string appleID, string bundleID, string email, int merchantId, int platformId, string passWord = "", string UUIID = "", string clientIP = "")
        {
            // @AppleID varchar(100) = NULL
            //,@BundleID nvarchar(100) = NULL
            //,@Email nvarchar(100)
            //,@Merchant  int = 1
            //,@Os int
            //,@PassWord varchar(120)
            //,@UIID NVARCHAR( 150) = ''
            //,@ClientIP varchar(50) = ''
            //,@AccountID bigint OUTPUT
            //,@AccountName nvarchar(100) OUTPUT
            try
            {
                NLogManager.Info(string.Format("appleID = {0}, bundleID = {1}, email = {2}", appleID, bundleID, email));
                SqlParameter[] pars = new SqlParameter[10];

                pars[0] = new SqlParameter("@AppleID", appleID); //3
                pars[1] = new SqlParameter("@BundleID", bundleID); //
                pars[2] = new SqlParameter("@Email", email); //
                pars[3] = new SqlParameter("@Merchant", merchantId); //1
                pars[4] = new SqlParameter("@Os", platformId);
                pars[5] = new SqlParameter("@PassWord", passWord);
                pars[6] = new SqlParameter("@UIID", UUIID); //@UIID
                pars[7] = new SqlParameter("@ClientIP", clientIP); //IP
                pars[8] = new SqlParameter("@AccountID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@AccountName", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Channeling_LoginApple", pars);

                long accountID = 0;
                string accountName = null;

                if (!DBNull.Value.Equals(pars[8].Value))
                    accountID = Convert.ToInt64(pars[8].Value);

                if (!DBNull.Value.Equals(pars[9].Value))
                    accountName = Convert.ToString(pars[9].Value);

                if (accountID > 0 && accountName != null)
                {
                    ChannelingAccount channelingAccount = new ChannelingAccount();
                    channelingAccount.AccountID = accountID;
                    channelingAccount.AccountName = accountName;
                    return channelingAccount;
                }

                return null;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
        }
        /// <summary>
        /// Function Authentication
        /// </summary>
        /// <param name="serviceid"></param>
        /// <param name="servicekey"></param>
        /// <param name="accountName"></param>
        /// <param name="password"></param>
        /// <param name="IPAddress"></param>
        /// <param name="EventCoinPresent"></param>
        /// <param name="isOtp"></param>
        /// <param name="isMD5"></param>
        /// <returns></returns>
        public long Authentication(int serviceid, string key, string accountName, string authKey, string IPAddress, out int EventCoinPresent, out int isLoginOtp, out int isRedirect, out int isViewPayment, out int isMobileConfirm, bool isMD5 = false, string UUIID = "")
        {
            //PROCEDURE[dbo].[SP_BettingGame_Authenticate]
            //@_ServiceID INT = 0,
            //@_Username          NVARCHAR(30), 
            //@_Password VARCHAR(50),
            //@_Salt              VARCHAR(50),
            //@_ClientIP          VARCHAR(20) = '',
            //@_UIID              NVARCHAR(150) = '',
            //@_IsLoginedOTP      INT = 0 OUTPUT,
            //@_IsMobileConfirmed INT = 0 OUTPUT,
            //@_ResponseStatus    INT OUTPUT

            //NLogManager.Info(string.Format("serviceid = {0}, accountName = {1}, key={2}, authKey={3}", serviceid, accountName, key, authKey));
            int res = -99;
            isRedirect = 0;
            isMobileConfirm = 0;
            isViewPayment = 0;
            isLoginOtp = 0;
            EventCoinPresent = 0;

            try
            {
                var pars = new SqlParameter[9];
                pars[0] = new SqlParameter("@_ServiceID", serviceid);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_Password", key);
                pars[3] = new SqlParameter("@_Salt", key);
                pars[4] = new SqlParameter("@_ClientIP", IPAddress);
                pars[5] = new SqlParameter("@_UIID", UUIID);
                pars[6] = new SqlParameter("@_IsLoginedOTP", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_IsMobileConfirmed", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_BettingGame_Authenticate", pars);

                if (!DBNull.Value.Equals(pars[8].Value))
                    res = Convert.ToInt32(pars[8].Value);

                if (!DBNull.Value.Equals(pars[6].Value))
                    isLoginOtp = Convert.ToInt32(pars[6].Value);

                if(!DBNull.Value.Equals(pars[7].Value))
                    isMobileConfirm = Convert.ToInt32(pars[7].Value);

                if (res < 0)
                {
                    //DECLARE @_ERR_NOT_EXIST_ACCOUNT INT = -50;
                    //DECLARE @_ERR_INVALID_PASSWORD INT = -53;
                    //DECLARE @_ERR_ACCOUNT_BLOCKED INT = -54;
                    string mes = string.Empty;
                    if(res == -50)
                        mes = "Tài khoản không tồn tại";
                    else if(res == -53)
                        mes = "Mật khẩu không đúng";
                    else if(res == -54)
                        mes = "Tài khoản bị khóa";

                    NLogManager.Info(string.Format("accountName = {0}, Response = {1}, Message = {2}", accountName, res, mes));
                    isLoginOtp = 0;
                }
                else
                {
                    //isOtp = (int)pars[8].Value;

                    //bool redirect = (bool)pars[8].Value;
                    //isRedirect = redirect ? 1 : 0;

                    //bool viewPayment = (bool)pars[9].Value;
                    //isViewPayment = viewPayment ? 1 : 0;

                    //bool viewHotLine = (bool)pars[10].Value;
                    //isViewHotLine = viewHotLine ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                EventCoinPresent = 0;
                isLoginOtp = 0;
                NLogManager.Exception(ex);
            }
           
            return res;
        }


        /// <summary>
        /// Function Authentication
        /// </summary>
        /// <param name="serviceid"></param>
        /// <param name="servicekey"></param>
        /// <param name="accountName"></param>
        /// <param name="password"></param>
        /// <param name="IPAddress"></param>
        /// <param name="EventCoinPresent"></param>
        /// <param name="isOtp"></param>
        /// <param name="isMD5"></param>
        /// <returns></returns>
        public long VipCodeAuthentication(int serviceid, string key, string accountName, string authKey, string IPAddress, out int EventCoinPresent, out int isOtp, out int isRedirect, out int isViewPayment, out int isViewHotLine, bool isMD5 = false, string UUIID = "")
        {
            //NLogManager.Info(string.Format("serviceid = {0}, accountName = {1}, key={2}, authKey={3}", serviceid, accountName, key, authKey));
            long r = -99;
            isRedirect = 0;
            isViewHotLine = 0;
            isViewPayment = 0;

            if (UUIID == "")
                UUIID = "-1";
            try
            {
                var pars = new SqlParameter[12];
                pars[0] = new SqlParameter("@_ServiceID", serviceid);
                pars[1] = new SqlParameter("@_Vipcode", accountName);
                pars[2] = new SqlParameter("@_Password", authKey);
                pars[3] = new SqlParameter("@_Salt", key);
                pars[4] = new SqlParameter("@_ClientIP", IPAddress);
                pars[5] = new SqlParameter("@_UUIID", UUIID);
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_EventCoinPresent", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_IsCheckOTP", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_IsRedirect", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_IsViewPayment", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_IsViewHotline", SqlDbType.Bit) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_BettingGame_VIPCODE_Authenticate", pars);
                EventCoinPresent = (int)pars[7].Value;

                r = (long)pars[6].Value;

                if (r <= 0)
                {
                    NLogManager.Info(string.Format("accountName = {0}, Response = {1}", accountName, r));
                    isOtp = 0;
                }
                else
                {
                    isOtp = 0;
                    //bool Otp = (bool)pars[7].Value;
                    //isOtp = Otp == false ? 0 : 1;

                    //bool redirect = (bool)pars[8].Value;
                    //isRedirect = redirect ? 1 : 0;

                    //bool viewPayment = (bool)pars[9].Value;
                    //isViewPayment = viewPayment ? 1 : 0;

                    //bool viewHotLine = (bool)pars[10].Value;
                    //isViewHotLine = viewHotLine ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                EventCoinPresent = 0;
                isOtp = 0;

                NLogManager.Exception(ex);
            }
           
            return r;
        }
        //Save this overloaded method to avoid fixing PlayerHandlers.
        public long Authentication(int serviceid, string servicekey, string accountName, string password, string IPAddress, out int EventCoinPresent, out int isOtp, bool isMD5 = false, string UUIID = "")
        {
            int isRedirect;
            int isViewPayment;
            int isViewHotLine;
            return Authentication(serviceid, servicekey, accountName, password, IPAddress, out EventCoinPresent, out isOtp, out isRedirect, out isViewPayment, out isViewHotLine, false, UUIID);
        }

        ///// <summary>
        ///// Function Authentication -- Duplicate
        ///// </summary>
        ///// <param name="serviceid"></param>
        ///// <param name="servicekey"></param>
        ///// <param name="accountName"></param>
        ///// <param name="password"></param>
        ///// <param name="IPAddress"></param>
        ///// <param name="EventCoinPresent"></param>
        ///// <param name="isOtp"></param>
        ///// <param name="isMD5"></param>
        ///// <returns></returns>
        //public long AuthenticationMobile(int serviceid, string servicekey, string accountName, string password, string IPAddress, out int EventCoinPresent, out int isOtp, bool isMD5 = false)
        //{
        //    long r = -99;
        //    DBHelper db = null;
        //    try
        //    {
        //        if (!isMD5)
        //            password = Security.MD5Encrypt(password);

        //        var pars = new SqlParameter[8];
        //        pars[0] = new SqlParameter("@_ServiceID", serviceid);
        //        pars[1] = new SqlParameter("@_Username", accountName);

        //        //pars[2] = new SqlParameter("@_Password", password); //pars[2] = password != null ? new SqlParameter("@_Password", password) : new SqlParameter("@_Password", DBNull.Value);

        //        pars[2] = new SqlParameter("@_Password", Security.MD5Encrypt(string.Format("{0}{1}{2}", accountName, password, servicekey))); //pars[2] = password != null ? new SqlParameter("@_Password", password) : new SqlParameter("@_Password", DBNull.Value);
        //        pars[3] = new SqlParameter("@_Salt", servicekey);
        //        pars[4] = new SqlParameter("@_ClientIP", IPAddress);
        //        //pars[5] = new SqlParameter("@_Token", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output };
        //        //pars[6] = new SqlParameter("@_AccessToken", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output };
        //        //pars[7] = new SqlParameter("@_FullName", SqlDbType.VarChar, 150) { Direction = ParameterDirection.Output };
        //        //pars[8] = new SqlParameter("@_Gender", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //        //pars[9] = new SqlParameter("@_VcoinGame", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //        //pars[10] = new SqlParameter("@_VcoinPayment", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //        pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        //        pars[6] = new SqlParameter("@_EventCoinPresent", SqlDbType.Int) { Direction = ParameterDirection.Output };

        //        // Add by CUONGNV
        //        pars[7] = new SqlParameter("@_IsCheckOTP", SqlDbType.Bit) { Direction = ParameterDirection.Output };

        //        db = new DBHelper(appSettings.CardGameBettingAPIConnectionString);
        //        db.ExecuteNonQuerySP("SP_BettingGame_Authenticate", pars);
        //        EventCoinPresent = (int)pars[6].Value;

        //        r = (long)pars[5].Value;

        //        if (r <= 0)
        //        {
        //            NLogManager.LogMessage(string.Format("Authentication: {0}|{1}", accountName, r));
        //            isOtp = 0;

        //        }
        //        else
        //        {
        //            bool Otp = (bool)pars[7].Value;
        //            isOtp = Otp == false ? 0 : 1;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        EventCoinPresent = 0;
        //        isOtp = 0;
        //        NLogManager.Exception(ex);
        //    }
        //    finally
        //    {
        //        if (db != null)
        //        {
        //            db.Close();
        //        }
        //    }
        //    return r;
        //}

        /// <summary>
        /// Function GetAccount
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="username"></param>
        /// <param name="isOTP"></param>
        /// <param name="eventCoin"></param>
        /// <returns></returns>
        public AccountDb GetAccount(long accountId, string username, int isOTP = 0,
            int eventCoin = 0, int isGetVcoinBalance = 0, AccountInfoType type = AccountInfoType.ALL)
        {

         //   ALTER PROCEDURE[dbo].[SP_Accounts_GetInfo]
         //   @_AccountID INT OUTPUT
	        //,@_UserName VARCHAR(30) OUTPUT
	        //,@_TotalStar BIGINT = 0 OUTPUT
	        //,@_Star BIGINT = 0 OUTPUT
	        //,@_Vcoin INT = 0 OUTPUT
	        //,@_Coin BIGINT = 0  OUTPUT
	        //,@_MerchantID INT = 0 OUTPUT  	
	        //,@_UserEmail VARCHAR(50) OUTPUT
	        //,@_ResponseStatus BIGINT OUTPUT
	        //,@_Type INT = 0--1: Sao; 2: Xu; 0: All
	        //,@_UserFullName VARCHAR(500) OUTPUT
	        //,@_Avatar INT OUTPUT

            try
            {
                //int responseStatus = -1;
                //var param = new SqlParameter[12];
                //param[0] = new SqlParameter("@_AccountID", accountId);
                //param[1] = new SqlParameter("@_UserName", username);
                ////param[1] = new SqlParameter("@_UserName", TMP) { Direction = ParameterDirection.InputOutput };
                //param[2] = new SqlParameter("@_TotalStar", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                //param[3] = new SqlParameter("@_Star", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                //param[4] = new SqlParameter("@_Vcoin", SqlDbType.Int) { Direction = ParameterDirection.Output };
                //param[5] = new SqlParameter("@_Coin", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                //param[6] = new SqlParameter("@_MerchantID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                //param[7] = new SqlParameter("@_UserEmail", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                //param[8] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                //param[9] = new SqlParameter("@_Type", (int)type);
                //param[10] = new SqlParameter("@_UserFullName", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                //param[11] = new SqlParameter("@_Avatar", SqlDbType.Int) { Direction = ParameterDirection.Output };

                int responseStatus = -1;
                var param = new SqlParameter[10];
                param[0] = new SqlParameter("@_AccountID", accountId);
                param[1] = new SqlParameter("@_UserName", username);
                param[2] = new SqlParameter("@_TotalStar", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                param[3] = new SqlParameter("@_Star", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                param[4] = new SqlParameter("@_UserEmail", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                param[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                param[6] = new SqlParameter("@_UserFullName", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                param[7] = new SqlParameter("@_Avatar", SqlDbType.Int) { Direction = ParameterDirection.Output };
                param[8] = new SqlParameter("@_LocationID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                param[9] = new SqlParameter("@_PreFix", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                

    

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.GetInstanceSP<AccountDb>("SP_Accounts_GetInfo", param);

                if(!DBNull.Value.Equals(param[5].Value))
                    responseStatus = Convert.ToInt32((param[5].Value.ToString()));

                if (responseStatus >= 0)
                {
                    long totalBalanceVip = !DBNull.Value.Equals(param[2].Value) ? Convert.ToInt64((param[2].Value.ToString())) : 0;
                    long totalBalanceFree = !DBNull.Value.Equals(param[3].Value) ? Convert.ToInt64((param[3].Value.ToString())) : 0;
                    int avatar = !DBNull.Value.Equals(param[7].Value) ? Convert.ToInt32((param[7].Value)) : 1;
                    string email = param[4].Value.ToString();
                    string userFullname = param[6].Value.ToString();

                    return new AccountDb(accountId, username, totalBalanceVip, totalBalanceFree, 0, 0, 0, isOTP, 0, email, userFullname, 1, avatar);
                }
                else
                {
                    if (responseStatus == -48)
                    {
                        return new AccountDb(accountId, username, 0, 0, 0, 0, 0, isOTP, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return null;
        }

        //public string GetUserFullName(long accountId)
        //{
        //    DBHelper db = null;
        //    string userFullName = null;
        //    try
        //    {
        //        int responseStatus = 0;
        //        var param = new SqlParameter[3];
        //        param[0] = new SqlParameter("@_AccountID", accountId);
        //        param[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int){Direction = ParameterDirection.Output};
        //        param[2] = new SqlParameter("@_UserFullName", SqlDbType.NVarChar, 50){Direction = ParameterDirection.Output};

        //        db = new DBHelper(appSettings.CardGameBettingAPIConnectionString);
        //        db.GetInstanceSP<AccountDb>("SP_Accounts_GetUserFullName", param);
        //        responseStatus = Convert.ToInt32((param[1].Value.ToString()));
        //        if(responseStatus >= 0)
        //        {
        //            userFullName = param[2].Value.ToString();
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        NLogManager.Exception(ex);
        //    }
        //    finally
        //    {
        //        if(db != null)
        //        {
        //            db.Close();
        //        }
        //    }
        //    return userFullName;
        //}

        public int GetBalance_Authenticate(int serviceid, string accessToken, int accountId, string accountName, bool isCheck)
        {
            int r = -99;
            try
            {
                var pars = new SqlParameter[9];
                pars[0] = new SqlParameter("@_ServiceID", serviceid);
                pars[1] = new SqlParameter("@_AccessToken", accessToken);
                pars[2] = new SqlParameter("@_AccountID", accountId) { Direction = ParameterDirection.InputOutput }; ;
                pars[3] = new SqlParameter("@_Username", accountName) { Direction = ParameterDirection.InputOutput };
                pars[4] = new SqlParameter("@_TotalGameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_GameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_VcoinBalance", SqlDbType.Int) { Direction = ParameterDirection.Output }; ;
                pars[7] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_IsCheckAccessToken", isCheck);
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_GetBalance_Authenticate", pars);

                r = (int)pars[7].Value;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return r;
        }

        //public int GetBalance_Authenticate(int serviceid, string accessToken, int accountId,
        //               string accountName, bool isCheck, out long totalGameBalance, out long gameBalance, out int vcoinBalance)
        //{
        //    int r = -99;

        //    try
        //    {
        //        var pars = new SqlParameter[9];
        //        pars[0] = new SqlParameter("@_ServiceID", serviceid);
        //        pars[1] = new SqlParameter("@_AccessToken", accessToken);
        //        pars[2] = new SqlParameter("@_AccountID", accountId) { Direction = ParameterDirection.InputOutput }; ;
        //        pars[3] = new SqlParameter("@_Username", accountName) { Direction = ParameterDirection.InputOutput };
        //        pars[4] = new SqlParameter("@_TotalGameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        //        pars[5] = new SqlParameter("@_GameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        //        pars[6] = new SqlParameter("@_VcoinBalance", SqlDbType.Int) { Direction = ParameterDirection.Output }; ;
        //        pars[7] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //        pars[8] = new SqlParameter("@_IsCheckAccessToken", isCheck);

        //        new DBHelper(appSettings.SSOConnectionString).ExecuteNonQuerySP("SP_Account_GetBalance_Authenticate", pars);

        //        r = (int)pars[7].Value;
        //        totalGameBalance = (long)pars[4].Value;
        //        gameBalance = (long)pars[5].Value;
        //        vcoinBalance = (int)pars[6].Value;

        //    }
        //    catch (Exception ex)
        //    {
        //        totalGameBalance = 0;
        //        gameBalance = 0;
        //        vcoinBalance = 0;
        //        NLogManager.Exception(ex);
        //    }

        //    return r;
        //}
        
        public int GetBalance_Authenticate(long accountId, string accountName, out long starGameBalance, out int vcoinBalance)
        {
            int r = -99;
            starGameBalance = 0;
            vcoinBalance = 0;

            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_StarBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output }; ;
                pars[3] = new SqlParameter("@_VcoinBalance", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_GameCard_GetAccountBalance", pars);

                int returnValue;

                r = int.TryParse(pars[4].Value.ToString(), out returnValue) ? returnValue : 0;
                //NLogManager.Info("GetBalance_Authenticate" + r);
                // r = (int)(pars[4].Value ?? "0");
                starGameBalance = (long)pars[2].Value;
                vcoinBalance = (int)pars[3].Value;
            }
            catch (Exception ex)
            {
                starGameBalance = 0;
                vcoinBalance = 0;
                NLogManager.Exception(ex);
            }

            return r;
        }

        public long AddCoinToFacebookAccount(long accountId, string accountName)
        {
            long response = -99;
            int coin = 0;

            //@_UserID 			BIGINT,
            //@_Username          NVARCHAR(30),
            //@_ResponseStatus 	BIGINT OUTPUT ,
            //@_EventCoinPresent  INT OUTPUT
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_UserID", accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output }; ;
                pars[3] = new SqlParameter("@_EventCoinPresent", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_BettingGame_Authenticate_AddCoinAccountFacebook", pars);

                response = (long)pars[2].Value;
                coin = (int)pars[3].Value;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return response;
        }

        /// <summary>
        /// Deprecated by function: AccountDb Login(int serviceId, string serviceKey, string ipAddress, string username, string password);
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        //public AccountDb Login(string username, string password)
        //{
        //    long accountId = -1;
        //    AccountDb accDb = null;
        //    if ((accountId = GetAccountID(username, password)) > 0)
        //    {
        //        accDb = GetAccount(accountId, username, 0);
        //    }
        //    return accDb;
        //}

        //public int Authentication(int serviceid, string ipAddress, string servicekey, string accountName, string password)
        //{
        //    int r = -99;
        //    try
        //        {
        //            var pars = new SqlParameter[6];
        //            pars[0] = new SqlParameter("@_ServiceID", serviceid);
        //            pars[1] = new SqlParameter("@_ServiceKey", "");
        //            pars[2] = new SqlParameter("@_AccountName", accountName);
        //            pars[3] = new SqlParameter("@_Password", password);
        //            pars[4] = new SqlParameter("@_IPAddress", ipAddress);
        //            pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //            new DBHelper(appSettings.AuthenDBConnectionString).ExecuteNonQuerySP("SP_GO_Authentication", pars);
        //            r = (int)pars[5].Value;

        //        }
        //    catch(Exception ex)
        //    {
        //        NLogManager.Exception(ex);
        //    }
        //    return r;
        //}

        /// <summary>
        /// Get AccountID
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        //public long GetAccountID(string username, string password)
        //{
        //    long lAccountId = -1;
        //    if (username == null || password == null)
        //    {
        //        return -1;
        //    }
        //    else
        //    {
        //        MD5 md5Hash = MD5.Create();
        //        // Convert the input string to a byte array and compute the hash.
        //        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        //        // Create a new Stringbuilder to collect the bytes
        //        // and create a string.
        //        StringBuilder sBuilder = new StringBuilder();

        //        // Loop through each byte of the hashed data
        //        // and format each one as a hexadecimal string.
        //        for (int i = 0; i < data.Length; i++)
        //        {
        //            sBuilder.Append(data[i].ToString("x2"));
        //        }

        //        // Return the hexadecimal string.
        //        string hashedPass = sBuilder.ToString();
        //        DBHelper db = null;
        //        long responseStatus = -99;
        //        try
        //        {
        //            db = new DBHelper(appSettings.AuthenDBConnectionString);
        //            SqlParameter[] pars = new SqlParameter[6];
        //            pars[0] = new SqlParameter("@_ServiceID", 10000);
        //            pars[1] = new SqlParameter("@_IPAddress", "127.0.0.1");
        //            pars[2] = new SqlParameter("@_ServiceKey", "123456");
        //            pars[3] = new SqlParameter("@_AccountName", username);
        //            pars[4] = new SqlParameter("@_Password", hashedPass);
        //            pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

        //            object result = db.ExecuteScalarSP("SP_Authentication_For_GameMini", pars);

        //            if (pars[5].Value != null)
        //            {
        //                responseStatus = (int)pars[5].Value;
        //            }

        //            if (result != null)
        //            {
        //                if (long.TryParse(result.ToString(), out lAccountId))
        //                {
        //                    return lAccountId;
        //                }
        //            }
        //            return responseStatus;
        //        }
        //        catch (System.Exception ex)
        //        {
        //            NLogManager.Exception(ex);
        //            return responseStatus;
        //        }
        //        finally
        //        {
        //            if (db != null)
        //            {
        //                db.Close();
        //            }
        //        }
        //    }
        //}

        public void CheckRedirect(long accountId, out int isRedirect, out int isViewPayment, out int isViewHotline)
        {
            isRedirect = isViewPayment = isViewHotline = 0;

            //CREATE PROCEDURE [dbo].[SP_GameCard_CheckRedirect]
            // @_AccountID   INT,
            // @_IsRedirect  BON OUTPUT,
            // @_IsViewPayment  BON = 0 OUTPUT,
            // @_IsViewHotline  BON = 0 OUTPUT
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_IsRedirect", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_IsViewPayment", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_IsViewHotline", SqlDbType.Bit) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_GameCard_CheckRedirect", pars);

                bool redirect = (bool)pars[1].Value;
                isRedirect = redirect ? 1 : 0;

                bool viewPayment = (bool)pars[2].Value;
                isViewPayment = viewPayment ? 1 : 0;

                bool viewHotLine = (bool)pars[3].Value;
                isViewHotline = viewHotLine ? 1 : 0;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }

        #endregion Đăng nhập, đăng ký, các api gốc

        #region Thông tin tài khoản, mật khẩu

        /// <summary>
        /// Lấy ra thông tin tài khoản ebank dựa vào accountId
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public AccountEbank Get(long accountId)
        {
            try
            {
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                return _dbHepler.GetInstanceSP<AccountEbank>("SP_Account_GetInfoByAccountID"
                    , new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey)
                    , new SqlParameter("@_AccountID", accountId));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new AccountEbank();
            }
        }

        /// <summary>
        /// Update thông tin tài khoản đối với tk đăng ký nhanh qua FB và SMS
        /// </summary>
        /// <param name="account"></param>
        /// <returns>>=0 là thành công, ngược lại là lỗi</returns>
        public int UpdateAccountFastRegister(AccountEbank account)
        {
            try
            {
                var pars = new SqlParameter[14];
                pars[0] = new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey);
                pars[1] = new SqlParameter("@_AccountID", account.AccountID);
                pars[2] = new SqlParameter("@_FullName", string.IsNullOrEmpty(account.Fullname) ? DBNull.Value : (object)account.Fullname);
                pars[3] = new SqlParameter("@_Gender", account.Gender);
                pars[4] = new SqlParameter("@_Birthday", account.Birthday);
                pars[5] = new SqlParameter("@_Passport", string.IsNullOrEmpty(account.Passport) ? DBNull.Value : (object)account.Passport);
                pars[6] = new SqlParameter("@_LocationID", account.LocationID);
                pars[7] = new SqlParameter("@_Mobile", string.IsNullOrEmpty(account.Mobile) ? DBNull.Value : (object)account.Mobile);
                pars[8] = new SqlParameter("@_Address", string.IsNullOrEmpty(account.Address) ? DBNull.Value : (object)account.Address);
                pars[9] = new SqlParameter("@_QuestionID", account.QuestionID);
                pars[10] = new SqlParameter("@_Answer", string.IsNullOrEmpty(account.Answer) ? DBNull.Value : (object)account.Answer);
                pars[11] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[12] = new SqlParameter("@_UserEmail", string.IsNullOrEmpty(account.Email) ? DBNull.Value : (object)account.Email);
                pars[13] = new SqlParameter("@_IsUpdateSecurityInfo", account.IsUpdateSecurityInfo);
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_UpdateAccount", pars);
                return Convert.ToInt32(pars[11].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="password"></param>
        /// <param name="passwordNew"></param>
        /// <returns>>0 la thanh cong</returns>
        public int ChangePassword(long accountId, string password, string passwordNew)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey);
                pars[1] = new SqlParameter("@_AccountID", accountId);
                pars[2] = new SqlParameter("@_Password", password);
                pars[3] = new SqlParameter("@_PasswordNew", passwordNew);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_ChangePassword", pars);
                return Convert.ToInt32(pars[4].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        /// <summary>
        /// Insert thôgn tin đổi mật khẩu vào Log để window service tự động đổi mật khẩu các Game
        /// fifa,audition,trutien...
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="currentPass"></param>
        /// <param name="newPass"></param>
        /// <returns></returns>
        public int LogAccountChangepassWaiting_Insert(long accountId, string accountName, string currentPass, string newPass)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_CurrentPassWord", Security.Encrypt("abc123321", currentPass));
                pars[3] = new SqlParameter("@_NewPassword", Security.Encrypt("abc123321", newPass));
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_LogAccountChangePassWaiting_Insert", pars);
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);

                return -99;
            }
        }

        /// <summary>
        /// Kiểm tra tài khoản có tồn tại hay không
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns>>=0 là tồn tại. nếu = -1  là không tồn tại</returns>
        public int CheckAccountExist(string accountName)
        {
            try
            {
                var pars = new SqlParameter[2];
                //pars[0] = new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey);
                pars[0] = new SqlParameter("@_UserName", accountName);
                pars[1] = new SqlParameter("@_ErrorCode", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_CheckAccountExists", pars);
                return Convert.ToInt32(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        /// <summary>
        /// Kiểm tra xem email đó có tồn tại hay không
        /// </summary>
        /// <param name="email"></param>
        /// <returns>>=0 là có tồn tại, = -1 là không tồn tại</returns>
        public int CheckEmailExist(string email)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey);
                pars[1] = new SqlParameter("@_Email", email);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
               _dbHepler.ExecuteNonQuerySP("SP_Account_CheckEmailExists", pars);
                return Convert.ToInt32(pars[2].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        /// <summary>
        /// Hoang Duc Tuan- 29/08/2013
        /// Kiểm tra tài khoản có cần update thông tin không (dành cho các tài khoản đăng ký nhanh qua Facebook hoặc SMS)
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="joinFrom">
        /// 1: webid; 100: từ goprofile; 101: VTC Smile; 102: TK CDT; 103: TK iBig(app id); 104: cửu long triều; 105:wapid; 106: Từ Facebook
        /// </param>
        /// <returns>
        /// 0: Tài khoản không tồn tại hoặc ko cần update
        /// >0: Cần update thông tin, trả về accountId
        /// -2: Không có quyền truy cập SP
        /// -99: Lỗi ko xác định
        /// </returns>
        /// 
        public int CheckUpdateAccountFastRegister(long accountId, ref int joinFrom)
        {
            joinFrom = 0;
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", appSettings.SysPartnerKey);
                pars[1] = new SqlParameter("@_Email", accountId);
                pars[2] = new SqlParameter("@_Mobile", SqlDbType.Int);
                pars[1] = new SqlParameter("@_CMTND", accountId);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_UpdateInfo", pars);
                joinFrom = pars[2].Value != null ? Convert.ToInt32(pars[2].Value) : 0;
                return Convert.ToInt32(pars[3].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        public int UpdateInfo(long accountId, string email, string mobile, string passport, int avatar = -1)
        {
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Email", email);
                pars[2] = new SqlParameter("@_Mobile", mobile);
                pars[3] = new SqlParameter("@_CMTND", passport);
                pars[4] = new SqlParameter("@_Avatar", avatar);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_UpdateInfo", pars);
                return Convert.ToInt32(pars[5].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
           
        }

        public SecurityInfo GetSecurityInfo(long accountId)
        {
            try
            {
                //var pars = new SqlParameter[8];
                //pars[0] = new SqlParameter("@_AccountID", accountId);
                //pars[1] = new SqlParameter("@_Email", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                //pars[2] = new SqlParameter("@_Mobile", SqlDbType.NVarChar, 15) { Direction = ParameterDirection.Output };
                //pars[3] = new SqlParameter("@_CMTND", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output };
                //pars[4] = new SqlParameter("@_IsEmailActived", SqlDbType.Int) { Direction = ParameterDirection.Output };
                //pars[5] = new SqlParameter("@_IsMobileActived", SqlDbType.Int) { Direction = ParameterDirection.Output };
                //pars[6] = new SqlParameter("@_IsOTP", SqlDbType.Int) { Direction = ParameterDirection.Output };
                //pars[7] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                //db = new DBHelper(appSettings.BillingAuthenticationAPIConnectionString);
                //db.ExecuteNonQuerySP("SP_Accounts_GetSecurityInfo", pars);
                //int res = !DBNull.Value.Equals(pars[7].Value) ? Convert.ToInt32(pars[7].Value) : -1;

                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_Mobile", SqlDbType.VarChar, 20) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_IsLoginOTP", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_GetSecurityInfo", pars);
                int res = !DBNull.Value.Equals(pars[4].Value) ? Convert.ToInt32(pars[4].Value) : -1;

                if(res >= 0)
                {
                    string email = string.Empty;
                    string mobile = !DBNull.Value.Equals(pars[2].Value) ? Convert.ToString(pars[2].Value) : string.Empty;
                    string passport = string.Empty;
                    bool emailActived = false;
                    int status = !DBNull.Value.Equals(pars[1].Value) ? Convert.ToInt32(pars[1].Value) : 0;
                    bool mobileActived = status == 1 ? true : false;
                    bool isLoginOtp = !DBNull.Value.Equals(pars[3].Value) ? Convert.ToBoolean(pars[3].Value) : false;

                    return new SecurityInfo(email, mobile, passport, emailActived, mobileActived, isLoginOtp);
                }

                return new SecurityInfo("", "", "", false, false, false);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
           
        }

        /// <summary>
        /// Update lai ma confirm sau khi hoan thanh muc dich
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="comfirmCode"></param>
        /// <returns> > 0 la thanh cong</returns>
        public int UpdateUserConfirm(string accountName, int comfirmCode)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_ComfirmCode", comfirmCode);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_UpdateConfirmCode", pars);
                return Convert.ToInt32(pars[3].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        /// <summary>
        /// Reset mat khau
        ///
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="password"></param>
        /// <param name="logType"> = 3 la tu ebank</param>
        /// <returns> > 0 la thanh cong</returns>
        public int ResetPassword(string accountName, string password, Int16 logType)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_SysPartnerKey", appSettings.SysPartnerKey);
                pars[1] = new SqlParameter("@_UserName", accountName);
                pars[2] = new SqlParameter("@_UserPassword", password);
                pars[3] = new SqlParameter("@_LogType", logType);
                pars[4] = new SqlParameter("@_ErrorCode", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_ResetPassword", pars);
                return Convert.ToInt32(pars[4].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
            
            //return -1;
        }

        public List<SMSBanking> GetSmsbankingByAccountId(long accountId, ref string userid, ref string username, ref string securitycode, ref int status)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_UserID", SqlDbType.NChar, 15) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_Username", SqlDbType.NVarChar, 30) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_SercurityCode", SqlDbType.VarChar, 4) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_AccountStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var list = _dbHepler.GetListSP<SMSBanking>("sp_SMSBanking_Select", pars);
                userid = pars[1].Value.ToString();
                username = pars[2].Value.ToString();
                securitycode = pars[3].Value.ToString();
                status = int.Parse(pars[4].Value.ToString());
                return list;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new List<SMSBanking>();
            }
        }

        /// <summary>
        /// _ErrorCode=0 hop le
        /// _ErrorCode=-6 OTP qua han
        /// _ErrorCode=-7 OTP khong chinh xac
        /// _ErrorCode=-99 loi khong xac dinh
        /// Kiểm tra mã xác thực OTP|ODP  isOneTime=0--CheckODP; =1--CheckOTP
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="otp"></param>
        /// <param name="isOneTime"></param>
        /// <returns></returns>
        public int CheckOtp(long accountId, string otp, int isOneTime)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_OTP", otp);
                pars[2] = new SqlParameter("@_IsOneTime", isOneTime);
                pars[3] = new SqlParameter("@_ErrorCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("sp_SMSBanking_CheckOTP", pars);
                return Convert.ToInt32(pars[3].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        public int CheckUserInputCard(long accountId)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_IsInputCard", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_EventT08_CheckUserInputCard", pars);
                return Convert.ToInt32(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        #endregion Thông tin tài khoản, mật khẩu

        #region CPLCampaign

        /// <summary>
        /// Insert sự kiện CPL
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="status"></param>
        /// <param name="url"></param>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        public int CreateAccountCampaign(long userId, string userName, int status, string url, int partnerId)
        {
            try
            {
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_UserID", userId);
                pars[1] = new SqlParameter("@_UserName", userName);
                pars[2] = new SqlParameter("@_Status", status);
                pars[3] = new SqlParameter("@_IP", "");
                pars[4] = new SqlParameter("@_PartnerID", partnerId);
                pars[5] = new SqlParameter("@_FullUrl", url);
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Events_CreateAccountsCampaign", pars);

                int responseStatus;
                responseStatus = Convert.ToInt32((pars[6].Value.ToString()));
                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        public int UpdateAccountCampaign(long userId, string userName, int status)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_UserID", userId);
                pars[1] = new SqlParameter("@_AccountName", userName);
                pars[2] = new SqlParameter("@_Status", status);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Events_UpdateAccountsCampaign", pars);

                int responseStatus;
                responseStatus = Convert.ToInt32((pars[3].Value.ToString()));
                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        #endregion CPLCampaign

        #region LogDevice

        public int LogDeviceFingerprint(long accountId, string Ip, string Headers, string PluginHash, string FontHash, string TimeZone, string Video, string ETagIdentity, int ServiceID, int UserEvent)
        {
            try
            {
                var pars = new SqlParameter[11];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_IP", Ip);
                pars[2] = new SqlParameter("@_Headers", Headers);
                pars[3] = new SqlParameter("@_PluginHash", PluginHash);
                pars[4] = new SqlParameter("@_FontHash", FontHash);
                pars[5] = new SqlParameter("@_TimeZone", TimeZone);
                pars[6] = new SqlParameter("@_Video", Video);
                pars[7] = new SqlParameter("@_ServiceID", ServiceID);
                pars[8] = new SqlParameter("@_ETagIdentity", ETagIdentity);
                pars[9] = new SqlParameter("@_UserEvent", UserEvent);
                pars[10] = new SqlParameter("@_Return", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("DeviceFingerprintLog_Insert", pars);

                int r = Convert.ToInt32(pars[10].Value);
                return r;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -1;
            }
        }

        #endregion LogDevice

        public bool CheckFrozen(long accountId)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_GameCard_CheckFrozen", pars);
                return Convert.ToBoolean(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return false;
            }
        }

        public int EventTarget(int ServiceID, int AccountID, string Username)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_ServiceID", ServiceID);
                pars[1] = new SqlParameter("@_AccountID", AccountID);
                pars[2] = new SqlParameter("@_Username", Username);
                pars[3] = new SqlParameter("@_PrizeValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_eBankGame_Event_TargetAccounts_GetInfo", pars);

                int ResponseStatus = Convert.ToInt32(pars[4].Value);
                int PrizeValue = Convert.ToInt32(pars[3].Value);

                NLogManager.Info(string.Format("Event Target: ResponseStatus={0}, PrizeValue={1}", ResponseStatus, PrizeValue));

                if (ResponseStatus > 0)
                    return PrizeValue;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return -1;
        }

        public int UpdateUserFullName(string accountName, long accountId,  string userFullName, string RefCode)
        {
            try
            {
                //RefCode = "211";
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_UserFullName", userFullName);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                pars[4] = new SqlParameter("@_RefCode", RefCode);
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_UpdateUserFullName", pars);
                return Convert.ToInt32(pars[3].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }
        public int UpdateUserFullNameFB(long accountId, string accountName, string password, string userFullName)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_UserFullName", userFullName);
                pars[3] = new SqlParameter("@_Password", password);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_UpdateUserFullNameFB", pars);
                return Convert.ToInt32(pars[4].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
         
            return -1;
        }
    

        public void Logout(long accountId)
        {
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountId);

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_BettingGame_Logout", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }
        /// <summary>
        /// Type: 
        /// 1: Login OTP 
        /// 3: Transfer
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="mobile"></param>
        /// <param name="type"></param>
        /// <param name="codeVerify"></param>
        /// <returns></returns>
        public int CreateSecuritySMS(long accountId, string accountName, string mobile, int type, ref int codeVerify)
        {
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_UserSMS", mobile);
                pars[3] = new SqlParameter("@_TYPE", type);
                pars[4] = new SqlParameter("@_CodeVerifying", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_Create_SMSVerify", pars);

                codeVerify = Convert.ToInt32(pars[4].Value);
                return Convert.ToInt32(pars[5].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }

        public int CreateSecurityEmail(long accountId, string accountName, string email, ref int codeVerify)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_Email", email);
                pars[3] = new SqlParameter("@_CodeVerifying", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int){Direction = ParameterDirection.Output};
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_Create_EmailVerify", pars);

                codeVerify = Convert.ToInt32(pars[3].Value);
                return Convert.ToInt32(pars[4].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }

        public int VerifySecurityEmail(long accountId, string accountName, string email, int codeVerify)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_UserEmail", email);
                pars[3] = new SqlParameter("@_CodeVerifying", codeVerify);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_EmailVerify", pars);

                return Convert.ToInt32(pars[4].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }
        public int RegisterOTP(long accountId, string accountName, string mobile, int codeVerify, int type)
        {
 //             @_AccountID INT = NULL OUTPUT,
	//          @_Username VARCHAR(30) = NULL,
 //             @_UserSMS               NVARCHAR(20),
 //             @_TYPE              INT = 0, --0: dang ky OTP, 9: Huy OTP
 //             @_CodeVerifying INT = 0,
 //             @_ResponseStatus    INT OUTPUT
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_UserSMS", mobile);
                pars[3] = new SqlParameter("@_TYPE", type);
                pars[4] = new SqlParameter("@_CodeVerifying", codeVerify);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_RegisterOTP", pars);

                return Convert.ToInt32(pars[5].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return -1;
        }

        public int CancelSecurityEmail(long accountId, string accountName, string email, int codeVerify)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_UserEmail", email);
                pars[3] = new SqlParameter("@_CodeVerifying", codeVerify);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_CancelSecurityEmail", pars);

                return Convert.ToInt32(pars[4].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return -1;
        }
        public int CancelSecuritySMS(long accountId, string accountName, string mobile, int codeVerify)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_UserSMS", mobile);
                pars[3] = new SqlParameter("@_CodeVerifying", codeVerify);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_CancelSecuritySMS", pars);

                return Convert.ToInt32(pars[4].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }
        public int RegisterLoginOTP(long accountId, string accountName, int registerType, int minValue = 0)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_UserName", accountName);
                pars[2] = new SqlParameter("@_Register", registerType); // 1: register, 0:unregister
                pars[3] = new SqlParameter("@_MinValue", minValue);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_RegisterLoginOTP", pars);

                return Convert.ToInt32(pars[4].Value);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }

        public int UnRegisterOTP(long accountId, string accountName, string mobile, int codeVerify)
        {
            throw new NotImplementedException();
        }

        public long Frozen(long accountId, long bonFrozen, int serverId, string serviceKey, bool frozen, int sourceId, string clientIP, out long balance, out long frozenValue)
        {
            // ALTER PROCEDURE[dbo].[SP_Frozen_Account]
	        //,@_AccountID INT OUTPUT
	        //,@_Amount BIGINT
            //,@_Type                 INT = 0 -- Type =1 Dong bang, type = 2 mo dong bang
	        //,@_Description NVARCHAR(150) = NULL
	        //,@_SourceID INT = 0
            //,@_ClientIP             VARCHAR(15)
	        //,@_GameBalance BIGINT = 0 OUTPUT
	        //,@_FroZenAmount BIGINT OUTPUT
	        //,@_ResponseStatus INT OUTPUT

            balance = -1;
            frozenValue = -1;

            try
            {
                var pars = new SqlParameter[9];
                //pars[0] = new SqlParameter("@_ServiceID", serverId);
                //pars[1] = new SqlParameter("@_ServiceKey", serviceKey);
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Amount", bonFrozen);
                pars[2] = new SqlParameter("@_Type", frozen ? 1 : 2);
                pars[3] = new SqlParameter("@_Description", "");
                pars[4] = new SqlParameter("@_SourceID", sourceId);
                pars[5] = new SqlParameter("@_ClientIP", clientIP);
                pars[6] = new SqlParameter("@_GameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_FroZenAmount", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Frozen_Account", pars);

                long responeStatus = Convert.ToInt64(pars[8].Value);
                if(responeStatus >= 0)
                {
                    balance = Convert.ToInt64(pars[6].Value);
                    frozenValue = Convert.ToInt64(pars[7].Value);
                }
                return responeStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return -1;
        }

        public long GetFrozenValue(long accountId, out long balance)
        {
            //  ALTER PROCEDURE[dbo].[SP_Accounts_GetBalance_UserFullName]
            //  @_AccountID INT = NULL OUTPUT,
            //	@_Username VARCHAR(30) = NULL OUTPUT,
            //  @_TotalGameBalance  BIGINT = 0  OUTPUT,
            //	@_GameBalance BIGINT = 0 OUTPUT,
            //	@_VcoinBalance INT = 0 OUTPUT,		
            //	@_ResponseStatus BIGINT  OUTPUT,
            //	@_IsQuickReg INT = 0,
            //  @_MerchantID        INT = 0 OUTPUT,
            //	@_UserFullName NVARCHAR(30) OUTPUT,
            //	@_FrozenValue BIGINT = 0 OUTPUT
            //AS

            balance = -1;
            try
            {
                var pars = new SqlParameter[10];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", SqlDbType.VarChar, 30) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_TotalGameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_GameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_VcoinBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_IsQuickReg", 0);
                pars[7] = new SqlParameter("@_MerchantID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_UserFullName", SqlDbType.VarChar, 30) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_FrozenValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
               _dbHepler.ExecuteNonQuerySP("SP_Accounts_GetBalance_UserFullName", pars);

                int responeStatus = Convert.ToInt32(pars[5].Value);
                if (responeStatus >= 0)
                {
                    balance = Convert.ToInt64(pars[3].Value);
                    long frozenValue = Convert.ToInt64(pars[9].Value);
                    return frozenValue;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }

        public int UpdateMobile(long accountId, string mobile)
        {
            // ALTER PROCEDURE[dbo].[SP_Accounts_UpdateMobile]
            // @_AccountID INT,
            // @_Mobile    NVARCHAR(15) = N'',
            // @_ResponseStatus INT OUTPUT
            //AS

            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Mobile", mobile);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_UpdateMobile", pars);

                int responeStatus = Convert.ToInt32(pars[2].Value);
                return responeStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }

        public List<OTPInfor> GetSecurityInfoByMobile(string mobile)
        {
            //ALTER PROCEDURE[dbo].[SP_Accounts_GetSecurityInfoByMobile]
            //@_Mobile NVARCHAR(15)
            //AS

            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_Mobile", mobile);
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                var res = _dbHepler.GetListSP<OTPInfor>("SP_Accounts_GetSecurityInfoByMobile", pars);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return new List<OTPInfor>();
        }

        public int DeleteMobile(long accountId, string mobile)
        {
            // ALTER PROCEDURE[dbo].[SP_Accounts_DeleteMobile]
            // @_AccountID INT,
            // @_Mobile    NVARCHAR(15) = N'',
            // @_ResponseStatus INT OUTPUT
            //AS

            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Mobile", mobile);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_DeleteMobile", pars);

                int responeStatus = Convert.ToInt32(pars[2].Value);
                return responeStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }

        public AccountLoyalty GetLoyalty(long accountId)
        {
            // ALTER PROCEDURE[dbo].[SP_Account_Loyalty]
            //    @_AccountID                     BIGINT,
            //    @_VipPoint INT OUTPUT, --Point hiện tại
            //    @_TotalVipPoint INT OUTPUT, --Tổng point
            //    @_Level         INT OUTPUT, --Level
            //    @_ResponseStatus INT OUTPUT
            //AS

            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_VipPoint", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_TotalVipPoint", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_Level", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_Loyalty", pars);

                int responeStatus = -1;//Convert.ToInt32(pars[4].Value);
                int level = 0;
                int totalVipPoint = 0;
                int vipPoint = 0;

                if (!DBNull.Value.Equals(pars[4].Value))
                    responeStatus = Convert.ToInt32(pars[4].Value);

                if (!DBNull.Value.Equals(pars[3].Value))
                    level = Convert.ToInt32(pars[3].Value);

                if (!DBNull.Value.Equals(pars[2].Value))
                    totalVipPoint = Convert.ToInt32(pars[2].Value);

                if (!DBNull.Value.Equals(pars[1].Value))
                    vipPoint = Convert.ToInt32(pars[1].Value);

                if (responeStatus >= 0)
                {
                    return new AccountLoyalty
                    {
                        AccountID = accountId,
                        Level = level,
                        TotalVipPoint = totalVipPoint,
                        VipPoint = vipPoint
                    };
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return null;
        }

        public int GetAccountByNickName(string nickName, out string userName, out int accountId)
        {
            //   ALTER PROCEDURE[dbo].[SP_Accounts_GetInfoByNickName]
            //   @_NickName VARCHAR(30),
            //   @_UserName VARCHAR(30) = '' OUTPUT, 
            //   @_AccountId INT = 0 OUTPUT
            //   AS

            userName = "";
            accountId = -1;

            DBHelper db = null;
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_NickName", nickName);
                pars[1] = new SqlParameter("@_UserName", SqlDbType.VarChar, 30) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_AccountId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_GetInfoByNickName", pars);

                accountId = !DBNull.Value.Equals(pars[2].Value) ? Convert.ToInt32(pars[2].Value) : 0;
                userName = pars[1].Value.ToString();
                return accountId;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }

        public AccountDb GetAccountByNickName(string nickName)
        {
            //  ALTER PROCEDURE[dbo].[SP_Accounts_GetInfoByNickNameFull]
            //  @_NickName VARCHAR(500)
            // ,@_AccountID INT = 0 OUTPUT
            // ,@_UserName VARCHAR(30) = NULL OUTPUT
            // ,@_Mobile VARCHAR(30) = NULL OUTPUT
            // ,@_TotalMoneyPayment BIGINT = 0 OUTPUT
            // ,@_TotalMoneyFree BIGINT = 0 OUTPUT
            // ,@_Avatar INT = 0 OUTPUT
            // ,@_MobileConfirm INT = 0 OUTPUT

            DBHelper db = null;
            try
            {
                var pars = new SqlParameter[8];
                pars[0] = new SqlParameter("@_NickName", nickName);
                pars[1] = new SqlParameter("@_UserName", SqlDbType.VarChar, 30) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_AccountId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_Mobile", SqlDbType.VarChar, 30) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_TotalMoneyPayment", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_TotalMoneyFree", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_Avatar", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_MobileConfirm", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_GetInfoByNickNameFull", pars);

                int accountId = !DBNull.Value.Equals(pars[2].Value) ? Convert.ToInt32(pars[2].Value) : 0;
                if(accountId > 0)
                {
                    string userName = !DBNull.Value.Equals(pars[1].Value) ? pars[1].Value.ToString() : string.Empty;
                    string mobile = !DBNull.Value.Equals(pars[3].Value) ? pars[3].Value.ToString() : string.Empty;
                    long totalMoneyPayment = !DBNull.Value.Equals(pars[4].Value) ? Convert.ToInt64(pars[4].Value) : 0;
                    long totalMoneyPFree = !DBNull.Value.Equals(pars[5].Value) ? Convert.ToInt64(pars[5].Value) : 0;
                    int avatar = !DBNull.Value.Equals(pars[6].Value) ? Convert.ToInt32(pars[6].Value) : 0;
                    int mobileConfirm = !DBNull.Value.Equals(pars[7].Value) ? Convert.ToInt32(pars[7].Value) : 0;

                    AccountDb accountDb = new AccountDb
                    {
                        AccountID = accountId,
                        UserName = userName,
                        UserFullname = nickName,
                        Mobile = mobile,
                        TotalCoin = totalMoneyPayment,
                        TotalXu = totalMoneyPFree,
                        Avatar = avatar,
                        IsMobileActived = mobileConfirm == 1 ? true : false
                    };

                    return accountDb;
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return null;
        }

        public int GetChatStatus(long accountID)
        {
            //ALTER PROCEDURE[dbo].[SP_Accounts_GetChatStatus]
            //@_AccountID INT,
            //@_ResponseStatus  INT OUTPUT

            DBHelper db = null;
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Accounts_GetChatStatus", pars);

                int responseStatus = !DBNull.Value.Equals(pars[1].Value) ? Convert.ToInt32(pars[1].Value) : 0;
                return responseStatus;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return 0;
        }

        public long VipPointTrade(long accountID, string accountName, int vipPoint, out int vipPointCurrent)
        {
            //   ALTER PROCEDURE[dbo].[VipPointTrade]
            //      @_AccountID BIGINT
            //      ,@_UserName             NVARCHAR(50)
            //	    ,@_AmountVP             INT
            //      ,@_MerchantIdTopUp      INT
            //      ,@_MerchantKeyTopUp     INT
            //      ,@_MerchantIdDeduct     INT
            //      ,@_MerchantKeyDeduct    INT
            //      ,@_ResponseStatus   BIGINT OUTPUT
            //      ,@_VipPointCurrent  INT OUTPUT
            //AS
            vipPointCurrent = 0;
            try
            {
                var pars = new SqlParameter[9];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_UserName", accountName);
                pars[2] = new SqlParameter("@_AmountVP", vipPoint);
                pars[3] = new SqlParameter("@_MerchantIdTopUp", 20180102);
                pars[4] = new SqlParameter("@_MerchantKeyTopUp", 123456);
                pars[5] = new SqlParameter("@_MerchantIdDeduct", 20180101);
                pars[6] = new SqlParameter("@_MerchantKeyDeduct", 123456);
                pars[7] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_VipPointCurrent", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("VipPointTrade", pars);

                long responseStatus = !DBNull.Value.Equals(pars[7].Value) ? Convert.ToInt64(pars[7].Value) : -1;
                vipPointCurrent = !DBNull.Value.Equals(pars[8].Value) ? Convert.ToInt32(pars[8].Value) : 0;
                return responseStatus;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1;
        }

        public List<RankingVipPoint> GetRankingVip()
        {
            //ALTER PROCEDURE[dbo].[SP_Account_GetRankingVip]
            //@_ResponseStatus INT OUTPUT
            //AS

            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var res = _dbHepler.GetListSP<RankingVipPoint>("SP_Account_GetRankingVip", pars);
                return res;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return new List<RankingVipPoint>();
        }

        public int CheckUpdateMobile(long accountID)
        {
            //ALTER PROCEDURE[dbo].[SP_Account_CheckUpdateMobile]
            //@_AccountID BIGINT,
            //@_ResponseStatus BIGINT OUTPUT
            //AS

            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Account_CheckUpdateMobile", pars);

                int res = !DBNull.Value.Equals(pars[1].Value) ? Convert.ToInt32(pars[1].Value) : -1;
                return res;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return -1;
        }

        public int ChangeMoneyFishing(long accountID, int amount, string desc, int type, int sourceID,out FishingInfo fishingInfo)
        {
            fishingInfo = null;
            try
            {
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_Amount", amount);
                pars[2] = new SqlParameter("@_Description", desc);
                pars[3] = new SqlParameter("@_Type", type);
                pars[4] = new SqlParameter("@_SourceID", sourceID);
                pars[5] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHepler.ExecuteNonQuerySP("SP_Change_Money_Fishing", pars);

                int res = !DBNull.Value.Equals(pars[6].Value) ? Convert.ToInt32(pars[6].Value) : -1;
                if (res >= 0)
                {
                    fishingInfo = new FishingInfo();
                    fishingInfo.balance = !DBNull.Value.Equals(pars[5].Value) ? Convert.ToInt32(pars[5].Value) : 0;
                    fishingInfo.amount = amount;
                }
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return -1;
        }
        public List<Country> GetLocationRegister()
        {
            //ALTER PROCEDURE[dbo].[SP_Account_GetRankingVip]
            //@_ResponseStatus INT OUTPUT
            //AS

            try
            {
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                var res = _dbHepler.GetListSP<Country>("SP_Account_GetCountryRegister");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return new List<Country>();
        }


        public List<MobileCode> GetMobileCodes()
        {
            //ALTER PROCEDURE[dbo].[SP_Account_GetRankingVip]
            //@_ResponseStatus INT OUTPUT
            //AS

            try
            {
                _dbHepler.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                var res = _dbHepler.GetListSP<MobileCode>("SP_Account_GetMobileCodes");
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return new List<MobileCode>();
        }
    }
}
