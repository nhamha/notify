using Microsoft.Extensions.Options;
using Netcore.Gateway.Interfaces;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess.DTO;
using ServerCore.PortalAPI.Models;
using ServerCore.Utilities.Database;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DAOImpl
{
    public class ReportDAOImpl : IReportDAO
    {
        private readonly AppSettings appSettings;

        public ReportDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
        }

        public string CheckCardMaintain(string cardType, int topupType)
        {
         //   PROCEDURE[dbo].[ScheduleMaintainGetMaintain]
         //   @_Provider NVARCHAR(50)-- VTT, VNP, VMS, GATE
         //   ,@_Type   INT--0: mua thẻ, 3: nạp thẻ cào
	     //   ,@_ResponseStatus INT OUT-- >= 1 :Đang bảo trì, NULL: Không bảo trì

            DBHelper db = null;
            try
            {

                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_Provider", cardType);
                pars[1] = new SqlParameter("@_Type", topupType);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.DateTime) { Direction = ParameterDirection.Output };

                SQLAccess.getBilling().ExecuteNonQuerySP("ScheduleMaintainGetMaintain", pars);

                string res = !DBNull.Value.Equals(pars[2].Value) ? Convert.ToString(pars[2].Value) : null;
                return res;
            }
            catch(Exception exception)
            {
                NLogManager.Exception(exception);
                return null;
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
