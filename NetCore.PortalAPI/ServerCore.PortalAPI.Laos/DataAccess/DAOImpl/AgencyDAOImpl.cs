using ServerCore.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using ServerCore.Utilities.Utils;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Database;
using ServerCore.DataAccess.DTO;
using Microsoft.Extensions.Options;
using ServerCore.PortalAPI.Models;
using System.Data;
using Netcore.Gateway.Interfaces;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
namespace ServerCore.DataAccess.DAOImpl
{
    public class AgencyDAOImpl : IAgencyDAO
    {
        private readonly AppSettings appSettings;
       // private static List<GameAgency> listAgency = null;
       
        private static long currentEnd = DateTime.Now.Ticks;
        private static long currentDaiLy = DateTime.Now.Ticks;
        public AgencyDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
          
        }
        //  LẤY THÔNG TIN ĐẠI LÝ
        public int CheckIsAgency(int accountID)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
              
                SQLAccess.getAgency().ExecuteNonQuerySP("SP_Agency_CheckExist", pars);


               int ResponseStatus = Convert.ToInt32(pars[1].Value);
                if (ResponseStatus >= 0)
                    return 1;
               return 0;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return 0;
            }
        }
        //  LẤY THÔNG TIN ĐẠI LÝ
        public AgenciesInfo AgencyGetInfo(string NickName)
        {
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_NickName", NickName);
                pars[1] = new SqlParameter("@_AgencyID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_AgencyParentID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_AgencyName", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_LevelID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ErrorCode", SqlDbType.Int) { Direction = ParameterDirection.Output };

                SQLAccess.getAgency().ExecuteNonQuerySP("SP_Agency_GetInfoAgency", pars);

                AgenciesInfo _agencies = new AgenciesInfo();

                if (string.IsNullOrEmpty(pars[1].Value.ToString()))
                {
                    _agencies.AgencyID = -1;
                }
                else
                {
                    _agencies.AgencyID = int.Parse(pars[1].Value.ToString());
                }

                if (string.IsNullOrEmpty(pars[4].Value.ToString()))
                {
                    _agencies.LevelID = -1;
                }
                else
                {
                    _agencies.LevelID = int.Parse(pars[4].Value.ToString());
                }

                if (string.IsNullOrEmpty(pars[5].Value.ToString()))
                {
                    _agencies.AccountID = -1;
                }
                else
                {
                    _agencies.AccountID = int.Parse(pars[5].Value.ToString());
                }

                return _agencies;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
                return new AgenciesInfo();
            }
        }
        /// <summary>
        /// Lấy danh sách đại lý
        /// </summary>
        /// <param name="level"> 0: lấy tất, 1: lấy đại lý cấp 1, 2: cấp 2</param>
        /// <param name="status"> 2: lấy tất, 1: lấy đại lý đang hoạt động, 0: lấy đại lý không hoạt động</param>
        /// <param name="agencyParentID"></param>
        /// <returns> Danh sách đại lý</returns>
        public List<Agency> GetAgencies(int level = 1, int status = 1, int agencyParentID = 0)
        {
            //List<Agency> list = null;
            //long currentTime = DateTime.Now.Ticks;
            //long duration = currentTime - currentDaiLy;
            //if (duration < 9000000000 && listDaiLy != null && listDaiLy.Count > 0)
            //    return listDaiLy;
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_Level", level);
                pars[1] = new SqlParameter("@_Status", status);
                pars[2] = new SqlParameter("@_AgencyParentID", agencyParentID);
                pars[3] = new SqlParameter("@_AgencyName", "");
                NLogManager.Info("SP_AgencyGetList" + JsonConvert.SerializeObject(pars));
                List<Agency>  listDaiLy = SQLAccess.getAgency().GetListSP<Agency>("SP_AgencyGetList", pars);
               // currentDaiLy = currentTime;
                return listDaiLy;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return new List<Agency>();
        }
        public List<Agency> GetAgenciesByAccountID(long accountid)
        {
            //List<Agency> list = null;
            //long currentTime = DateTime.Now.Ticks;
            //long duration = currentTime - currentDaiLy;
            //if (duration < 9000000000 && listDaiLy != null && listDaiLy.Count > 0)
            //    return listDaiLy;
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                List<Agency> listDaiLy = SQLAccess.getAgency().GetListSP<Agency>("SP_AgencyByAccountID", pars);
                return listDaiLy;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return new List<Agency>();
        }

       
        public long GetAgencyIDByMember(long accountId)
        {
            int res = -1;
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                SQLAccess.getGuild().ExecuteNonQuerySP("SP_GUILD_GetInfoMember", pars);
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
                List<GameAgency> listAgency = SQLAccess.getGuild().GetListSP<GameAgency>("SP_GUILD_AgencyGetList");
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
                int accountId = SQLAccess.getAuthen().ExecuteNonQuery(sqlSearchAccountId);
                if(accountId > 0)
                {
                    int agencyId = SQLAccess.getAuthen().ExecuteNonQuery(string.Format("SELECT [AgencyID] FROM [Agency] where UserName = '{0}'", userName));
                    if(agencyId > 0)
                        return -2;

                    string sqlCmd = string.Format("INSERT INTO [Agency]([AgencyID],[Username],[AgencyName],[AgencyMobile],[Status],[CreateDate],[LastChanged])" +
                                            "VALUES({0},'{1}','{2}','{3}',{4},GETDATE(),GETDATE()", accountId, userName, agencyName, phone, 1);

                    res = SQLAccess.getAuthen().ExecuteNonQuery(sqlCmd);
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

                SQLAccess.getGuild().ExecuteNonQuerySP("SP_GUILD_AccountRequestJoinAgency", pars);
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

                SQLAccess.getGuild().ExecuteNonQuerySP("SP_GUILD_GuildMemberUpdate", pars);
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

                SQLAccess.getGuild().ExecuteNonQuerySP("SP_GUILD_UpdateRule", pars);
                return Convert.ToInt32(pars[2].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return res;
        }

        public int EnableAgency(int accountId, int enable)
        {
            int res = -1;
            try
            {
                string sqlSearchAccountId = string.Format("SELECT [AgencyID] FROM [Agency] where AgencyID = {0}", accountId);
                res = SQLAccess.getAuthen().ExecuteNonQuery(sqlSearchAccountId);
                if(res > 0)
                {
                    string sqlCmd = string.Format("UPDATE [Agency] SET [Status] = {0}, [LastChanged] = GETDATE() WHERE AgencyID = {1}", enable, accountId);
                    res = SQLAccess.getAuthen().ExecuteNonQuery(sqlCmd);
                }
            }
            catch(Exception ex)
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
                list = SQLAccess.getGuild().GetListSP<GameMember>("SP_GUILD_MemberGetList", pars);
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
                list = SQLAccess.getGuild().GetListSP<GameMember>("SP_GUILD_MemberGetList_Other", pars);
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
                List<GameJoinRequest> lsRequest = SQLAccess.getGuild().GetListSP<GameJoinRequest>("SP_GUILD_JoinRequestInfo", pars);
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
                List<GameAgency> lsGa = SQLAccess.getGuild().GetListSP<GameAgency>("SP_GUILD_AgencyDetail", pars);
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
