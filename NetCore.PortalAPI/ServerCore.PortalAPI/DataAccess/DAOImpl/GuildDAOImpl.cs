using ServerCore.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ServerCore.Utilities.Utils;
using ServerCore.Utilities.Models;
using Microsoft.Extensions.Options;
using System.Data;
using NetCore.Utils.Interfaces;

namespace ServerCore.DataAccess.DAOImpl
{
    public class GuildDAOImpl : IGuildDAO
    {
        private readonly AppSettings appSettings;       
        private static long currentEnd = DateTime.Now.Ticks;
        private static long currentDaiLy = DateTime.Now.Ticks;
        private readonly IDBHelper _dbHelper;

        public GuildDAOImpl(IDBHelper dbHelper, IOptions<AppSettings> options)
        {
            appSettings = options.Value;
            _dbHelper = dbHelper;
        }
    
        public long GetAgencyIDByMember(long accountId)
        {
            int res = -1;
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_GUILD_GetInfoMember", pars);
                return Convert.ToInt32(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return res;
        }

        public List<GameAgency> GetGameAgencies()
        {
            long currentTime = DateTime.Now.Ticks;
            long duration = currentTime - currentEnd;
            try
            {
                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                List<GameAgency> listAgency = _dbHelper.GetListSP<GameAgency>("SP_GUILD_AgencyGetList");
                currentEnd = DateTime.Now.Ticks;
                return listAgency;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return new List<GameAgency>();
        }

        public int RegisterAgency(string userName, string agencyName, string phone)
        {
            int res = -1;
            try
            {
                string sqlSearchAccountId = string.Format("SELECT [AccountID] FROM [Accounts] where UserName = '{0}'", userName);
                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                int accountId = _dbHelper.ExecuteNonQuery(sqlSearchAccountId);
                if(accountId > 0)
                {
                    int agencyId = _dbHelper.ExecuteNonQuery(string.Format("SELECT [AgencyID] FROM [Agency] where UserName = '{0}'", userName));
                    if(agencyId > 0)
                        return -2;

                    string sqlCmd = string.Format("INSERT INTO [Agency]([AgencyID],[Username],[AgencyName],[AgencyMobile],[Status],[CreateDate],[LastChanged])" +
                                            "VALUES({0},'{1}','{2}','{3}',{4},GETDATE(),GETDATE()", accountId, userName, agencyName, phone, 1);

                    res = _dbHelper.ExecuteNonQuery(sqlCmd);
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return res;
        }

        public int RequestJoinAgency(int agencyAccountId, long accountId, string userName, string nickName)
        {
            int res = -1;
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AgencyID", agencyAccountId);
                pars[1] = new SqlParameter("@_AccountID", accountId);
                pars[2] = new SqlParameter("@_UserName", userName);
                pars[3] = new SqlParameter("@_NickName", nickName);
                pars[4] = new SqlParameter("@_ResponStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_GUILD_AccountRequestJoinAgency", pars);
                return Convert.ToInt32(pars[4].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return res;
        }

        public int AcceptRequestInfo(long agencyAccountId, string nickName, int status, int isAgency)
        {
            int res = -1;
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountIDAgency", agencyAccountId);
                pars[1] = new SqlParameter("@_NickName", nickName);
                pars[2] = new SqlParameter("@_Status", status);
                pars[3] = new SqlParameter("@_Agency", isAgency);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_GUILD_GuildMemberUpdate", pars);
                return Convert.ToInt32(pars[4].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return res;
        }

        public int UpdateRule(long agencyAccountId, string content)
        {
            int res = -1;
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", agencyAccountId);
                pars[1] = new SqlParameter("@_Content", content);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_GUILD_UpdateRule", pars);
                return Convert.ToInt32(pars[2].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return res;
        }

        public List<GameMember> GetListMember(long agencyAccountId, int status)
        {
            List<GameMember> list = null;
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AgencyAccountId", agencyAccountId);
                pars[1] = new SqlParameter("@_Status", status);
                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                list = _dbHelper.GetListSP<GameMember>("SP_GUILD_MemberGetList", pars);
                return list;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return new List<GameMember>();
        }

        public List<GameMember> GetListMemberOther(long agencyAccountId, int status)
        {
            List<GameMember> list = null;
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AgencyAccountId", agencyAccountId);
                pars[1] = new SqlParameter("@_Status", status);
                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                list = _dbHelper.GetListSP<GameMember>("SP_GUILD_MemberGetList_Other", pars);
                return list;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return new List<GameMember>();
        }

        public List<GameJoinRequest> GetListRequestJoinAgency(long accountId, int status)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_Status", status);
                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                List<GameJoinRequest> lsRequest = _dbHelper.GetListSP<GameJoinRequest>("SP_GUILD_JoinRequestInfo", pars);
                return lsRequest;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return null;
        }

        public GameAgency GetGameAgency(int agencyAccountId)
        {
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AgencyAccountID", agencyAccountId);
                _dbHelper.SetConnectionString(appSettings.BillingGuildAPIConnectionString);
                List<GameAgency> lsGa = _dbHelper.GetListSP<GameAgency>("SP_GUILD_AgencyDetail", pars);
                if(lsGa == null || lsGa.Count <=0)
                    return null;
                return lsGa[0];
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return null;
        }
    }
}
