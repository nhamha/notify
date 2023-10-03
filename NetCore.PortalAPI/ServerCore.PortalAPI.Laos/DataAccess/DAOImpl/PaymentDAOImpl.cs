using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Options;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess.DTO;
using ServerCore.PortalAPI.Models;
using ServerCore.Utilities.Utils;

namespace ServerCore.DataAccess.DAOImpl
{
    public class PaymentDAOImpl : IPaymentDAO
    {
        private readonly AppSettings appSettings;
        public PaymentDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
        }
        /// <summary>
        /// // nạp Vcoin qua thẻ cào Vcoin
        /// cardInfo.Error: 
        /// >=0: nạp thành công
        /// -1: Thẻ đã dùng rồi
        /// -2: thẻ đã bị khóa
        /// -3: thẻ quá hạn
        /// -10: seri ko tồn tại
        /// -11: mã thẻ ko đúng
        /// -12: Thẻ ko tồn tại
        /// </summary>
        /// <param name="vcoin"></param>
        /// <param name="gift"></param>
        /// <param name="totalVcoin"></param>
        /// <param name="cardInfo"></param>
        [Obsolete("Old version of billing Vcoin - use method TopupStarByVTCCard", false)]
        public int TopupVcoinByVTCCard(ref int vcoin, ref int gift, ref int totalVcoin, VTOCard cardInfo)
        {
            NLogManager.Info(string.Format("TopupVcoinByVTCCard:{0}_CardID: {1}_oCardCode:{2}_CardCode:{3}", cardInfo.AccountName, cardInfo.CardID, cardInfo.OriginalCode, cardInfo.CardCode));
            return -1;
            //var pars = new SqlParameter[13];
            //pars[0] = new SqlParameter("@_ServiceID", cardInfo.ServiceID);
            //pars[1] = new SqlParameter("@_ServiceKey", cardInfo.ServiceKey);
            //pars[2] = new SqlParameter("@_OrderID", SqlDbType.Int) { Value = 0 };
            //pars[3] = new SqlParameter("@_AccountName", cardInfo.AccountName);
            //pars[4] = new SqlParameter("@_CardID", cardInfo.CardID);
            //pars[5] = new SqlParameter("@_oCardCode", cardInfo.OriginalCode);
            //pars[6] = new SqlParameter("@_CardCode", cardInfo.CardCode);
            //pars[7] = new SqlParameter("@_AccountIP", cardInfo.AccountIP);
            //pars[8] = new SqlParameter("@_CardValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[9] = new SqlParameter("@_Vcoin", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[10] = new SqlParameter("@_Gift", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[11] = new SqlParameter("@_TotalVCoin", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[12] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

            //try
            //{
            //    var db = new DBHelper(AppSettings.IntecomApiConnectionString);
            //    if (cardInfo.Prefix.Equals("PM", StringComparison.OrdinalIgnoreCase))
            //    {
            //        db.ExecuteNonQuerySP("SP_Account_InputCard", pars);
            //    }
            //    else if (cardInfo.Prefix.Equals("ID", StringComparison.OrdinalIgnoreCase))
            //    {
            //        db.ExecuteNonQuerySP("SP_Account_InputCard_ID", pars);
            //    }
            //    else throw new Exception("Invalid card prefix");

            //    var result = (int)pars[12].Value;
            //    if (result == 0)
            //    {
            //        cardInfo.CardValue = (int)pars[8].Value;
            //        vcoin = (int)pars[9].Value;
            //        gift = (int)pars[10].Value;
            //        totalVcoin = (int)pars[11].Value;
            //    }
            //    NLogManager.Info(string.Format("TopupVcoinByVTCCard:{0}_CardID: {1}_oCardCode:{2}_CardCode:{3}_Result:{4}", cardInfo.AccountName, cardInfo.CardID, cardInfo.OriginalCode, cardInfo.CardCode, result));

            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Exception(ex);
            //    return -99;
            //}
        }

        /// <summary>
        /// // nạp SAO qua thẻ cào Vcoin
        /// cardInfo.Error: 
        /// >=0: nạp thành công
        /// -1: Thẻ đã dùng rồi
        /// -2: thẻ đã bị khóa
        /// -3: thẻ quá hạn
        /// -10: seri ko tồn tại
        /// -11: mã thẻ ko đúng
        /// -12: Thẻ ko tồn tại
        /// </summary>
        /// <param name="vcoin"></param>
        /// <param name="gift"></param>
        /// <param name="totalVcoin"></param>
        /// <param name="cardInfo"></param>
        public int TopupStarByVTCCard(ref int vcoin, ref int gift, ref int totalVcoin, ref long totalStar, VTOCard cardInfo)
        {
            NLogManager.Info(string.Format("TopupStarByVTCCard:{0}_CardID: {1}_oCardCode:{2}_CardCode:{3}", cardInfo.AccountName, cardInfo.CardID, cardInfo.OriginalCode, cardInfo.CardCode));
            return -1;
            //var pars = new SqlParameter[15];
            //pars[0] = new SqlParameter("@_ServiceID", cardInfo.ServiceID);
            //pars[1] = new SqlParameter("@_ServiceKey", cardInfo.ServiceKey);
            //pars[2] = new SqlParameter("@_OrderID", SqlDbType.Int) { Value = 0 };
            //pars[3] = new SqlParameter("@_AccountName", cardInfo.AccountName);
            //pars[4] = new SqlParameter("@_CardID", cardInfo.CardID);
            //pars[5] = new SqlParameter("@_oCardCode", cardInfo.OriginalCode);
            //pars[6] = new SqlParameter("@_CardCode", cardInfo.CardCode);
            //pars[7] = new SqlParameter("@_AccountIP", cardInfo.AccountIP);
            //pars[8] = new SqlParameter("@_CardValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[9] = new SqlParameter("@_Vcoin", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[10] = new SqlParameter("@_Gift", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[11] = new SqlParameter("@_TotalVCoin", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[12] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //pars[13] = new SqlParameter("@_IsTopupStar", true);
            //pars[14] = new SqlParameter("@_GameBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//OUTPUT -- Số dư Sao (nếu nạp Sao)

            //try
            //{
            //    var db = new DBHelper(AppSettings.IntecomApiConnectionString);
            //    if (cardInfo.Prefix.Equals("PM", StringComparison.OrdinalIgnoreCase))
            //    {
            //        db.ExecuteNonQuerySP("SP_Account_InputCard", pars);
            //    }
            //    else if (cardInfo.Prefix.Equals("ID", StringComparison.OrdinalIgnoreCase))
            //    {
            //        db.ExecuteNonQuerySP("SP_Account_InputCard_ID", pars);
            //    }
            //    else throw new Exception("Invalid card prefix");

            //    var result = (int)pars[12].Value;
            //    if (result == 0)
            //    {
            //        cardInfo.CardValue = (int)pars[8].Value;
            //        vcoin = (int)pars[9].Value;
            //        gift = (int)pars[10].Value;
            //        totalVcoin = (int)pars[11].Value;
            //        totalStar = (long)pars[14].Value;
            //    }
            //    NLogManager.Info(string.Format("TopupStarByVTCCard:{0}_CardID: {1}_oCardCode:{2}_CardCode:{3}_Result:{4}", cardInfo.AccountName, cardInfo.CardID, cardInfo.OriginalCode, cardInfo.CardCode, result));

            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Exception(ex);
            //    return -99;
            //}
        }
    }
}