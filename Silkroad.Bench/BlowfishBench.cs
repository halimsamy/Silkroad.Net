using System;
using BenchmarkDotNet.Attributes;
using Silkroad.Security;

namespace Silkroad.Bench {
    [MemoryDiagnoser]
    public class BlowfishBench {
        private readonly Blowfish _blowfish;
        private readonly byte[] _data;

        public BlowfishBench() {
            this._data = new byte[] {0x0F, 0x07, 0x3D, 0x20, 0x56, 0x62, 0xC9, 0xEB};
            this._blowfish = new Blowfish(new byte[] {0x0F, 0x07, 0x3D, 0x20, 0x56, 0x62, 0xC9, 0xEB}.AsSpan());
        }

        [Benchmark]
        public void Encrypt() {
            this._blowfish.Encrypt(this._data.AsSpan());
        }

        [Benchmark]
        public void Decrypt() {
            this._blowfish.Decrypt(this._data.AsSpan());
        }
    }
}