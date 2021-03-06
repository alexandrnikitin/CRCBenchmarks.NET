﻿// from https://github.com/Microsoft/Kafkanet/blob/455510ab175cc52e8a49262b99fe9da1626c9d26/src/KafkaNET.Library/Utils/Crc32Hasher.cs

namespace Kafka.Client.Utils
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// From http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net
    /// </summary>
    public class Crc32Hasher : HashAlgorithm
    {
        internal const UInt32 DefaultPolynomial = 0xedb88320;
        internal const UInt32 DefaultSeed = 0xffffffff;

        private UInt32 hash;
        private UInt32 seed;
        private UInt32[] table;
        private static UInt32[] defaultTable;

        public Crc32Hasher()
        {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            Initialize();
        }

        [CLSCompliant(false)]
        public Crc32Hasher(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = seed;
            Initialize();
        }

        public override void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(table, hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
            this.HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize
        {
            get { return 32; }
        }

        public static byte[] Compute(byte[] bytes)
        {
            var hasher = new Crc32Hasher();
            byte[] hash = hasher.ComputeHash(bytes);
            return hash;
        }

        internal static uint ComputeCrcUint32(byte[] bytes, int offset, int count)
        {
            var hasher = new Crc32Hasher();
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, bytes, offset, count);
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

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
                defaultTable = createTable;

            return createTable;
        }

        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
        {
            UInt32 crc = seed;
            for (int i = start; i < start + size; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        private byte[] UInt32ToBigEndianBytes(UInt32 x)
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
