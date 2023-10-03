using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.Utilities.Utils {
    public enum PlatformDef {
        ANDROID_PLATFORM = 1,
        IOS_PLATFORM = 2,
        WINDOWS_PLATFORM = 3,
        WEB_PLATFORM = 4
    }

    public enum Mode
    {
        PUBLIC = 0,
        ALPHA = 1,
    }

    public enum GameMoneyType
    {
        BAC = 0,
        BON = 1
    }

    public enum GameBetType
    {
        MONEY = 1,
        FREE = 2
    }

    public class MoneyPercent
    {
        public static readonly double NORMAL = 1;
        public static readonly double BAC = 0.93;
        public static readonly double BON = 0.98;
    }

    public class GameIdTypes
    {
        public const byte BA_CAY = 1;
        public const byte PHOM = 3;
        public const byte POKERHK = 5;
        public const byte TIEN_LEN_MN = 7;
        public const byte TIEN_LEN_MN_NHAT_AN_TAT = 27;
        public const byte TIEN_LEN_MN_SOLO = 33;
        public const byte MAU_BINH = 9;
        public const byte TIEN_LEN_MB = 11;
        public const byte POKER = 13;
        public const byte SAM_LOC = 15;
        public const byte SAM_LOC_SOLO = 35;
        public const byte LIENG = 17;
        public const byte CHAN = 19;
        public const byte BA_CAY_BIEN = 23;
        public const byte BA_CAY_GA = 21;
        public const byte XOC_DIA = 25;
        public const byte TAI_XIU = 40;
        public const byte POKER_SLOT = 41;
        public const byte CARD_SLOT = 42;
    }
}
