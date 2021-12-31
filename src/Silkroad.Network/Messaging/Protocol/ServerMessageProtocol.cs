namespace Silkroad.Network.Messaging.Protocol; 

/// <summary>
///     Implements a Silkroad Server messaging protocol.
/// </summary>
internal class ServerMessageProtocol : MessageProtocol {
    public ServerMessageProtocol() {
        this.State = MessageProtocolState.None;
        this.Option = MessageProtocolOption.Default;
    }

    protected override void Validate(Message msg) {
        if (!this.Option.HasFlag(MessageProtocolOption.Checksum)) return;

        if (msg.Sequence != this.Sequence.Next())
            throw new InvalidMessageException(InvalidMessageReason.InvalidSequence);

        var msgCrc = msg.CRC;
        msg.CRC = 0;

        if (msgCrc != this.Crc.Compute(msg.AsSpan()))
            throw new InvalidMessageException(InvalidMessageReason.InvalidCRC);
    }

    protected override void Sign(Message msg) {
        if (!this.Option.HasFlag(MessageProtocolOption.Checksum)) return;

        // Only the client signs the message, the server just validate them
        // to ensure that there is no third-party in the connection.
        msg.Sequence = 0;
        msg.CRC = 0;
    }
}