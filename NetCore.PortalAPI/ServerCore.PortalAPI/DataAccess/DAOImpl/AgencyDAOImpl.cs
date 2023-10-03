using ServerCore.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ServerCore.Utilities.Utils;
using ServerCore.Utilities.Models;
using Microsoft.Extensions.Options;
using System.Data;
using NetCore.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ServerCore.DataAccess.DAOImpl
{
    public class AgencyDAOImpl : IAgencyDAO
    {
        private readonly AppSettings appSettings;       
        private static long currentEnd = DateTime.Now.Ticks;
        private static long currentDaiLy = DateTime.Now.Ticks;
        private readonly IDBHelper _dbHelper;

        public AgencyDAOImpl(IDBHelper dbHelper, IOptions<AppSettings> options)
        {
            appSettings = options.Value;
            _dbHelper = dbHelper;
        }
        //  LẤY THÔNG TIN ĐẠI LÝ
        public int CheckIsAgency(int accountID)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_Agency_CheckExist", pars);


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

                _dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_Agency_GetInfoAgency", pars);

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
                NLogManager.Info(string.Format("SP_AgencyGetList1: _Level = {0}, _Status = {1}, _AgencyParentID = {2} ", level, status, agencyParentID));
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_Level", level);
                pars[1] = new SqlParameter("@_Status", status);
                pars[2] = new SqlParameter("@_AgencyParentID", agencyParentID);
                pars[3] = new SqlParameter("@_AgencyName", "");
                _dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
                List<Agency>  listDaiLy = _dbHelper.GetListSP<Agency>("SP_AgencyGetList", pars);
               // currentDaiLy = currentTime;
                return listDaiLy;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return new List<Agency>();
        }
        /// <summary>
        /// Lấy danh sách đại lý theo locationId
        /// </summary>
        /// <param name="level"> 0: lấy tất, 1: lấy đại lý cấp 1, 2: cấp 2</param>
        /// <param name="status"> 2: lấy tất, 1: lấy đại lý đang hoạt động, 0: lấy đại lý không hoạt động</param>
        /// <param name="agencyParentID"></param>
        /// <returns> Danh sách đại lý</returns>
        public List<Agency> GetAgenciesClient(int locationId, int level = -1, int status = 1, int agencyParentID = 0)
        {
            //List<Agency> list = null;
            //long currentTime = DateTime.Now.Ticks;
            //long duration = currentTime - currentDaiLy;
            //if (duration < 9000000000 && listDaiLy != null && listDaiLy.Count > 0)
            //    return listDaiLy;
            try
            {
                NLogManager.Info(string.Format("SP_AgencyGetList_2: _Level = {0}, _Status = {1}, _AgencyParentID = {2} ", level, status, agencyParentID));
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_Level", level);
                pars[1] = new SqlParameter("@_Status", status);
                pars[2] = new SqlParameter("@_AgencyParentID", agencyParentID);
                pars[3] = new SqlParameter("@_AgencyName", "");
                pars[4] = new SqlParameter("@_LocationID", locationId);
                _dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
                List<Agency> listDaiLy = _dbHelper.GetListSP<Agency>("SP_AgencyGetList_2", pars);
                // currentDaiLy = currentTime;
                return listDaiLy;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return new List<Agency>();
        }

        public List<AgencyBank> GetAgenciesBank()
        {
            //List<Agency> list = null;
            //long currentTime = DateTime.Now.Ticks;
            //long duration = currentTime - currentDaiLy;
            //if (duration < 9000000000 && listDaiLy != null && listDaiLy.Count > 0)
            //    return listDaiLy;
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AgencyID", 1);
                _dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
                List<AgencyBank> listDaiLy = _dbHelper.GetListSP<AgencyBank>("GetAgencyBank", pars);
                return listDaiLy;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return null;
        }


        public double GetTransferRate(int type, int levelId)
        {
            double rate = 0;
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_Type", type);
                pars[1] = new SqlParameter("@_LevelID", levelId);
                pars[2] = new SqlParameter("@_Rate", SqlDbType.Float) { Direction = ParameterDirection.Output };
                _dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_Agency_GetRate", pars);
                rate = (double)pars[2].Value;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return rate;
        }

        public double GetTransferRateNew(string nickNameTran, string nickNameRev)
        {
            double rate = 0;
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_NickNameTran", nickNameTran);
                pars[1] = new SqlParameter("@_NickNameRecv", nickNameRev);
                pars[2] = new SqlParameter("@_Rate", SqlDbType.Float) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_ErrorCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelper.SetConnectionString(appSettings.BillingAgencyAPIConnectionString);
                _dbHelper.ExecuteNonQuerySP("SP_Agency_GetRate_New", pars);
                int errorCode = Int32.Parse(pars[3].Value.ToString());
                if (errorCode >= 0)
                    rate = (double)pars[2].Value;
            }
            catch (Exception ex)
            {
                //NLogManager.Exception(ex);

            }

            return rate;
        }
    }
}
