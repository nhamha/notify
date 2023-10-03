using Microsoft.Extensions.Options;
using NetCore.Utils.Interfaces;
using ServerCore.DataAccess.DAO;
using ServerCore.PortalAPI.Models;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using static ServerCore.DataAccess.DTO.OTPs;

namespace ServerCore.DataAccess.DAOImpl
{
    public class ReportDAOImpl : IReportDAO
    {
        private readonly AppSettings appSettings;
        private readonly IDBHelper _dbHelper;

        public ReportDAOImpl(IDBHelper dbHelper, IOptions<AppSettings> options)
        {
            appSettings = options.Value;
            this._dbHelper = dbHelper;
        }

        public string CheckCardMaintain(string cardType, int topupType)
        {
            try
            {

                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_Provider", cardType);
                pars[1] = new SqlParameter("@_Type", topupType);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.DateTime) { Direction = ParameterDirection.Output };

                _dbHelper.SetConnectionString(appSettings.BillingDatabaseAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("ScheduleMaintainGetMaintain", pars);

                string res = !DBNull.Value.Equals(pars[2].Value) ? Convert.ToString(pars[2].Value) : null;
                return res;
            }
            catch(Exception exception)
            {
                NLogManager.Exception(exception);
                return null;
            }
           
        }
        public UserRevenueAffilicateModel GetUserRevenueAffilicate(long accountID, int fromDate, int toDate)
        {
            try
            {

                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_FromDate", fromDate);
                pars[2] = new SqlParameter("@_ToDate", toDate) ;

                _dbHelper.SetConnectionString(appSettings.ReportConnectionString);
                var res = _dbHelper.GetListSP<UserRevenueAffilicateModel>("SP_Report_GetUserRevenueAffilicate", pars);
                if(res !=null && res.Count>0)
                    return res[0]; 
            }
            catch (Exception exception)
            {
                NLogManager.Exception(exception);
                
            }
            return null;
        }
        public List<TransactionLog> GetTransactionLog(long accountID, int limit)
        {
            if (limit > 100)
                return null;
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_LimitTop", limit);

                _dbHelper.SetConnectionString(appSettings.ReportConnectionString);
                var res = _dbHelper.GetListSP<TransactionLog>("SP_Account_GetTransactionLogs", pars);
                if (res != null && res.Count > 0)
                    return res;
            }
            catch (Exception exception)
            {
                NLogManager.Exception(exception);

            }
            return null;
        }
		public InfoRevenueAffiliate GetInfoRevenueAffiliate(long accountID, int fromDate, int toDate)
		{
			try
			{
				var pars = new SqlParameter[3];
				pars[0] = new SqlParameter("@_AccountID", accountID);
				pars[1] = new SqlParameter("@_FromDateInt", fromDate);
				pars[2] = new SqlParameter("@_ToDateInt", toDate);

				_dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
				var res = _dbHelper.GetListSP<InfoRevenueAffiliate>("SP_Affilicate_GetInfoRevenue", pars);
				if (res != null && res.Count > 0)
					return res[0];
			}
			catch (Exception exception)
			{
				NLogManager.Exception(exception);

			}
			return null;
		}
		public List<HistoryDeductRevenue> GetHistoryDeductRevenue(long accountID)
		{
			try
			{
				var pars = new SqlParameter[1];
				pars[0] = new SqlParameter("@_AccountID", accountID);

				_dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
				var res = _dbHelper.GetListSP<HistoryDeductRevenue>("SP_Affilicate_GetHistoryDeductRevenue", pars);
				if (res != null && res.Count > 0)
					return res;
                return new List<HistoryDeductRevenue> { new HistoryDeductRevenue() { Amount=20000,CreatedAt=DateTime.Now,Description="Test lịch sử rút tiền", TransCode=1} };
			}
			catch (Exception exception)
			{
				NLogManager.Exception(exception);

			}
			return null;
		}
		public int WithdrawAffiliate(long accountID, string nickName, long withdrawValue,string ipAddress, int sourceId, out long balanceNew)
		{
            balanceNew = 0;
			int response = -1;
			try
			{
				var pars = new SqlParameter[7];
				pars[0] = new SqlParameter("@_AccountID", accountID);
				pars[1] = new SqlParameter("@_NickName", nickName);
				pars[2] = new SqlParameter("@_TranferValue", withdrawValue);
				pars[3] = new SqlParameter("@_ClientIP", ipAddress);
				pars[4] = new SqlParameter("@_SourceID", sourceId);
				pars[5] = new SqlParameter("@_UserBalanceNew", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
				pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
				_dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_Affiliate_WithdrawTransactionTopup", pars);
				response = (int)pars[6].Value;
				balanceNew = (long)pars[5].Value;
			}
			catch (Exception exception)
			{
				NLogManager.Exception(exception);

			}
			return response;
		}
	}
}
