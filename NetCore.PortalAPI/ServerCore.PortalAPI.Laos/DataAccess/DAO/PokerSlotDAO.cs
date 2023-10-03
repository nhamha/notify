using System;

using ServerCore.DataAccess.DTO;
using System.Collections.Generic;
using ServerCore.DataAccess.DAOImpl;

namespace ServerCore.DataAccess.DAO {
    //Cac SP tu PaygateDB: dang nhap va lay thong tin tai khoan
    public interface PokerSlotDAO {
        MiniPokerSpinResponse Spin(long accountId, string accountName, int betType, int roomId, string ip, int sourceId, int merchantId);
        long GetJackpot(int betType, int roomId);
        List<MiniPokerAccountHistory> GetAccountHistory(int accountId, int betType, int topCount);
        List<MiniPokerTopWinnerModel> GetTopWinners(int betType, int topCount);

        int CreateHistory(long SpinID, long AccountID, string Username, int RoomID, int BetType, long BetValue, int CardTypeID,
                                   long PrizeValue, bool IsJackpot, string CardResult, string ClientIP = "", int SourceID = 0, int MerchantID = 0);

        List<MiniPokerAccountHistoryDetail> GetAccountHistoryDetails(int accountId, int betType, long spinID);

        MiniPokerSpinResponse Spinmini(long accountId, string accountName, int betType, int roomId, string ip, int sourceId, int merchantId);
    }
}