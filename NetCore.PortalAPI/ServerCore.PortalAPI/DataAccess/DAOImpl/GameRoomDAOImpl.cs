using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Options;
using ServerCore.DataAccess.DAO;
using ServerCore.DataAccess.DTO;
using ServerCore.PortalAPI.Models;
using ServerCore.Utilities.Database;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;

namespace ServerCore.DataAccess.DAOImpl
{
    public class GameRoomDAOImpl : IGameRoomDAO
    {
        private readonly AppSettings appSettings;
        private readonly DBHelper _dbHelperAuthen;
        public GameRoomDAOImpl(IOptions<AppSettings> options)
        {
            appSettings = options.Value;

          
        }
        //return RoomId
        // Tao room
        public long CreateRoom(string roomName, byte roomStatus, byte gameId, byte maxPlayer,
            byte minLevel, byte maxLevel, int minBet, int maxBet,
            bool isHasPass, string rule, string password, int betStep, byte roomType)
        {
            try
            {
                long responseStatus = 0;
                var pars = new SqlParameter[14];
                pars[0] = new SqlParameter("@_RoomName", roomName);
                pars[1] = new SqlParameter("@_RoomStatus", roomStatus);
                pars[2] = new SqlParameter("@_GameId", gameId);
                pars[3] = new SqlParameter("@_MaxPlayer", maxPlayer);
                pars[4] = new SqlParameter("@_MinLevel", minLevel);
                pars[5] = new SqlParameter("@_MaxLevel", maxLevel);
                pars[6] = new SqlParameter("@_MinBet", minBet);
                pars[7] = new SqlParameter("@_MaxBet", maxBet);
                pars[8] = new SqlParameter("@_IsPasswordProtected", isHasPass);
                pars[9] = new SqlParameter("@_RuleDescription", rule);
                pars[10] = new SqlParameter("@_PasswordProtected", password);
                pars[11] = new SqlParameter("@_BetStep", betStep);
                pars[12] = new SqlParameter("@_RoomType", roomType);
                pars[13] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                _dbHelperAuthen.ExecuteNonQuerySP("SP_Rooms_InsertRoomInfo", pars);
                responseStatus = Int64.Parse(pars[13].Value.ToString() ?? "-1");

                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
          
            return -1002;
        }

        /// <summary>
        /// Cap nhat so luong nguoi choi, tang hoac giam
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="changeValue"></param>
        /// <returns></returns>
        public int UpdateRoomNumberPlayer(long roomId, int changeValue)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_RoomId", roomId);
                pars[1] = new SqlParameter("@_Value", changeValue);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                _dbHelperAuthen.ExecuteNonQuerySP("SP_Rooms_UpdateRoomNumberPlayer", pars);

                return Int32.Parse(pars[2].Value.ToString() ?? "-1");
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1002;
        }

        public long UpdateRoom(long roomId, string roomName, byte roomStatus, byte gameId, byte maxPlayer,
            byte minLevel, byte maxLevel, int minBet, int maxBet,
            bool isHasPass, string rule, string password, int betStep, byte roomType, int numberPlayers)
        {
            try
            {
                long responseStatus = 0;
                var pars = new SqlParameter[16];
                pars[0] = new SqlParameter("@_RoomId", roomId);
                pars[1] = new SqlParameter("@_RoomName", roomName);
                pars[2] = new SqlParameter("@_RoomStatus", roomStatus);
                pars[3] = new SqlParameter("@_GameId", gameId);
                pars[4] = new SqlParameter("@_MaxPlayer", maxPlayer);
                pars[5] = new SqlParameter("@_MinLevel", minLevel);
                pars[6] = new SqlParameter("@_MaxLevel", maxLevel);
                pars[7] = new SqlParameter("@_MinBet", minBet);
                pars[8] = new SqlParameter("@_MaxBet", maxBet);
                pars[9] = new SqlParameter("@_IsPasswordProtected", isHasPass);
                pars[10] = new SqlParameter("@_RuleDescription", rule);
                pars[11] = new SqlParameter("@_PasswordProtected", password);
                pars[12] = new SqlParameter("@_BetStep", betStep);
                pars[13] = new SqlParameter("@_RoomType", roomType);
                pars[14] = new SqlParameter("@_NumberCurrentPlayers", numberPlayers);
                pars[15] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                _dbHelperAuthen.ExecuteNonQuerySP("SP_Rooms_UpdateRoomInfo", pars);
                responseStatus = Int64.Parse(pars[15].Value.ToString() ?? "-1");
                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1002;
        }

        /// <summary>
        /// Return RoomDb, cho SP hoan chinh.
        ///  @_Condition TINYINT 
        //,@_RoomId BIGINT
        //,@_RoomName varchar(20)
        //,@_RoomStatus TINYINT 
        //,@_GameId TINYINT
        //,@_MaxPlayer TINYINT
        //,@_Level TINYINT
        //,@_Bet INT
        //,@_IsPasswordProtected TINYINT
        //,@_RuleDescription NVARCHAR(100)
        //,@_PasswordProtected VARCHAR(10)
        //,@_BetStep INT
        //,@_RoomType TINYINT
        //,@_NumberCurrentPlayers TINYINT
        //,@_ResponseStatus BIGINT OUTPUT
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="gameId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        //public RoomDb FindRoom(byte condition, long roomId, string roomName, byte roomStatus, byte gameId, byte maxPlayer, byte level,
        //    int bet, byte isPasswordProtected, string rule, string passwordProtected, int betStep, byte roomType, byte numberPlayers)
        //{
        //    RoomDb roomdb = new RoomDb();
        //    try
        //    {
        //        var pars = new SqlParameter[15];
        //        pars[0] = new SqlParameter("@_Condition", condition);
        //        pars[1] = new SqlParameter("@_RoomId", roomId);
        //        pars[2] = new SqlParameter("@_RoomName", roomName);
        //        pars[3] = new SqlParameter("@_RoomStatus", roomStatus);
        //        pars[4] = new SqlParameter("@_GameId", gameId);
        //        pars[5] = new SqlParameter("@_MaxPlayer", maxPlayer);
        //        pars[6] = new SqlParameter("@_Level", level);
        //        pars[7] = new SqlParameter("@_Bet", bet);
        //        pars[8] = new SqlParameter("@_IsPasswordProtected", isPasswordProtected);
        //        pars[9] = new SqlParameter("@_RuleDescription", rule);
        //        pars[10] = new SqlParameter("@_PasswordProtected", passwordProtected);
        //        pars[11] = new SqlParameter("@_BetStep", betStep);
        //        pars[12] = new SqlParameter("@_RoomType", roomType);
        //        pars[13] = new SqlParameter("@_NumberCurrentPlayers", numberPlayers);
        //        pars[14] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //        //var list = db.GetList<RoomDb>("SP_Rooms_GetMatchRoomByCondition", pars);
        //        // var result = db.ExecuteNonQuerySP("SP_Rooms_GetMatchRoomByCondition",pars);
        //        DataTable table = _dbHelperAuthen.GetDataTableSP("SP_Rooms_GetMatchRoomByCondition", pars);

        //        if (table.Rows.Count > 0)
        //        {
        //            roomdb.RoomId = Int64.Parse(table.Rows[0]["RoomId"].ToString());
        //            roomdb.RoomName = table.Rows[0]["RoomName"].ToString();
        //            roomdb.RoomStatus = Byte.Parse(table.Rows[0]["RoomStatus"].ToString());
        //            roomdb.GameId = Byte.Parse(table.Rows[0]["GameId"].ToString());
        //            roomdb.MaxPlayer = Byte.Parse(table.Rows[0]["MaxPlayer"].ToString());
        //            roomdb.MinLevel = Byte.Parse(table.Rows[0]["MinLevel"].ToString());
        //            roomdb.MaxLevel = Byte.Parse(table.Rows[0]["MaxLevel"].ToString());
        //            roomdb.MinBet = Byte.Parse(table.Rows[0]["MinBet"].ToString());
        //            roomdb.MaxBet = Byte.Parse(table.Rows[0]["MaxBet"].ToString());

        //            roomdb.IsPasswordProtected = Boolean.Parse(table.Rows[0]["IsPasswordProtected"].ToString());
        //            roomdb.RuleDescription = table.Rows[0]["RuleDescription"].ToString();
        //            roomdb.Password = table.Rows[0]["PasswordProtected"].ToString();

        //            roomdb.BetStep = Int32.Parse(table.Rows[0]["BetStep"].ToString());
        //            roomdb.RoomType = Byte.Parse(table.Rows[0]["RoomType"].ToString());
        //            roomdb.NumberCurrentPlayers = Byte.Parse(table.Rows[0]["NumberCurrentPlayers"].ToString());
        //            return roomdb;
        //        }

        //        var responseStatus = Int32.Parse(pars[14].Value.ToString() ?? "-1");
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Exception(ex);
        //    }
            
        //    return null;
        //}

        //return RoomConfigId
        public int CreateRoomConfig(int minBet, int maxBet, int minLevel, int maxLevel, bool isFriendOnly, byte moneyType)
        {
            try
            {
                int responseStatus = 0;
                var pars = new SqlParameter[7];
                pars[0] = new SqlParameter("@_MinBet", minBet);
                pars[1] = new SqlParameter("@_MaxBet", maxBet);
                pars[2] = new SqlParameter("@_MinLevel", minLevel);
                pars[3] = new SqlParameter("@_MaxLevel", maxLevel);
                pars[4] = new SqlParameter("@_OnlyFriend", isFriendOnly);
                pars[5] = new SqlParameter("@_MoneyType", moneyType); 
                pars[6] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                _dbHelperAuthen.ExecuteNonQuerySP("SP_RoomConfig_InsertRoomConfig", pars);
                responseStatus = Int32.Parse(pars[6].Value.ToString() ?? "-1");

                return responseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
           
            return -1002;
        }
    }
}