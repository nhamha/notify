using ServerCore.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore.DataAccess.DTO;
using ServerCore.Utilities.Utils;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using ServerCore.Utilities.Database;
using ServerCore.PortalAPI.Models;
using Netcore.Gateway.Interfaces;

namespace ServerCore.DataAccess.DAOImpl
{
    public class OTPDAOImpl : IOTPDAO
    {
        private readonly AppSettings appSettings;
        private readonly ISQLAccess SQLAccess;
        public OTPDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
        }

        public OTPs.ResponMes SetupAppToken(OTPs.SetupAppTokenOutput input)
        {
         //   ALTER PROCEDURE[dbo].[SP_OTPApp_SetupAppToken]
         //   @_AccountID INT, --User của hệ thống cần sử dụng phương thức bảo mật
         //   @_Mobile        VARCHAR(50), --Mobile, Email
         //   @_TokenKey      VARCHAR(50) = NULL, --Serial cua dien thoai khi active OTP App
         //   @_Status TINYINT,    --0: Dang ky; 1: Dang su dung(da active); 9: Huy
         //   @_Description   NVARCHAR(250) = NULL, --Cau hinh dien thoai, Dia chi nhan the Matrix
         //   @_VerifyCode VARCHAR(30) OUTPUT,
	     //   @_ResponseCode INT = -1 OUTPUT

            DBHelper db = null;
            try
            {
                //NLogManager.Info("SetupAppToken_input : " + JsonConvert.SerializeObject(input));
            
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_AccountID", input.AccountID);
                pars[1] = new SqlParameter("@_Mobile", input.Mobile);
                pars[2] = new SqlParameter("@_TokenKey", input.TokenKey);
                pars[3] = new SqlParameter("@_Status", input.Status);
                pars[4] = new SqlParameter("@_Description", input.Description);
                pars[5] = new SqlParameter("@_VerifyCode", SqlDbType.NVarChar, 30) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_OTPApp_SetupAppToken", pars);
                NLogManager.Info(string.Format("SetupAppToken_output: VerifyCode = {0}, ResponseCode = {1}", pars[5].Value, pars[6].Value));

                OTPs.ResponMes responMes = new OTPs.ResponMes();
                responMes.ResponCode = Convert.ToInt32(pars[5].Value);

                //if (responseCode >= 0)
                //{
                //    responMes.ResponCode = int.Parse(pars[6].Value.ToString());
                //}
                return responMes;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return new OTPs.ResponMes();
            }
        }

        //public OTPs.ResponMes SetupAppTokenActive(OTPs.SetupAppTokenOutput input)
        //{
        //    DBHelper db = null;
        //    try
        //    {
        //        NLogManager.Info("SetupAppToken_input: " + JsonConvert.SerializeObject(input));

        //        db = new DBHelper(appSettings.BillingOtpAppAPIConnectionString);
        //        var pars = new SqlParameter[10];
        //        pars[0] = new SqlParameter("@_SystemID", input.SystemID);
        //        pars[1] = new SqlParameter("@_SystemUserName", input.SystemUserName);
        //        pars[2] = new SqlParameter("@_AccountName", input.Mobile);
        //        pars[3] = new SqlParameter("@_TokenKey", input.TokenKey);
        //        pars[4] = new SqlParameter("@_ServiceID", input.ServiceID);
        //        pars[5] = new SqlParameter("@_MinAmount", input.MinAmount);
        //        pars[6] = new SqlParameter("@_Status", input.Status);
        //        pars[7] = new SqlParameter("@_Description", input.Description);
        //        pars[8] = new SqlParameter("@_ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //        pars[9] = new SqlParameter("@_VerifyCode", input.VerifyCode) { Direction = ParameterDirection.InputOutput };

        //        db.ExecuteNonQuerySP("SP_Tokens_SetupAppToken", pars);
        //        NLogManager.Info(string.Format("SetupAppToken_output : VerifyCode = {0}, ResponseCode = {1}", pars[9].Value, pars[8].Value));

        //        OTPs.ResponMes responMes = new OTPs.ResponMes();
        //        responMes.ResponCode = int.Parse(pars[8].Value.ToString());

        //        return responMes;
        //    }
        //    catch (Exception e)
        //    {
        //        NLogManager.Exception(e);
        //        return new OTPs.ResponMes();
        //    }
        //}


        public int AccountVerifySMS(OTPs.AccountVerifySms input)
        {
          
            try
            {
                NLogManager.Info("AccountVerifySMS_input : " + JsonConvert.SerializeObject(input));
             
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", input.AccountID) { Direction = ParameterDirection.InputOutput };
                pars[1] = new SqlParameter("@_Username", input.Username);
                pars[2] = new SqlParameter("@_UserSMS", input.UserSMS);
                pars[3] = new SqlParameter("@_TYPE", input.TYPE);
                pars[4] = new SqlParameter("@_CodeVerifying", input.CodeVerifying);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_Account_VerifySMS", pars);

                NLogManager.Info(string.Format("AccountVerifySMS_output : {0}|{1}", pars[0].Value, pars[5].Value));

                return int.Parse(pars[5].Value.ToString());
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return -69;
            }
        }

        public int TokensSyncTime(OTPs.CheckActiveSyncTime input)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountID", input.SystemID);
                pars[1] = new SqlParameter("@_Mobile", input.Mobile);
                pars[2] = new SqlParameter("@_TokenKey", input.TokenKey);
                pars[3] = new SqlParameter("@_ClientUnixTimestamp", input.ClientUnixTimestamp);
                pars[4] = new SqlParameter("@_ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_Tokens_SyncTime", pars);

                NLogManager.Info(string.Format("TokensSyncTime_input: {0} | TokensSyncTime_output: {1}",
                    JsonConvert.SerializeObject(input), pars[4].Value));

                return int.Parse(pars[4].Value.ToString());
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return -69;
            }
        }

        public int TokensCheckActive(OTPs.CheckActiveSyncTime input)
        {
            try
            {
               
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_SystemID", input.SystemID);
                pars[1] = new SqlParameter("@_SystemUserName", input.SystemUserName);
                pars[2] = new SqlParameter("@_AccountName", input.Mobile);
                pars[3] = new SqlParameter("@_TokenKey", input.TokenKey);
                pars[4] = new SqlParameter("@_VerifyCode", input.VerifyCode);
                pars[5] = new SqlParameter("@_ServiceID", input.ServiceID);
                pars[6] = new SqlParameter("@_ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_Tokens_CheckActive", pars);

                return int.Parse(pars[6].Value.ToString());
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return -69;
            }
        }

        public int AccountVerifyOTP(OTPs.AccountVerifyOTPInput input)
        {
            try
            {
                NLogManager.Info("AccountVerifyOTP_input : " + JsonConvert.SerializeObject(input));
               
                var pars = new SqlParameter[6];
                //pars[0] = new SqlParameter("@_AccountID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[0] = new SqlParameter("@_AccountID", input.AccountID) { Direction = ParameterDirection.InputOutput };
                pars[1] = new SqlParameter("@_Username", input.Username);
                pars[2] = new SqlParameter("@_UserSMS", input.UserSMS);
                pars[3] = new SqlParameter("@_TYPE", input.TYPE);
                pars[4] = new SqlParameter("@_CodeVerifying", input.CodeVerifying);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_Account_VerifyOTP", pars);

                NLogManager.Info(string.Format("AccountVerifyOTP_output : {0}|{1}", pars[0].Value, pars[5].Value));

                return int.Parse(pars[5].Value.ToString());
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return -69;
            }
        }

        public int UpdateInfo(OTPs.UpdateInfo input)
        {
  
            try
            {
              
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", input.AccountID);
                pars[1] = new SqlParameter("@_Email", input.Email);
                pars[2] = new SqlParameter("@_Mobile", input.Mobile);
                pars[3] = new SqlParameter("@_CMTND", input.CMTND);
                pars[4] = new SqlParameter("@_Avatar", input.Avatar);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                NLogManager.Info("UpdateInfo_input : " + JsonConvert.SerializeObject(input));

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_Accounts_UpdateInfo", pars);
                NLogManager.Info(string.Format("UpdateInfo_output : {0}", pars[5].Value));

                return int.Parse(pars[5].Value.ToString());
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return -69;
            }
           
        }

        public int ActiveTwoFactor(OTPs.ActiveFactor input)
        {
            //PROCEDURE[dbo].[SP_OTPApp_ActiveTwoFactAuthenticator]
            //@_AccountID     INT, --User của hệ thống cần sử dụng phương thức bảo mật
            //@_AccountName   VARCHAR(50),
            //@_TokenKey      VARCHAR(50) = NULL, --Serial cua dien thoai khi active
            //@_SecretKey     VARCHAR(50) = NULL, --Mã để tạo OTP
            //@_Status        BIT,	--0: Hủy; 1: Active
            //@_ResponseCode  INT = -1 OUTPUT

            try
            {
                NLogManager.Info("SP_OTPApp_ActiveTwoFactAuthenticator: " + JsonConvert.SerializeObject(input));
             
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountID", input.AccountID);
                pars[1] = new SqlParameter("@_AccountName", input.AccountName);
                pars[2] = new SqlParameter("@_TokenKey", input.TokenKey);
                pars[3] = new SqlParameter("@_SecretKey", input.SecretKey);
                pars[4] = new SqlParameter("@_Status", input.Status);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_OTPApp_ActiveTwoFactAuthenticator", pars);

                return int.Parse(pars[5].Value.ToString());
            }
            catch(Exception e)
            {
                NLogManager.Exception(e);
                return -1;
            }
           
        }

        public int GetTwoFactorInfo(long accountID, out string tokenKey, out string secretKey)
        {
            //PROCEDURE[dbo].[SP_OTPApp_GetTwoFactAuthenticatorInfo]
            //@_AccountID INT, --User của hệ thống cần sử dụng phương thức bảo mật
            //@_TokenKey VARCHAR(50) OUTPUT, --Serial cua dien thoai khi active
            //@_SecretKey     VARCHAR(50) OUTPUT, --Mã để tạo OTP
            //@_ResponseCode  INT = -1 OUTPUT

            tokenKey = "";
            secretKey = "";

            try
            {
              
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_TokenKey", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_SecretKey", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_OTPApp_GetTwoFactAuthenticatorInfo", pars);

                tokenKey = DBNull.Value.Equals(pars[1].Value) ? string.Empty : pars[1].Value.ToString();
                secretKey = DBNull.Value.Equals(pars[2].Value) ? string.Empty : pars[2].Value.ToString();

                return int.Parse(pars[3].Value.ToString());
            }
            catch(Exception e)
            {
                NLogManager.Exception(e);
                return -1;
            }
           
        }
    }
}
