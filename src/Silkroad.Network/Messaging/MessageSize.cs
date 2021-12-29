using System.Runtime.InteropServices;

namespace Silkroad.Network.Messaging {
    /// <summary>
    ///     Specify a masked <see cref="Message" /> size.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = sizeof(ushort))]
    public struct MessageSize : IEquatable<MessageSize> {
        /// <summary>
        ///     The raw masked value.
        /// </summary>
        public ushort Value { get; set; }

        /// <summary>
        ///     The unmasked data size.
        /// </summary>
        public ushort DataSize {
            get => (ushort) ((this.Value & SizeMask) >> SizeOffset);
            set => this.Value = (ushort) ((this.Value & ~SizeMask) | ((value << SizeOffset) & SizeMask));
        }

        /// <summary>
        ///     The encryption flag.
        /// </summary>
        public bool Encrypted {
            get => Convert.ToBoolean((this.Value & EncryptedMask) >> EncryptedOffset);
            set => this.Value = (ushort) ((this.Value & ~EncryptedMask) |
                                          ((Convert.ToByte(value) << EncryptedOffset) & EncryptedMask));
        }

        public bool Equals(MessageSize other) {
            return this.Value == other.Value;
        }

        public override string ToString() {
            return $"[{this.DataSize} bytes] {(this.Encrypted ? "[Encrypted]" : "")}";
        }

        public override bool Equals(object? obj) {
            return obj is MessageSize other && this.Equals(other);
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return this.Value.GetHashCode();
        }

        #region Bit Masking

        // (MSB)                                                                      (LSB)
        // | 15 | 14 | 13 | 12 | 11 | 10 | 09 | 08 | 07 | 06 | 05 | 04 | 03 | 02 | 01 | 00 |
        // | E* |                                   SIZE                                   |
        //
        // E* = EncryptionFlag

        private const int SizeSize = 15;
        private const int SizeOffset = 0;
        private const ushort SizeMask = ((1 << SizeSize) - 1) << SizeOffset;

        private const int EncryptedSize = 1;
        private const int EncryptedOffset = SizeOffset + SizeSize;
        private const ushort EncryptedMask = ((1 << EncryptedSize) - 1) << EncryptedOffset;

        #endregion Bit Masking
    }
}