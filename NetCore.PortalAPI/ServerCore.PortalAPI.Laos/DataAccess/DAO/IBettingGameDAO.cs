using System;
using System.Collections.Generic;
using System.Linq;
using ServerCore.Utilities.Models;
using ServerCore.DataAccess.DTO;
using ServerCore.DataAccess.DAOImpl;

namespace ServerCore.DataAccess.DAO
{
    public abstract class IBettingGameDAO
    {
        public abstract long CreateRoom(int roomIndex, byte gameId, byte moneyType, int minBet, int betStep, int maxBet, string password, byte rule);

        public abstract long CreateSession(byte gameID, long gameOwnerID, string playersData, long roomID, byte moneyType, int minBet, out float fee);
        public abstract long CreateSession(byte gameId, long gameOwnerId, string playersData, long roomId, byte moneyType, int minBet, long refSessionId);

        public abstract long CreateTransaction(byte gameID, long roomID, long gameSessionID, string transData);
        public abstract long CreateTransaction(byte gameID, long roomID, long gameSessionID, string transData, long refGameSessionID);

        public abstract long FinishSession(long roomId, long sessionId, byte gameID);

        public abstract long CheckTransaction(long sessionId, byte gameID);

        public abstract long CancelSession(long roomId, long sessionId, byte gameID);

        public abstract long TopupEventCoin(int serviceid, string servicekey, long accountid, string username, long amount, string description, long refid, string ipadress);

        public abstract List<ProfileDb> GetProfile(long accountid, byte gameID, byte roomType);

        public abstract long UpdateProfile(long accountID, string accountName, long amount, byte moneyType, byte gameId, string chacracterName);

        public abstract List<ProfileDb> GetLeaderBoard(byte gameId, byte moneyType, bool isWeek);

        public abstract void InsertCCU(byte gameid, int vip, int normal, int total, int serviceID);

        public abstract string GetSystemMessage(byte gameid, out int remainTime);

        public abstract void LogPhomActions(long sessionid, string initdata, string actions);

        public abstract void LogXitoActions(long sessionid, string initdata, string actions);
        public abstract void LogPokerActions(long sessionid, string initdata, string actions);

        public abstract void LogTienLenMienNamActions(long sessionid, long gameloopId, string initdata, string actions);
        
        public abstract void LogSamLocActions(long sessionid, long gameloopId, string initdata, string actions);

        public abstract void LogTienLenMienBacActions(long sessionid, string initdata, string actions);

        public abstract void LogBaCayActions(long sessionid, string initdata);

        public abstract void LogMaubinhActions(long sessionid, string initdata,string actions);

        public abstract void LogLiengActions(long sessionid, string initdata, string actions);

        public abstract List<AccountForm> GetBotAccounts(int top, int index = 0);

        public abstract long UpdateBotState(long accountId, int state);

        public abstract long UpdateBotState(long accountId, string username, int state);

        public abstract List<ConfigGame> GetConfigGame(int gameTypeId);

        public abstract List<ProfileDb> GetAllProfile(long accountid);

        public abstract long UpdatePlayersInGame(int gameId, int roomValue, int roomType, int countPlayers);
        public abstract List<CCUItems> GetCCU(int gameId);
    }
}