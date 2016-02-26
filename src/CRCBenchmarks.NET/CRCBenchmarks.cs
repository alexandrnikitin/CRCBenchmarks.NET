using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using DamienG.Security.Cryptography;

using Kafka.Client.Utils;

namespace CRCBenchmarks.NET
{
    //[Config(typeof(Config))]
    public class CRCBenchmarks
    {
        private const int N = 10000;

        private readonly Crc32 _crcDamien = new Crc32();
        private readonly Crc32Hasher _crcKafka = new Crc32Hasher();
        private readonly byte[] _data;

        public CRCBenchmarks()
        {
            _data = new byte[N];
            new Random(42).NextBytes(_data);
        }

        [Benchmark]
        public uint CrcVoron()
        {
            return Voron.Util.Crc.Value(_data);
        }

        [Benchmark]
        public byte[] CrcKit()
        {
            return _crcDamien.ComputeHash(_data);
        }

        [Benchmark]
        public byte[] CrcKafka()
        {
            return _crcKafka.ComputeHash(_data, 0, _data.Length);
        }

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(
                    new Job
                        {
                            Platform = Platform.X64,
                            Jit = Jit.LegacyJit,
                            LaunchCount = 1,
                            WarmupCount = 3,
                            TargetCount = 3,
                        });
            }
        }
    }
}