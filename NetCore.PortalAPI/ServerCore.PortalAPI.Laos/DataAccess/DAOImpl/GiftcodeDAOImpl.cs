using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;
using ServerCore.DataAccess.DTO;
using System.Collections.Generic;
using ServerCore.Utilities.Database;
using Microsoft.Extensions.Options;
using ServerCore.PortalAPI.Models;
using Netcore.Gateway.Interfaces;

namespace ServerCore.DataAccess.DAOImpl
{
    public class GiftcodeDAOImpl : IGiftcodeDAO
    {
        private readonly AppSettings appSettings;
        public GiftcodeDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
        }

        public int Giftcode(int accountId, string username, string giftcode, string clientIP, string mobile, ref int value)
        {
            int ResponseStatus = -1000;
            try
            {
                var param = new SqlParameter[7];
                param[0] = new SqlParameter("@_AccountID", accountId);
                param[1] = new SqlParameter("@_Username", username);
                param[2] = new SqlParameter("@_GiftCode", giftcode);
                param[3] = new SqlParameter("@_ClientIP", clientIP);
                param[4] = new SqlParameter("@_Mobile", DBNull.Value);
                param[5] = new SqlParameter("@_Value", SqlDbType.Int) { Direction = ParameterDirection.Output };
                param[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };


                SQLAccess.getGifcode().ExecuteNonQuerySP("SP_GiftCodes_Used", param);

                value = Convert.ToInt32(param[5].Value);
                ResponseStatus = Convert.ToInt32(param[6].Value);
            }
            catch (Exception ex)
            {
                //NLogLogger.Exception(ex);
                NLogManager.Exception(ex);
            }

            return ResponseStatus;
        }

    }
}
