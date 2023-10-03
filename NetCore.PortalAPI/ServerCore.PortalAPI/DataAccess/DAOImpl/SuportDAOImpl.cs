using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;
using System.Data.SqlClient;
using System.Data;
using ServerCore.Utilities.Database;
using ServerCore.DataAccess.DTO;
using Microsoft.Extensions.Options;
using ServerCore.PortalAPI.Models;

namespace ServerCore.DataAccess.DAOImpl {
    public class SuportDAOImpl : ISuportDAO
    {
        private readonly AppSettings appSettings;
        public SuportDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
        }

        public List<TopupHistory> GetTopupHistory(string permitKey, string userName, string dateFrom, string dateTo, string nickName)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper();
                db.SetConnectionString(appSettings.GameLogConnectionString);

                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_UserName", userName);
                pars[1] = new SqlParameter("@_DateFrom", dateFrom);
                pars[2] = new SqlParameter("@_DateTo", dateTo);
                pars[3] = new SqlParameter("@_NickName", nickName);
                pars[4] = new SqlParameter("@_PermitKey", permitKey);
                
                var res = db.GetListSP<TopupHistory>("SP_GetTopupHistory", pars);

                return res == null ? new List<TopupHistory>() : res;
            }
            catch(Exception exception)
            {
                NLogManager.Exception(exception);
                return new List<TopupHistory>();
            }
            finally
            {
                if(db != null)
                {
                    db.Close();
                }
            }
        }

        public List<UserTransactionHistory> GetUserTransactionHistory(string permitKey, string userName, string dateFrom, string dateTo, string nickName, int gameType)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper();
                db.SetConnectionString(appSettings.GameLogConnectionString);
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_NickName", nickName);
                pars[1] = new SqlParameter("@_UserName", userName);
                pars[2] = new SqlParameter("@_GameType", gameType);
                pars[3] = new SqlParameter("@_PermitKey", permitKey);
                pars[4] = new SqlParameter("@_DateFrom", dateFrom);
                pars[5] = new SqlParameter("@_DateTo", dateTo);

                var res = db.GetListSP<UserTransactionHistory>("SP_GetUserHistoryTransaction", pars);
                return res == null ? new List<UserTransactionHistory>() : res;

                //List<UserTransactionHistory> list = null;
                //string sqlCmd = string.Format("SELECT M.[CreatedTime] as CreatedTime " +
                //      " ,N.[Description] as ServiceName " +
                //      " ,-[Amount] as Amount " +
                //      " ,M.[Description] as Descriptions " +
                //      " ,M.ReferenceID as TransactionID " +
                //  " FROM [ServerCore.Database].[dbo].[OutputTransactions] M " +
                //    ", [PTCNGame.CardGameDB].dbo.[GameRoomTypes] N " +
                //  "WHERE N.BetServiceID = M.ServiceID " +
                //  "AND [Username] = '{0}' " + 
                //  "AND M.[CreatedTime] between '{1}' AND '{2}' " +
                //  "AND N.GameTypeID = {3} " + 
                //  "AND N.Currency = 1 " +
                //  "ORDER BY OutputTransactionID DESC", userName, dateFrom, dateTo, gameType);

                //list = db.GetList<UserTransactionHistory>(sqlCmd);//db.GetListSP<Agency>("SP_Account_GetAgencies");
                //return list;
            }
            catch(Exception exception)
            {
                NLogManager.Exception(exception);
                return new List<UserTransactionHistory>();
            }
            finally
            {
                if(db != null)
                {
                    db.Close();
                }
            }
        }
    }
}
