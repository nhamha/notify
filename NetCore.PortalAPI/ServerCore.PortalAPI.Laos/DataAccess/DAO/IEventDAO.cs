using ServerCore.DataAccess.DTO;
using System;
using System.Collections.Generic;

namespace ServerCore.DataAccess.DAO
{
    public interface IEventDAO
    {
        int CheckGiftCode(long accountId, string accountName, string nickName, string giftcode, int merchantId, string merchantKey, int sourceId, string ip, out long balance);

        int GetNumberTurn(long accountID);

        SpinModel Spin(long accountID, string userName, string clientIP, int sourceID);

        List<SpinModel> GetTopWinners(int topCount);

        List<SpinModel> GetAccountHistory(int topCount, long accountID);

        dynamic GetCardBit(long accountID, string userName, long spinID, int type, string telco);

        #region VQMM

        int EventVQMMGet(long accountID);

        VQMMSpin EventVQMMSpin(long accountID, string accountName, bool isVip);
        List<VQMMCard> VQMMGetCard(long accountID);

        void VQMMUpdateCard(long spinID, string cardSeri, string cardCode, string cardType, string desc);

        int VQMMUpdateStatus(long spinID, int status);

        VQMMCard VQMMGetCardBySpinID(long spinID, long accountID);
        int GetBon(long accountID, string userName, long spinID, string ClientIP, out long Balance);

        #endregion VQMM
    }
}