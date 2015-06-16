using System.Collections;

namespace Arduino
{
    public class AVRStatusRegister
    {
        byte data = 0;

        public const int CARRY = 0;
        public const int ZERO = 1;
        public const int NEGATIVE = 2;
        public const int OVERFLOW = 3;
        public const int SIGN = 4;
        public const int HALFCARRY = 5;
        public const int BITCOPY = 6;
        public const int INTERUPT = 7;

        public bool isFlagSet(int i) {
            //TODO: check 0 <= i <= 7
            return (data & 0x01 << i) != 0;
        }

        public bool isFlagClear(int i) {
            //TODO: check 0 <= i <= 7
            return (data & 0x01 << i) == 0;
        }
    }
}