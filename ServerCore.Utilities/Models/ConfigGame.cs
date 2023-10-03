using System;
using System.Linq;

namespace ServerCore.Utilities.Models
{
    [Serializable]
    public class ConfigGame
    {
        public ConfigGame()
        {
        }

        public ConfigGame(int gametypeId, float rate, int currency, string gameTypeName)
        {
            GameRoomTypeId = gametypeId;
            PrizeRate = rate;
            Currency = currency;
            GameTypeName = gameTypeName;
        }

        int GameRoomTypeId { get; set; }

        float PrizeRate { get; set; }

        int Currency { get; set; }

        string GameTypeName { get; set; }
    }
}