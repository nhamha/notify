using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Models;
using ServerCore.DataAccess.DTO;
using ServerCore.Utilities.Utils;
using ServerCore.Utilities.Database;
using Microsoft.Extensions.Options;
using ServerCore.PortalAPI.Models;
using Netcore.Gateway.Interfaces;

namespace ServerCore.DataAccess.DAOImpl
{
    public class BettingGameDAO : IBettingGameDAO
    {
        private readonly AppSettings appSettings;
       
        public BettingGameDAO(IOptions<AppSettings> options)
        {
            appSettings = options.Value;
        }
        /// <summary>
        /// Tao Session. roomName = pri(pub):roomId:moneyType:rule
        /// </summary>
        /// <param name="roomIndex">Chi so phong: 1 -> 1000</param>
        /// <param name="gameId">Game ID</param>
        /// <param name="minBet">Min bet</param>
        /// <param name="betStep">Bet step</param>
        /// <param name="maxBet">Max bet</param>
        /// <param name="password">Password</param>
        /// <param name="rule">Rule</param>
        /// <param name="moneyType">Money type</param>
        /// <returns>SessionID</returns>
        public override long CreateRoom(int roomIndex, byte gameId, byte moneyType, int minBet, int betStep, int maxBet, string password, byte rule)
        {
            long responseStatus = -1;
            string roomName = string.Empty;
            try
            {
                roomName = string.Format("{0}:{1}", string.IsNullOrEmpty(password) ? "public" : "private", roomIndex);

                var pars = new SqlParameter[10];
                pars[0] = new SqlParameter("@_RoomName", roomName);
                pars[1] = new SqlParameter("@_GameID", gameId);
                pars[2] = new SqlParameter("@_MoneyType", moneyType);
                pars[3] = new SqlParameter("@_MinBet", minBet);
                pars[4] = new SqlParameter("@_BetStep", betStep);
                pars[5] = new SqlParameter("@_MaxBet", maxBet);
                pars[6] = new SqlParameter("@_CreatedTime", DateTime.Now);
                pars[7] = new SqlParameter("@_PasswordProtected", password);
                pars[8] = new SqlParameter("@_Rule", rule);
                pars[9] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameRoom_CreateGameRoom", pars);
                if (Int64.TryParse(pars[9].Value.ToString() ?? "-1", out responseStatus))
                {
                    return responseStatus;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1002;
        }

        /// <summary>
        /// Tao GameLoop
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="gameOwnerId">Owner ID</param>
        /// <param name="playersData">Players Data</param>
        /// <param name="roomId">Room ID</param>
        /// <param name="moneyType"></param>
        /// <param name="minBet"></param>
        /// <returns>GameLoop ID</returns>
        public override long CreateSession(byte gameId, long gameOwnerId, string playersData, long roomId, byte moneyType, int minBet, out float fee)
        {
            long gameLoopId = -1;
            fee = 0.0f;
            try
            {
                var pars = new SqlParameter[10];
                pars[0] = new SqlParameter("@_GameID", gameId);
                pars[1] = new SqlParameter("@_MoneyType", moneyType);
                pars[2] = new SqlParameter("@_Owner", gameOwnerId);
                pars[3] = new SqlParameter("@_ParValue", minBet);
                pars[4] = new SqlParameter("@_GameData", playersData);
                pars[5] = new SqlParameter("@_RoomID", roomId);
                pars[6] = new SqlParameter("@_CreatedTime", DateTime.Now);
                pars[7] = new SqlParameter("@_RefGameRoomID", 0);
                pars[8] = new SqlParameter("@_FeeRate", SqlDbType.Float) { Direction = ParameterDirection.Output };//fee
                pars[9] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//sess

                //NLogManager.LogMessage("@_GameID:" + Convert.ToString(gameId) + "@_MoneyType: " + Convert.ToString(moneyType) + "@_Owner: " + gameOwnerId.ToString() + "@_ParValue: " + minBet
                //                        + "@_CreatedTime: " + DateTime.Now.ToString() + "@_GameData: " + playersData + "@_RoomID: " + roomId + "@_RefGameRoomID: 0");
                //db.ExecuteNonQuerySP("SP_GameSession_CreateGameSession", pars);
                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameSession_CreateGameSession_WithPar", pars);
                if(Int64.TryParse(pars[9].Value.ToString() ?? "-1", out gameLoopId))
                {
                    float.TryParse(pars[8].Value.ToString() ?? "", out fee);
                    return gameLoopId;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1002;
        }

        /// <summary>
        /// Tao GameLoop
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="gameOwnerId">Owner ID</param>
        /// <param name="playersData">Players Data</param>
        /// <param name="roomId">Room ID</param>
        /// <param name="moneyType"></param>
        /// <param name="minBet"></param>
        /// <param name="refSessionId"></param>
        /// <returns>GameLoop ID</returns>
        public override long CreateSession(byte gameId, long gameOwnerId, string playersData, long roomId, byte moneyType, int minBet, long refSessionId)
        {
            long sessId = -1;
            try
            {
                var pars = new SqlParameter[9];
                pars[0] = new SqlParameter("@_GameID", gameId);
                pars[1] = new SqlParameter("@_MoneyType", moneyType);
                pars[2] = new SqlParameter("@_Owner", gameOwnerId);
                pars[3] = new SqlParameter("@_ParValue", minBet);
                pars[4] = new SqlParameter("@_GameData", playersData);
                pars[5] = new SqlParameter("@_RoomID", roomId);
                pars[6] = new SqlParameter("@_CreatedTime", DateTime.Now);
                pars[7] = new SqlParameter("@_RefGameRoomID", refSessionId);
                pars[8] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//sess

                //db.ExecuteNonQuerySP("SP_GameSession_CreateGameSession", pars);
                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameSession_CreateGameSession_WithPar", pars);
                if (Int64.TryParse(pars[8].Value.ToString() ?? "-1", out sessId))
                {
                    NLogManager.Info(string.Format("RoomID = {0}, GameOwnerID = {1}, PlayersData = {2}, SessionID = {3},", roomId, gameOwnerId, playersData, sessId));
                    return sessId;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return -1002;
        }

        public override long CreateTransaction(byte gameID, long roomID, long gameSessionID, string transData)
        {
            long responseStatus = -1;
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_GameID", gameID);
                pars[1] = new SqlParameter("@_RoomID", roomID);
                pars[2] = new SqlParameter("@_GameSessionID", gameSessionID);
                pars[3] = new SqlParameter("@_GameTransactionData", transData ?? string.Empty);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                // goi SP 5 lan cho den khi thanh cong thi thoi
                int count = 0;
                while(responseStatus < 0)
                {
                    SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameTransaction_CreateGameTransaction", pars);
                    Int64.TryParse(pars[4].Value.ToString() ?? "-1", out responseStatus);
                        
                    count++;
                    if (count > 5)
                        break;
                }
                return responseStatus;
                //db.ExecuteNonQuerySP("SP_GameTransaction_CreateGameTransaction", pars);
                //if (Int64.TryParse(pars[4].Value.ToString() ?? "-1", out responseStatus))
                //{
                //    return responseStatus;
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1002;
        }

        public override long CreateTransaction(byte gameID, long roomID, long gameSessionID, string transData, long refGameSessionID)
        {
            long responseStatus = -1;
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_GameID", gameID);
                pars[1] = new SqlParameter("@_RoomID", roomID);
                pars[2] = new SqlParameter("@_GameSessionID", gameSessionID);
                pars[3] = new SqlParameter("@_GameTransactionData", transData ?? string.Empty);
                pars[4] = new SqlParameter("@_RefGameSessionID", refGameSessionID);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameTransaction_CreateGameTransaction_2", pars);
                if (Int64.TryParse(pars[5].Value.ToString() ?? "-1", out responseStatus))
                {
                    return responseStatus;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1002;
        }

        public override long FinishSession(long roomId, long sessionId, byte gameID)
        {
            long responseStatus = -1;
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameID", gameID);
                pars[1] = new SqlParameter("@_RoomID", roomId);
                pars[2] = new SqlParameter("@_GameSessionID", sessionId);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameSession_FinishGameSession", pars);

                if(Int64.TryParse(pars[3].Value.ToString() ?? "-1", out responseStatus))
                {
                    return responseStatus;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return -1002;
        }

        public override long CheckTransaction(long sessionId, byte gameID)
        {
            throw new NotImplementedException();
        }

        public override long CancelSession(long roomId, long gameSessionId, byte gameID)
        {
            try
            {
                long responseStatus = -1;

                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameID", gameID);
                pars[1] = new SqlParameter("@_RoomID", roomId);
                pars[2] = new SqlParameter("@_GameSessionID", gameSessionId);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameSession_CancelGameTransaction", pars);
                if (Int64.TryParse(pars[3].Value.ToString() ?? "-1", out responseStatus))
                {
                    return responseStatus;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return -1002;
        }

        public override long TopupEventCoin(int serviceid, string servicekey, long accountid, string username, long amount, string description, long refid, string ipadress)
        {
            try
            {
                long coin = -1;

                var pars = new SqlParameter[10];
                pars[0] = new SqlParameter("@_ServiceID", serviceid);
                pars[1] = new SqlParameter("@_ServiceKey", servicekey);
                pars[2] = new SqlParameter("@_AccountID", accountid);
                pars[3] = new SqlParameter("@_Usernamey", username);
                pars[4] = new SqlParameter("@_Amount", amount);
                pars[5] = new SqlParameter("@_Description", description);
                pars[6] = new SqlParameter("@_ReferenceID", refid);
                pars[7] = new SqlParameter("@_ClientIP", ipadress);
                pars[8] = new SqlParameter("@_Coin", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameCard_TopupEventCoin", pars);
                if (Int64.TryParse(pars[8].Value.ToString() ?? "-1", out coin))
                {
                    return coin;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1002;
        }

        public override List<ProfileDb> GetProfile(long accountid, byte gameID, byte roomType)
        {
            try
            {
     
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameID", gameID);
                pars[1] = new SqlParameter("@_AccountID", accountid);
                pars[2] = new SqlParameter("@_RoomType", roomType);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                List<ProfileDb> profile = SQLAccess.getAuthen().GetListSP<ProfileDb>("SP_GameProfile_GetProfileByAccountID", pars);
                return profile;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
            return null;
        }

        public override long UpdateProfile(long accountID, string accountName, long amount, byte moneyType, byte gameId, string characterName)
        {
            long responseStatus = -1;
            try
            {
             
                int win = 0;
                int lose = 0;
                long winMoney = 0;
                long loseMoney = 0;

                if (amount < 0)
                {
                    lose = 1;
                    loseMoney = -amount;
                }
                else if (amount > 0)
                {
                    win = 1;
                    winMoney = amount;
                }
                else
                {
                }

                var pars = new SqlParameter[16];
                pars[0] = new SqlParameter("@_GameID", gameId);
                pars[1] = new SqlParameter("@_AccountID", accountID);
                pars[2] = new SqlParameter("@_AccountName", accountName);
                pars[3] = new SqlParameter("@_CharacterID", accountID);
                pars[4] = new SqlParameter("@_CharacterName", characterName);
                pars[5] = new SqlParameter("@_Experiences", 1);
                pars[6] = new SqlParameter("@_Level", 1);
                pars[7] = new SqlParameter("@_Gender", true);
                pars[8] = new SqlParameter("@_TotalMoneyWins", winMoney);
                pars[9] = new SqlParameter("@_TotalMoneyLoses", loseMoney);
                pars[10] = new SqlParameter("@_TotalWin", win);
                pars[11] = new SqlParameter("@_TotaLose", lose);
                pars[12] = new SqlParameter("@_RoomType", moneyType);
                pars[13] = new SqlParameter("@_Achivements", string.Empty);
                pars[14] = new SqlParameter("@_TotalPoint", 1);
                pars[15] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameProfile_UpdateProfile", pars);
                if (Int64.TryParse(pars[15].Value.ToString() ?? "-1", out responseStatus))
                {
                    return responseStatus;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return -1002;
        }

        public override List<ProfileDb> GetLeaderBoard(byte gameId, byte moneyType, bool isWeek)
        {
            long responseStatus = -1;

            try
            {
                DateTime end = DateTime.Now;
                DateTime start = end.AddMonths(-1);

                if (isWeek)
                {

                    DayOfWeek dow = end.DayOfWeek;
                    int diff = dow - DayOfWeek.Monday;

                    if (dow == DayOfWeek.Sunday)
                    {
                        diff = 6;
                    }

                    start = end.AddDays(-diff);
                    start = new DateTime(start.Year, start.Month, start.Day);
                }

                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_GameID", gameId);
                pars[1] = new SqlParameter("@_RoomType", moneyType);
                pars[2] = new SqlParameter("@_StartDate", DBNull.Value);
                pars[3] = new SqlParameter("@_EndDate", DBNull.Value);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                List<ProfileDb> profile;
                if (!isWeek)
                    profile = SQLAccess.getAuthen().GetListSP<ProfileDb>("SP_GameProfile_GetLeaderBoard", pars);
                else profile = SQLAccess.getAuthen().GetListSP<ProfileDb>("SP_GameProfile_GetLeaderBoardInWeek", pars);

                if (profile.Count > 0)
                {
                    if (isWeek)
                        profile = profile.OrderByDescending(p => p.TotalMoneyWinsInWeek).ToList();
                    else
                        profile = profile.OrderByDescending(p => p.TotalMoneyWins).ToList();
                }
                Int64.TryParse(pars[4].Value.ToString() ?? "-1", out responseStatus);
                return profile;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return null;
        }

        public override void InsertCCU(byte gameid, int vip, int normal, int total, int serviceID)
        {
            if (string.IsNullOrEmpty(appSettings.ReportConnectionString))
                return;

            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_CCU", total);
                pars[1] = new SqlParameter("@_CCUVIP", vip);
                pars[2] = new SqlParameter("@_CCUNORMAL", normal);
                pars[3] = new SqlParameter("@_GameID", gameid);
                pars[4] = new SqlParameter("@_ServiceID", serviceID);
                pars[5] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_SetCCUReport", pars);
                NLogManager.Info(string.Format("GAMEDB: InsertCCU. GameID|Vip|Nornal|Total|->ResponseStatus: {0}|{1}|{2}|{3}|->{4}", gameid, vip, normal, total, responseStatus));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }

        public override string GetSystemMessage(byte gameid, out int remainTime)
        {
            int responseStatus = -1;
            remainTime = -1;
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameId", gameid);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_Message", SqlDbType.NVarChar, 1000) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_RemainTime", SqlDbType.Int) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_GameMaintainSchedule", pars);

                if (Int32.TryParse(pars[1].Value.ToString() ?? "-1", out responseStatus))
                {
                    if (responseStatus > 0)
                    {
                        remainTime = Int32.Parse(pars[3].Value.ToString());
                        NLogManager.Info(string.Format("Message:{0}", pars[2].Value.ToString()));
                        return pars[2].Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return string.Empty;
        }

        public override void LogPhomActions(long sessionid, string initdata, string actions)
        {
            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_GameAction", actions);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_PhomGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }

        public override void LogLiengActions(long sessionid, string initdata, string actions)
        {
            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_GameAction", actions);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_LiengGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            
        }

        public override void LogTienLenMienNamActions(long sessionid, long gameloopId, string initdata, string actions)
        {
            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_GameAction", actions);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_TienLenMienNamGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }

        public override void LogSamLocActions(long sessionid, long gameloopId, string initdata, string actions)
        {

            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_GameAction", actions);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_SamLocGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
        }

        public override void LogTienLenMienBacActions(long sessionid, string initdata, string actions)
        {

            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_GameAction", actions);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_TienLenMienBacGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
        }

        public override void LogXitoActions(long sessionid, string initdata, string actions)
        {

            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_GameAction", actions);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_XitoGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
        }

        public override void LogPokerActions(long sessionid, string initdata, string actions)
        {
            DBHelper db = null;
            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_GameAction", actions);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_PokerGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
        }

        public override void LogBaCayActions(long sessionid, string initdata)
        {
              int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_BaCayGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
        }

        public override void LogMaubinhActions(long sessionid, string initdata, string actions)
        {
            int responseStatus = -1;

            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                pars[1] = new SqlParameter("@_InitGameData", initdata);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_MaubinhGameLog_Create", pars);
                //db = new DBHelper(appSettings.GameLogConnectionString);
                //var pars = new SqlParameter[4];
                //pars[0] = new SqlParameter("@_GameSessionID", sessionid);
                //pars[1] = new SqlParameter("@_InitGameData", initdata);
                //pars[2] = new SqlParameter("@_GameAction", actions);
                //pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                responseStatus = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_MaubinhGameLog_Create", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
        }

        public override List<AccountForm> GetBotAccounts(int top, int index = 0)
        {
            DBHelper db = null;
            try
            {

                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_SysPartnerKey", "atchubai@vtc!@#456");
                pars[1] = new SqlParameter("@_Top", top);
                pars[2] = new SqlParameter("@_Index", index);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                return SQLAccess.getAuthen().GetListSP<AccountForm>("SP_Accounts_GetAllBots", pars);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return new List<AccountForm>();
        }

        public override long UpdateBotState(long accountId, int state)
        {
            DBHelper db = null;
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_SysPartnerKey", "atchubai@vtc!@#456");
                pars[1] = new SqlParameter("@_AccountID", accountId);
                pars[2] = new SqlParameter("@_State", state);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                long result = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_Accounts_UpdateBotState", pars);
                if (result > 0)
                    result = (long)pars[3].Value;
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           

            return -99;
        }

        public override long UpdateBotState(long accountId, string username, int state)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_SysPartnerKey", "atchubai@vtc!@#456");
                pars[1] = new SqlParameter("@_AccountID", accountId);
                pars[2] = new SqlParameter("@_Username", username);
                pars[3] = new SqlParameter("@_State", state);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                long result = SQLAccess.getAuthen().ExecuteNonQuerySP("SP_Accounts_UpdateBotStateByUsername", pars);
                if (result > 0)
                    result = (long)pars[4].Value;
                return result;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           

            return -99;
        }

        public override List<ConfigGame> GetConfigGame(int gameTypeId)
        {
            NLogManager.Info("DB CORE - GetConfigGame:");
            try
            {
                int responseStatus = 0;
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_GameTypeID", gameTypeId);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                var list = SQLAccess.getAuthen().GetListSP<ConfigGame>("SP_GameCard_GetListGameRoomTypes", pars);

                responseStatus = (int)(pars[1].Value ?? "-99");

                if (responseStatus < 0)
                {
                    throw new Exception(string.Format("DB CORE - Error GetConfigGame:{0}", responseStatus));
                }

                NLogManager.Info(string.Format("DB CORE - Response GetConfigGame:{0}", responseStatus));
                return list;
            }
            catch (SqlException sqlEx)
            {
                NLogManager.Exception(sqlEx);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return null;
        }

        public override List<ProfileDb> GetAllProfile(long accountid)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_GameID", DBNull.Value);
                pars[1] = new SqlParameter("@_AccountID", accountid);
                pars[2] = new SqlParameter("@_RoomType", DBNull.Value);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                List<ProfileDb> profile = SQLAccess.getAuthen().GetListSP<ProfileDb>("SP_GameProfile_GetProfileByAccountID_All", pars);
                return profile;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return null;
        }

        public override long UpdatePlayersInGame(int gameId, int roomValue, int roomType, int countPlayers)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_GameId", gameId);
                pars[1] = new SqlParameter("@_RoomValue", roomValue);
                pars[2] = new SqlParameter("@_RoomType", roomType);
                pars[3] = new SqlParameter("@_CountPlayers", countPlayers);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                long result = -1;
                SQLAccess.getAuthen().ExecuteNonQuerySP("SP_UpdateCCU", pars);
                result = (int)pars[4].Value;

                return result;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -99;
        }

        public override List<CCUItems> GetCCU(int gameId)
        {
            DBHelper db = null;
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_GameId", gameId);

                var result = SQLAccess.getAuthen().GetListSP<CCUItems>("SP_GetCCU_byGameid", pars);
                if(result.Count <= 0)
                    return new List<CCUItems>();

                return result;
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }

            return new List<CCUItems>();
        }
    }
}