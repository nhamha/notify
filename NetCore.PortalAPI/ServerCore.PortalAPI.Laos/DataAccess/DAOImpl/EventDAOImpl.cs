using Microsoft.Extensions.Options;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess.DTO;
using ServerCore.Utilities.Database;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ServerCore.PortalAPI.Models;

namespace ServerCore.DataAccess.DAOImpl
{
    public class EventDAOImpl : IEventDAO
    {
        private readonly AppSettings appSettings;
        private readonly DBHelper _dbHelperGifcode, _dbHelperEvent;

        public EventDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
        }

        public int CheckGiftCode(long accountId, string accountName, string nickName, string giftcode, int merchantId, string merchantKey, int sourceId, string ip, out long balance)
        {
            //  PROCEDURE[dbo].[SP_GiftCodeData_Charging]
            //  @_AccountId BIGINT,
            //  @_AccountName NVARCHAR(50),
            //  @_NickName NVARCHAR(100),
            //  @_Code NVARCHAR(50),
            //  @_MerchantId INT = 0,
            //  @_MerchantKey NVARCHAR(50),
            //  @_SourceID INT, --1: android, 2 WP, 3 IOS, 4 WEB
            //  @_ClientIP NVARCHAR(30),
            //  @_Balance BIGINT OUTPUT,
            //  @_ResponseStatus BIGINT OUTPUT

            //DECLARE @_ERROR_NOT_EXIST_MERCHANTID INT = -90
            //DECLARE @_ERROR_GIFCODE_USING INT = -100
            //DECLARE @_ERROR_GIFCODE_NOT_EXIST INT = -101
            //DECLARE @_ERROR_GIFCODE_NOT_TIME INT = -102
            //DECLARE @_ERROR_GIFCODE_EXPIRE INT = -103
            //DECLARE @_ERROR_GIFCODE_LOCK INT = -104

            balance = -1;
 
            try
            {
                int responseStatus = 0;
                var pars = new SqlParameter[10];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_NickName", nickName);
                pars[3] = new SqlParameter("@_Code", giftcode);
                pars[4] = new SqlParameter("@_MerchantId", merchantId);
                pars[5] = new SqlParameter("@_MerchantKey", merchantKey);
                pars[6] = new SqlParameter("@_SourceID", sourceId);
                pars[7] = new SqlParameter("@_ClientIP", ip);
                pars[8] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                SQLAccess.getGifcode().ExecuteNonQuerySP("SP_GiftCodeData_Charging", pars);

                responseStatus = Convert.ToInt32(pars[9].Value);
                if(responseStatus >= 0)
                    balance = Convert.ToInt64(pars[8].Value);
                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return -99;
        }

        public int GetNumberTurn(long accountID)
        {
           
            try
            {
              
                SqlParameter[] pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_Accountid", accountID);
                pars[1] = new SqlParameter("@_Lixi", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_Response", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var result = SQLAccess.getGifcode().ExecuteNonQuerySP("SP_Get_Lixi_by_Accountid", pars);
                if (Convert.ToInt32(pars[2].Value) < 0)
                {
                    return 0;
                }
                else
                    return Convert.ToInt32(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return 0;
            }
        }

        public SpinModel Spin(long accountID, string userName, string clientIP, int sourceID)
        {
      
            try
            {
               
                SqlParameter[] pars = new SqlParameter[14];
                pars[0] = new SqlParameter("@_Accountid", accountID);
                pars[1] = new SqlParameter("@_Username", accountID);
                pars[2] = new SqlParameter("@_ClientIP", accountID);
                pars[3] = new SqlParameter("@_SourceID", accountID);
                pars[4] = new SqlParameter("@_MerchantID", accountID);
                pars[5] = new SqlParameter("@_SpinID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_LIXI", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_PrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_vatphamType", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_VatphamGroupID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_TenVatpham", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_BitValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[12] = new SqlParameter("@_BacValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[13] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                var result = _dbHelperEvent.ExecuteNonQuerySP("SP_Spins_Event2016", pars);
                if (Convert.ToInt32(pars[13].Value) < 0)
                {
                    return null;
                }
                else
                    return new SpinModel()
                    {
                        SpinID = Convert.ToInt64(pars[5].Value),
                        LIXI = Convert.ToInt32(pars[6].Value),
                        PrizeValue = Convert.ToInt64(pars[7].Value),
                        vatphamType = Convert.ToInt32(pars[8].Value),
                        VatphamGroupID = Convert.ToInt32(pars[9].Value),
                        TenVatpham = pars[10].Value.ToString(),
                        BitValue = Convert.ToInt64(pars[11].Value),
                        BacValue = Convert.ToInt64(pars[12].Value),
                        ResponseStatus = Convert.ToInt64(pars[13].Value),
                    };
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
        }

        public List<SpinModel> GetTopWinners(int topCount)
        {
        
            try
            {
              
                SqlParameter[] pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_TopCount", topCount);
                var result = _dbHelperEvent.GetListSP<SpinModel>("SP_Spins_GetTopWinners", pars);
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
        }

        public List<SpinModel> GetAccountHistory(int topCount, long accountID)
        {
          
            try
            {
                
                SqlParameter[] pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_TopCount", topCount);
                var result = _dbHelperEvent.GetListSP<SpinModel>("SP_Spins_GetAccountHistory", pars);
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
        }

        public dynamic GetCardBit(long accountID, string userName, long spinID, int type, string telco)
        {
           
            try
            {
               
                SqlParameter[] pars = new SqlParameter[10];
                pars[0] = new SqlParameter("@_Accountid", accountID);
                pars[1] = new SqlParameter("@_UserName", userName);
                pars[2] = new SqlParameter("@_SpinID", spinID);
                pars[3] = new SqlParameter("@_Type", type);
                pars[4] = new SqlParameter("@_Telco", telco);
                pars[5] = new SqlParameter("@_Serial", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_CardCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_BIT", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_BAC", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelperEvent.ExecuteNonQuerySP("SP_GET_Card_Bit", pars);
                if (Convert.ToInt32(pars[9].Value) > 0)
                {
                    return new
                    {
                        Serial = pars[5].Value.ToString(),
                        CardCode = pars[6].Value.ToString(),
                        Bit = Convert.ToInt32(pars[7].Value),
                        Bac = Convert.ToInt32(pars[8].Value),
                        Response = Convert.ToInt32(pars[9].Value),
                    };
                }
                else return null;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
        }

        #region VQMM

        public int EventVQMMGet(long accountID)
        {
       
            try
            {
              
                SqlParameter[] pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_Remain", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelperEvent.ExecuteNonQuerySP("SP_VQMM_Get", pars);
                return Convert.ToInt32(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        public List<VQMMCard> VQMMGetCard(long accountID)
        {

            try
            {
                
                SqlParameter[] pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                return _dbHelperEvent.GetListSP<VQMMCard>("SP_VQMM_GetCard", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new List<VQMMCard>();
            }
        }

        public void VQMMUpdateCard(long spinID, string cardSeri, string cardCode, string cardType, string desc)
        {
            try
            {
                
                SqlParameter[] pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_SpinID", spinID);
                pars[1] = new SqlParameter("@_CardSeri", cardSeri);
                pars[2] = new SqlParameter("@_CardCode", cardCode);
                pars[3] = new SqlParameter("@_Description", desc);
                pars[4] = new SqlParameter("@_CardType", cardType);
                _dbHelperEvent.ExecuteNonQuerySP("SP_VQMM_UpdateCard", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }
        public int GetBon(long accountID, string userName, long spinID, string ClientIP, out long Balance)
        {
           
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_AccountId", accountID);
                pars[1] = new SqlParameter("@_Username", userName);
                pars[2] = new SqlParameter("@_SpinID", spinID);
                pars[3] = new SqlParameter("@_ClientIP", ClientIP);
                pars[4] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelperEvent.ExecuteNonQuerySP("SP_VQMM_GetBon", pars);
                var res = Convert.ToInt32(pars[5].Value);
                Balance = Convert.ToInt64(pars[4].Value);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.Error(ex.Message);
                Balance = 0;
                return -99;
            }
        }
        public int VQMMUpdateStatus(long spinID, int status)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_SpinID", spinID);
                pars[1] = new SqlParameter("@_Status", status) { Direction = ParameterDirection.InputOutput };
                _dbHelperEvent.ExecuteNonQuerySP("SP_VQMM_Card_UpdateStatus", pars);
                return Convert.ToInt32(pars[1].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return -99;
            }
        }

        public VQMMCard VQMMGetCardBySpinID(long spinID, long accountID)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_SpinID", spinID);
                pars[1] = new SqlParameter("@_AccountID", accountID);
                var res = _dbHelperEvent.GetListSP<VQMMCard>("SP_VQMM_Card_GetBySpinID", pars);
                return res.FirstOrDefault();
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new VQMMCard
                {
                    Status = -99
                };
            }
        }

        //public void VQMMUpdateStatus(int status)
        //{
        //    DBHelper db = null;
        //    try
        //    {
        //        db = new DBHelper(appSettings.EventDBConnectionString);
        //        var pars = new SqlParameter[1];
        //        pars[0] = new SqlParameter("@_Status", status);
        //        db.ExecuteNonQuerySP("SP_VQMM_Card_UpdateStatus", pars);
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Exception(ex);
        //    }
        //}

        public VQMMSpin EventVQMMSpin(long accountID, string accountName, bool isVip)
        {
   
            try
            {
           
                SqlParameter[] pars = new SqlParameter[13];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_IsVip", isVip);
                pars[3] = new SqlParameter("@_PrizeID_1", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_PrizeID_2", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_Prize_Value_1", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_Prize_Value_2", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_Prize_Name_1", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_Prize_Name_2", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_Remain", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_SilverBalance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[12] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelperEvent.ExecuteNonQuerySP("SP_VQMM_Spin", pars);
                var responseStatus = Convert.ToInt32(pars[12].Value);
                if (responseStatus >= 0)
                    return new VQMMSpin
                    {
                        PrizeID1 = Convert.ToInt32(pars[3].Value),
                        PrizeID2 = Convert.ToInt32(pars[4].Value),
                        PrizeValue1 = Convert.ToInt32(pars[5].Value),
                        PrizeValue2 = Convert.ToInt32(pars[6].Value),
                        PrizeName1 = pars[7].Value.ToString(),
                        PrizeName2 = pars[8].Value.ToString(),
                        Balance = Convert.ToInt64(pars[10].Value),
                        SilverBalance = Convert.ToInt64(pars[11].Value),
                        Remain = Convert.ToInt32(pars[9].Value),
                        ResponseCode = responseStatus,
                        Description = "Quay thành công"
                    };
                switch (responseStatus)
                {
                    case -98:
                        return new VQMMSpin
                        {
                            ResponseCode = responseStatus,
                            Description = "Hết lượt quay"
                        };

                    case -99:
                        return new VQMMSpin
                        {
                            ResponseCode = responseStatus,
                            Description = "Lỗi hệ thống"
                        };
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new VQMMSpin
            {
                ResponseCode = -99,
                Description = "Lỗi hệ thống"
            };
        }

        #endregion VQMM
    }
}