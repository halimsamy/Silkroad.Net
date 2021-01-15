using System;
using System.Runtime.InteropServices;

namespace Silkroad.Network.Messaging {
    /// <summary>
    ///     Specify a unique <see cref="Message" /> ID, used to identify messages.
    ///     This acts like HTTP response codes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = sizeof(ushort))]
    // ReSharper disable once InconsistentNaming
    public struct MessageID : IEquatable<MessageID> {
        /// <summary>
        ///     Initializes a MessageID with a full specific ID (a.k.a Opcode).
        /// </summary>
        /// <param name="value">The ID value.</param>
        public MessageID(ushort value) {
            this.Value = value;
        }

        /// <summary>
        ///     Initializes a MessageID with it's raw parts.
        /// </summary>
        /// <param name="dir">The message direction.</param>
        /// <param name="type">The message type.</param>
        /// <param name="op">The message operation code.</param>
        public MessageID(MessageDirection dir, MessageType type, ushort op) {
            this.Value = 0;
            this.Direction = dir;
            this.Type = type;
            this.Operation = op;
        }

        /// <summary>
        ///     The full value of the ID (a.k.a. Opcode).
        /// </summary>
        public ushort Value { get; set; }

        /// <summary>
        ///     The direction of the <see cref="Message" />.
        /// </summary>
        public MessageDirection Direction {
            get => (MessageDirection) ((this.Value & DirectionMask) >> DirectionOffset);
            set => this.Value = (ushort) ((this.Value & ~DirectionMask) |
                                          ((Convert.ToByte(value) << DirectionOffset) & DirectionMask));
        }

        /// <summary>
        ///     The type of the <see cref="Message" />.
        /// </summary>
        public MessageType Type {
            get => (MessageType) ((this.Value & TypeMask) >> TypeOffset);
            set => this.Value =
                (ushort) ((this.Value & ~TypeMask) | ((Convert.ToByte(value) << TypeOffset) & TypeMask));
        }

        /// <summary>
        ///     The operation of the <see cref="Message" />.
        /// </summary>
        public ushort Operation {
            // TODO: enum this value.
            get => (ushort) ((this.Value & OperationMask) >> OperationOffset);
            set => this.Value = (ushort) ((this.Value & ~OperationMask) | ((value << OperationOffset) & OperationMask));
        }


        public bool Equals(MessageID other) {
            return this.Value == other.Value;
        }

        public override string ToString() {
            return $"[{this.Value:X4}] [{this.Direction}] [{this.Type}] [{this.Operation:X4}]";
        }

        public override bool Equals(object obj) {
            return obj is MessageID other && this.Equals(other);
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return this.Value.GetHashCode();
        }

        #region Bit Masking

        // (MSB)                                                                       (LSB)
        // | 15 | 14 | 13 | 12 | 11 | 10 | 09 | 08 | 07 | 06 | 05 | 04 | 03 | 02 | 01 | 00 |
        // |   DIR   |   TYPE  |                       OPERATION                           |

        private const int OperationSize = 12;
        private const int OperationOffset = 0;
        private const ushort OperationMask = ((1 << OperationSize) - 1) << OperationOffset;

        private const int TypeSize = 2;
        private const int TypeOffset = OperationOffset + OperationSize;
        private const ushort TypeMask = ((1 << TypeSize) - 1) << TypeOffset;

        private const int DirectionSize = 2;
        private const int DirectionOffset = TypeOffset + TypeSize;
        private const ushort DirectionMask = ((1 << DirectionSize) - 1) << DirectionOffset;

        #endregion Bit Masking
    }
}