using Microsoft.Extensions.Options;
using NetCore.Utils.Interfaces;
using ServerCore.DataAccess.DTO;
using ServerCore.Utilities.Utils;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System;
using ServerCore.PortalAPI.Models;
using ServerCore.PortalAPI.DataAccess.DAO;

namespace ServerCore.PortalAPI.DataAccess.DAOImpl
{
	public class LoyaltyDAOImpl:ILoyaltyDAO
	{
		private readonly AppSettings appSettings;
		private readonly IDBHelper _dbHepler;

		public LoyaltyDAOImpl(IDBHelper dbHepler, IOptions<AppSettings> options)
		{
			appSettings = options.Value;
			_dbHepler = dbHepler;
		}
		public List<VpLevel> GetVpLevel(int? type)
		{
			try
			{
				var pars = new SqlParameter[1];
				pars[0] = new SqlParameter("@_Type", type);
				_dbHepler.SetConnectionString(appSettings.LoyaltyConnectionString);
				var list = _dbHepler.GetListSP<VpLevel>("SP_FindAllVpLevel", pars);
				
				return list;
			}
			catch (Exception ex)
			{
				NLogManager.Exception(ex);
				return new List<VpLevel>();
			}
		}
		public VipPointByAccountId FindVipPointInfo(long accountID)
		{
			try
			{
				var pars = new SqlParameter[3];
				pars[0] = new SqlParameter("@_AccountID", accountID);
				pars[1] = new SqlParameter("@_TimeDay", DateTime.Now.Date);
				pars[2] = new SqlParameter("@_TimeMonth", new DateTime(DateTime.Now.Year,DateTime.Now.Month,1));
				_dbHepler.SetConnectionString(appSettings.LoyaltyConnectionString);
				var list = _dbHepler.GetListSP<VipPointByAccountId>("SP_FindVipPointByAccountId", pars);
				if (list != null&& list.Count>1)
					return list[0];
				return new VipPointByAccountId()
				{
					levelDay = 1,
					levelMonth = 2,
					totalJDay = 4000,
					totalJMonth = 40000,
					vpDay = 10,
					vpMonth = 100
				};
			}
			catch (Exception ex)
			{
				NLogManager.Exception(ex);
				return null;
			}
		}
		public List<AccountVpLevelExt> FindTopAccountVpTransaction(long accountID, int top)
		{
			try
			{
				var pars = new SqlParameter[2];
				pars[0] = new SqlParameter("@_AccountID", accountID);
				pars[1] = new SqlParameter("@_Top", top);
				_dbHepler.SetConnectionString(appSettings.LoyaltyConnectionString);
				var list = _dbHepler.GetListSP<AccountVpLevelExt>("SP_TopRewardByAccountId", pars);
				if (list != null && list.Count > 0)
					return list;
				return new List<AccountVpLevelExt>
				{
					new AccountVpLevelExt() {datetime = DateTime.Now,vpLevelName="Hạng Sắt",accountVp=3,totalAmount=1200,vpType=0},
					new AccountVpLevelExt() {datetime = DateTime.Now,vpLevelName="Hạng Sắt",accountVp=4,totalAmount=1600,vpType=0}
				};
			}
			catch (Exception ex)
			{
				NLogManager.Exception(ex);
				return null;
			}
		}
		public List<AccountVpLevelInfo> FindTopRankVipPointLevelInfo(long accountID, int top, DateTime date, int type)
		{
			try
			{
				var pars = new SqlParameter[4];
				pars[0] = new SqlParameter("@_AccountID", accountID);
				pars[1] = new SqlParameter("@_Top", top);
				pars[2] = new SqlParameter("@_LevelTime", top);
				pars[3] = new SqlParameter("@_Type", top);
				_dbHepler.SetConnectionString(appSettings.LoyaltyConnectionString);
				var list = _dbHepler.GetListSP<AccountVpLevelInfo>("SP_FindTopRankVipPointLevelInfo", pars);
				if (list != null && list.Count > 0)
					return list;
				return new List<AccountVpLevelInfo>
				{
					new AccountVpLevelInfo(){accountId=accountID,username="Tesst111",userFullName="tesst",accountVp=3,level=1,levelName="Hạng Sắt",vpExchangeValue=400,totalAmount=1200},
					new AccountVpLevelInfo(){accountId=accountID,username="Tesst111",userFullName="tesst",accountVp=3,level=1,levelName="Hạng Sắt",vpExchangeValue=400,totalAmount=1200}
				};
			}
			catch (Exception ex)
			{
				NLogManager.Exception(ex);
				return null;
			}
		}
		//
	}
}
