using Silkroad.Cryptography;

namespace Silkroad.Network.Messaging.Protocol;

/// <summary>
///     Implements a Silkroad messaging protocol.
/// </summary>
internal abstract class MessageProtocol {
    /// <summary>
    ///     The Blowfish used for encryption.
    /// </summary>
    internal Blowfish Blowfish = null!;

    /// <summary>
    ///     The CRC used for computing checksum.
    /// </summary>
    internal MessageCRC Crc = null!;

    /// <summary>
    ///     The protocol option.
    /// </summary>
    internal MessageProtocolOption Option;

    /// <summary>
    ///     The Sequence used for sequencing the messages.
    /// </summary>
    internal MessageSequence Sequence = null!;

    /// <summary>
    ///     The protocol state.
    /// </summary>
    internal MessageProtocolState State = MessageProtocolState.None;

    /// <summary>
    ///     Indicate if the Handshake process is done, and this protocol ready to process other messages.
    /// </summary>
    internal bool Ready => this.State == MessageProtocolState.Completed;

    /// <summary>
    ///     Validates a <see cref="Message" /> using its CRC and Sequence.
    /// </summary>
    /// <param name="msg">The message to validate.</param>
    protected abstract void Validate(Message msg);

    /// <summary>
    ///     Signs a <see cref="Message" /> by setting its CRC and Sequence.
    /// </summary>
    /// <param name="msg">The message to sign.</param>
    protected abstract void Sign(Message msg);

    /// <summary>
    ///     Decodes a raw buffer into a ready to use <see cref="Message" />.
    /// </summary>
    /// <param name="size">The 2 bytes masked message size from the raw buffer.</param>
    /// <param name="buffer">The remaining message raw buffer.</param>
    /// <returns>The decoded ready to use <see cref="Message" />.</returns>
    /// <exception cref="InvalidOperationException">The message was interrupted or injected (checksum failed).</exception>
    internal Message Decode(MessageSize size, Span<byte> buffer) {
        if (size.Encrypted && this.Option.HasFlag(MessageProtocolOption.Encryption)) this.Blowfish.Decrypt(buffer);

        var msg = new Message(size, buffer);
        this.Validate(msg);

        return msg;
    }

    /// <summary>
    ///     Encodes a <see cref="Message" /> into a raw ready to send buffer.
    /// </summary>
    /// <param name="msg">The message to be encoded.</param>
    /// <returns>The raw ready to send buffer.</returns>
    internal Memory<byte> Encode(Message msg) {
        this.Sign(msg);

        if (msg.Encrypted && this.Option.HasFlag(MessageProtocolOption.Encryption)) {
            msg.Resize((ushort)(Message.EncryptOffset + Blowfish.GetOutputLength(Message.EncryptSize + msg.Size)));
            this.Blowfish.Encrypt(msg.AsSpan()[Message.EncryptOffset..]);
        }

        return msg.AsMemory();
    }
}