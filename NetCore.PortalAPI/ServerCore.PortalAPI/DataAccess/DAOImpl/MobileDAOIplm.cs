using Microsoft.Extensions.Options;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess.DTO;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NetCore.Utils.Interfaces;

namespace ServerCore.DataAccess.DAOImpl
{
    public class MobileDAOIplm : IMobileDAO
    {
        private readonly AppSettings appSettings;
        private readonly IDBHelper _dbHelper;

        public MobileDAOIplm(IDBHelper dbHelper, IOptions<AppSettings> options)
        {
            appSettings = options.Value;
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// lấy thông tin tài khoản facebook gắn kết
        /// </summary>
        /// <param name="accountid"></param>
        /// <param name="facebook"></param>
        /// <returns></returns>
        public int GetFacebookAccount(long accountid, out string facebook)
        {
            try
            {
                SqlParameter[] pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_FacebookAccounts", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHelper.ExecuteScalarSP("SP_InvitedEvent_GetFacebookAccountByAccountID", pars);
                int res = Convert.ToInt32(pars[2].Value);
                if (res > 0) facebook = pars[1].Value.ToString();
                else facebook = "";
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                facebook = "";
                return -99;
            }
           
        }

        /// <summary>
        /// Gắn kết facebook vào bảng sự kiện
        /// </summary>
        /// <param name="accountid"></param>
        /// <param name="facebook"></param>
        /// <returns></returns>
        public int UpdateFacebookAccount(long accountid, string facebook)
        {
            try
            {
                SqlParameter[] pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_FacebookAccounts", facebook);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHelper.ExecuteScalarSP("SP_InvitedEvent_UpdateFacebookAccount", pars);
                int res = Convert.ToInt32(pars[2].Value);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
           
        }

        ///
        public int SetBitEvent(long accountid, int quantity, int playedQuantity)
        {
      
            try
            {
                SqlParameter[] pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_Quantity", quantity);
                pars[2] = new SqlParameter("@_Offset", playedQuantity);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_InvitedEvent_SetBit", pars);
                return Convert.ToInt32(pars[3].Value.ToString());
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -1;
            }
          
        }

        public List<InvitedEvent> GetEventAccountInfo(int accountid)
        {
     
            try
            {
                SqlParameter[] pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                var result = _dbHelper.GetListSP<InvitedEvent>("SP_InvitedEvent_Get", pars);
                if (result.Count <= 0)
                {
                    var lst = new List<InvitedEvent>();
                    lst.Add(new InvitedEvent() { AccountID = accountid, Bonus = 0, InvitedQuantity = 0, Offset = 0 });
                    return lst;
                }
                else
                    return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
           
        }

        public int SetInvitedEvent(int accountid, int quantity, int offset)
        {
            try
            {
                SqlParameter[] pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_Quantity", quantity);
                pars[2] = new SqlParameter("@_Offset", offset);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_InvitedEvent_Set", pars);
                return Convert.ToInt32(pars[3].Value.ToString());
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -1;
            }
        }

        public List<MobileBaseMoney> GetBaseMoney()
        {
           
            try
            {
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var result = _dbHelper.GetListSP<MobileBaseMoney>("SP_Mobile_Get_BaseMoney");
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
           
        }

        public List<MobileLink> GetLinks(int type)
        {
            try
            {
                SqlParameter[] pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@type", type);
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var result = _dbHelper.GetListSP<MobileLink>("SP_Mobile_Get_Links", pars);
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
        }

        public List<MobileLink3> GetLinks3(int type)
        {
            try
            {
                SqlParameter[] pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@type", type);
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var result = _dbHelper.GetListSP<MobileLink3>("SP_Mobile_Get_Links3", pars);
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
           
        }

        public int UpdateEmail(long accountid, string email)
        {
           
            try
            {
                SqlParameter[] pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_Email", email);
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var result = _dbHelper.ExecuteNonQuerySP("SP_Accounts_AddEmail", pars);
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -1;
            }
           
        }

        public List<MobileVersion> GetVersion(string os, int partner_id)
        {
           
            try
            {
                SqlParameter[] pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@os", os);
                pars[1] = new SqlParameter("@partner_id", partner_id);
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var result = _dbHelper.GetListSP<MobileVersion>("SP_Mobile_Get_Version", pars);
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
           
        }

        public int SetEvent(string eventUrl, string eventImageUrl, string gitfUrl)
        {
            
            try
            {
                SqlParameter[] pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_EventUrl", eventUrl);
                pars[1] = new SqlParameter("@_EventImageUrl", eventImageUrl);
                pars[2] = new SqlParameter("@_GiftUrl", gitfUrl);
                _dbHelper.SetConnectionString(appSettings.BillingAuthenticationAPIConnectionString);
                var result = _dbHelper.ExecuteNonQuerySP("SP_Mobile_SetEvent", pars);
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -1;
            }
        }
    }
}