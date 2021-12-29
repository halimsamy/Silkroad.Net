using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Silkroad.Network.Messaging {
    public class Message {
        /// <summary>
        ///     The underlying internal buffer.
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        ///     Initializes a new Message.
        /// </summary>
        /// <param name="id">The message unique ID.</param>
        /// <param name="encrypted">Indicates whether the message is encrypted.</param>
        /// <param name="massive">Indicates whether the message should be warped into MASSIVE message.</param>
        /// <param name="capacity">The pre-allocated capacity.</param>
        /// <exception cref="NotImplementedException">A message cannot be both encrypted and massive.</exception>
        public Message(MessageID id, bool encrypted, bool massive = false, ushort capacity = 0) {
            if (encrypted && massive) {
                throw new NotImplementedException();
            }

            this._buffer = new byte[HeaderSize + capacity];
            this.Position = DataOffset;
            this.ID = id;
            this.Encrypted = encrypted;
            this.Massive = massive;
        }

        /// <summary>
        ///     Initializes a new Message that is not encrypted nor massive.
        /// </summary>
        /// <param name="id">The message unique ID.</param>
        /// <param name="capacity">The pre-allocated capacity.</param>
        /// <exception cref="NotImplementedException">A message cannot be both encrypted and massive.</exception>
        public Message(MessageID id, ushort capacity = 0)
            : this(id, false, false, capacity) {
        }

        /// <summary>
        ///     Initializes a new Message.
        /// </summary>
        /// <param name="id">The message unique ID (a.k.a Opcode).</param>
        /// <param name="encrypted">Indicates whether the message is encrypted.</param>
        /// <param name="massive">Indicates whether the message should be warped into MASSIVE message.</param>
        /// <param name="capacity">The pre-allocated capacity.</param>
        /// <exception cref="NotImplementedException">A message cannot be both encrypted and massive.</exception>
        public Message(ushort id, bool encrypted, bool massive = false, ushort capacity = 0)
            : this(new MessageID(id), encrypted, massive, capacity) {
        }

        /// <summary>
        ///     Initializes a new Message that is not encrypted nor massive.
        /// </summary>
        /// <param name="id">The message unique ID (a.k.a Opcode).</param>
        /// <param name="capacity">The pre-allocated capacity.</param>
        /// <exception cref="NotImplementedException">A message cannot be both encrypted and massive.</exception>
        public Message(ushort id, ushort capacity = 0)
            : this(new MessageID(id), false, false, capacity) {
        }

        internal Message(MessageSize size, Span<byte> buffer) {
            this._buffer = new byte[HeaderSize + size.DataSize];
            this.Position = SizeOffset;
            this.Write(size);
            this.Write<byte>(buffer.Slice(0, EncryptSize + size.DataSize));
            this.Position = DataOffset;
        }

        /// <summary>
        ///     The message data size.
        /// </summary>
        public ushort Size {
            get {
                lock (this._buffer) {
                    var size = MemoryMarshal.Read<MessageSize>(this._buffer.AsSpan(SizeOffset,
                        Unsafe.SizeOf<MessageSize>()));
                    return size.DataSize;
                }
            }
            private set {
                lock (this._buffer) {
                    ref var size =
                        ref MemoryMarshal.AsRef<MessageSize>(this._buffer.AsSpan(SizeOffset,
                            Unsafe.SizeOf<MessageSize>()));
                    size.DataSize = value;
                }
            }
        }

        /// <summary>
        ///     Indicates whether the message is encrypted.
        /// </summary>
        public bool Encrypted {
            get {
                lock (this._buffer) {
                    var size = MemoryMarshal.Read<MessageSize>(this._buffer.AsSpan(SizeOffset,
                        Unsafe.SizeOf<MessageSize>()));
                    return size.Encrypted;
                }
            }
            private init {
                lock (this._buffer) {
                    ref var size =
                        ref MemoryMarshal.AsRef<MessageSize>(this._buffer.AsSpan(SizeOffset,
                            Unsafe.SizeOf<MessageSize>()));
                    size.Encrypted = value;
                }
            }
        }

        /// <summary>
        ///     Indicates whether the message should be warped into MASSIVE message.
        /// </summary>
        public bool Massive { get; }

        /// <summary>
        ///     The message ID, used to identify messages.
        /// </summary>
        // ReSharper disable InconsistentNaming
        public MessageID ID {
            get {
                lock (this._buffer) {
                    var id = MemoryMarshal.Read<MessageID>(this._buffer.AsSpan(IDOffset,
                        Unsafe.SizeOf<MessageID>()));
                    return id;
                }
            }
            set {
                lock (this._buffer) {
                    ref var id =
                        ref MemoryMarshal.AsRef<MessageID>(this._buffer.AsSpan(IDOffset,
                            Unsafe.SizeOf<MessageID>()));
                    id = value;
                }
            }
        }

        /// <summary>
        ///     The number Sequence.
        /// </summary>
        internal byte Sequence {
            get {
                lock (this._buffer) {
                    return this._buffer[SequenceOffset];
                }
            }
            set {
                lock (this._buffer) {
                    this._buffer[SequenceOffset] = value;
                }
            }
        }

        /// <summary>
        ///     The CRC checksum.
        /// </summary>
        // ReSharper disable InconsistentNaming
        internal byte CRC {
            get {
                lock (this._buffer) {
                    return this._buffer[CRCOffset];
                }
            }
            set {
                lock (this._buffer) {
                    this._buffer[CRCOffset] = value;
                }
            }
        }

        /// <summary>
        ///     The capacity allocated for the message without counting the HeaderSize.
        /// </summary>
        public ushort Capacity {
            get {
                lock (this._buffer) {
                    return (ushort) (this._buffer.Length - HeaderSize);
                }
            }
        }

        /// <summary>
        ///     The underlying position for reading/writing from/into the current message.
        ///     This is dangerous to mutate, as it doesn't check where you lead the position.
        ///     And this may cause exceptions or unexpected behaviors when reading/writing.
        /// </summary>
        public ushort Position { get; set; }

        public override string ToString() {
            lock (this._buffer) {
                const int bytesPerLine = 16;
                var output = new StringBuilder();
                var asciiOutput = new StringBuilder();
                var length = this._buffer.Length - HeaderSize;
                if (length % bytesPerLine != 0) {
                    length += bytesPerLine - length % bytesPerLine;
                }

                output.Append(
                    $"{this.ID} [{this.Size:D4} bytes]{(this.Encrypted ? " [Encrypted]" : "")}{(this.Massive ? " [Massive]" : "")} {Environment.NewLine}");
                for (var x = 0; x <= length; ++x) {
                    if (x % bytesPerLine == 0) {
                        if (x > 0) {
                            output.Append($"  {asciiOutput}{Environment.NewLine}");
                            asciiOutput.Clear();
                        }

                        if (x != length) {
                            output.Append($"{x:d10}   ");
                        }
                    }

                    if (x < this._buffer.Length - HeaderSize) {
                        output.Append($"{this._buffer[x + HeaderSize]:X2} ");
                        var ch = (char) this._buffer[x + HeaderSize];
                        if (!char.IsControl(ch)) {
                            asciiOutput.Append($"{ch}");
                        } else {
                            asciiOutput.Append(".");
                        }
                    } else {
                        output.Append("   ");
                        asciiOutput.Append(".");
                    }
                }

                return output.ToString();
            }
        }

        /// <summary>
        ///     Forms a slice out of the current message including the header.
        /// </summary>
        /// <returns></returns>
        public Memory<byte> AsMemory() {
            lock (this._buffer) {
                return this._buffer.AsMemory();
            }
        }

        /// <summary>
        ///     Forms a slice out of the current message message data, without including the header.
        /// </summary>
        /// <returns></returns>
        public Memory<byte> AsDataMemory() {
            lock (this._buffer) {
                return this._buffer.AsMemory(DataOffset);
            }
        }

        /// <summary>
        ///     Forms a slice out of the current message including the header.
        /// </summary>
        /// <returns></returns>
        public Span<byte> AsSpan() {
            lock (this._buffer) {
                return this._buffer.AsSpan();
            }
        }

        /// <summary>
        ///     Forms a slice out of the current message data, without including the header.
        /// </summary>
        /// <returns></returns>
        public Span<byte> AsDataSpan() {
            lock (this._buffer) {
                return this._buffer.AsSpan(DataOffset);
            }
        }

        /// <summary>
        ///     Reads a value from a message.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The read value.</returns>
        public T Read<T>() where T : unmanaged {
            lock (this._buffer) {
                var size = Unsafe.SizeOf<T>();
                var value = MemoryMarshal.Read<T>(this._buffer.AsSpan(this.Position, size));
                this.Position += (ushort) size;
                return value;
            }
        }

        /// <summary>
        ///     Reads a into a <see cref="Span{T}" /> from a message until the span is full.
        /// </summary>
        /// <param name="span">The span to read into</param>
        /// <typeparam name="T">The span inner item type</typeparam>
        public void Read<T>(in Span<T> span) where T : unmanaged {
            lock (this._buffer) {
                var bytesSpan = MemoryMarshal.AsBytes(span);
                var size = bytesSpan.Length;
                this._buffer.AsSpan(this.Position, size).CopyTo(bytesSpan);
                this.Position += (ushort) size;
            }
        }

        /// <summary>
        ///     Reads a string with a specific encoding from a message.
        /// </summary>
        /// <param name="encoding">The string encoding.</param>
        /// <returns>The read ASCII string.</returns>
        public string Read(Encoding encoding) {
            lock (this._buffer) {
                var length = this.Read<ushort>();
                var str = encoding.GetString(this._buffer.AsSpan(this.Position, length));
                this.Position += length;
                return str;
            }
        }

        /// <summary>
        ///     Reads an ASCII encoded string from a message.
        /// </summary>
        /// <returns>The read ASCII string.</returns>
        public string Read() {
            return this.Read(Encoding.ASCII);
        }

        /// <summary>
        ///     Writes a ref value into a message, growing it as much as necessary.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <typeparam name="T">The value type</typeparam>
        public void Write<T>(ref T value) where T : unmanaged {
            lock (this._buffer) {
                var size = Unsafe.SizeOf<T>();
                if (this.Position + size > this._buffer.Length) {
                    Array.Resize(ref this._buffer, this.Position + size);
                }

                MemoryMarshal.Write(this._buffer.AsSpan(this.Position, size), ref value);
                this.Position += (ushort) size;
                this.Size = (ushort) (this._buffer.Length - HeaderSize);
            }
        }

        /// <summary>
        ///     Writes a value into a message, growing it as much as necessary.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <typeparam name="T">The value type.</typeparam>
        public void Write<T>(T value) where T : unmanaged {
            this.Write(ref value);
        }

        /// <summary>
        ///     Writes a <see cref="Span{T}" /> into a message, growing it as much as necessary.
        /// </summary>
        /// <param name="span">The span to write.</param>
        /// <typeparam name="T">The inner span item type.</typeparam>
        public void Write<T>(in ReadOnlySpan<T> span) where T : unmanaged {
            lock (this._buffer) {
                var bytesSpan = MemoryMarshal.AsBytes(span);
                var size = bytesSpan.Length;
                if (this.Position + size > this._buffer.Length) {
                    Array.Resize(ref this._buffer, this.Position + size);
                }

                bytesSpan.CopyTo(this._buffer.AsSpan(this.Position, size));
                this.Position += (ushort) size;
                this.Size = (ushort) (this._buffer.Length - HeaderSize);
            }
        }

        /// <summary>
        ///     Write a string value with a specific encoding into a message, growing it as much as necessary.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="encoding">The string encoding.</param>
        public void Write(string value, Encoding encoding) {
            lock (this._buffer) {
                var bytes = encoding.GetBytes(value);
                var length = (ushort) bytes.Length;
                this.Write(ref length);
                this.Write<byte>(bytes);
            }
        }

        /// <summary>
        ///     Writes an ASCII encoding string into a message, growing it as much as necessary.
        /// </summary>
        /// <param name="value">The string value.</param>
        public void Write(string value) {
            this.Write(value, Encoding.ASCII);
        }

        internal void Resize(ushort size) {
            lock (this._buffer) {
                if (size < HeaderSize) {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }

                Array.Resize(ref this._buffer, size);
            }
        }

        #region Size(s) and Offset(s)

        public const ushort BufferSize = 4096;
        public const ushort HeaderSize = 6;
        public const ushort DataSize = BufferSize - HeaderSize;

        public const ushort HeaderOffset = 0;
        public const ushort SizeOffset = HeaderOffset + 0;
        public const ushort IDOffset = HeaderOffset + 2;
        public const ushort SequenceOffset = HeaderOffset + 4;
        public const ushort CRCOffset = HeaderOffset + 5;
        public const ushort DataOffset = HeaderOffset + HeaderSize;

        public const ushort EncryptOffset = HeaderOffset + 2;
        public const ushort EncryptSize = HeaderSize - EncryptOffset;
        public const ushort EncryptMask = 0x8000;

        #endregion Size(s) and Offset(s)
    }
}