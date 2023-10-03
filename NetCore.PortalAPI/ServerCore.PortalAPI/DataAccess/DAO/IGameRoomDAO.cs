using System;
using System.Linq;
using ServerCore.Utilities.Models;

namespace ServerCore.DataAccess.DAO
{
    public interface IGameRoomDAO
    {
        long UpdateRoom(long roomId, string roomName, byte roomStatus, byte gameId, byte maxPlayer,
            byte minLevel, byte maxLevel, int minBet, int maxBet,
            bool isHasPass, string rule, string password, int betStep, byte roomType, int numberPlayers);

        int UpdateRoomNumberPlayer(long roomId, int changeValue);

        long CreateRoom(string roomName, byte roomStatus, byte gameId, byte maxPlayer,
            byte minLevel, byte maxLevel, int minBet, int maxBet,
            bool isHasPass, string rule, string password, int betStep, byte roomType);

        int CreateRoomConfig(int minBet, int maxBet, int minLevel, int maxLevel, bool isFriendOnly, byte moneyType);

        //RoomDb FindRoom(byte condition, long roomId, string roomName, byte roomStatus, byte gameId, byte maxPlayer, byte level,
        //    int bet, byte isPasswordProtected, string rule, string passwordProtected, int betStep, byte roomType, byte numberPlayers);
    }
}