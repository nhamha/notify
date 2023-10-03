using System;
using System.Security.Cryptography;

namespace NetCore.Utils
{
    public class RandomUtil
    {
        protected static readonly RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        protected static byte[] randomNumber = new byte[4];
        private static CryptoRandom rd = new CryptoRandom();

        public static int NextByte(int value)
        {
            if (value == 0)
                throw new DivideByZeroException();
            rngCsp.GetBytes(randomNumber);
            return Math.Abs(randomNumber[0] % value);
        }

        public static int NextInt(int value)
        {
            if (value == 0)
                throw new DivideByZeroException();
            rngCsp.GetBytes(randomNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(randomNumber);
            return Math.Abs(BitConverter.ToInt32(randomNumber, 0) % value);
        }

        public static int Random(int minValue, int maxValue)
        {
            return rd.Next(minValue, maxValue);
        }
    }
}