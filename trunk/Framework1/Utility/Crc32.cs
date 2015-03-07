using System;
using System.IO;
using System.Security.Cryptography;

namespace Server3.Utility
{
    public class Crc32
    {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;

        private static UInt32[] s_DefaultTable;

        private Crc32()
        {
            
        }

        static Crc32()
        {
            s_DefaultTable = InitializeTable(DefaultPolynomial);
        }

        public static int HashSize
        {
            get { return 4; }
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && s_DefaultTable != null)
                return s_DefaultTable;

            UInt32[] createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                s_DefaultTable = createTable;

            return createTable;
        }

        public static UInt32 CalculateHash(byte[] buffer, UInt32 seed = DefaultSeed)
        {
            UInt32 crc = seed;
            for (int i = 0; i < buffer.Length; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ s_DefaultTable[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        public static byte[] UInt32ToBigEndianBytes(UInt32 x)
        {
            return new[] {
 			(byte)((x >> 24) & 0xff),
 			(byte)((x >> 16) & 0xff),
 			(byte)((x >> 8) & 0xff),
 			(byte)(x & 0xff)
 		};
        }
    }
}
