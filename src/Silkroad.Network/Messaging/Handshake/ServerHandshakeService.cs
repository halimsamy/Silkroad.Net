using System.Security.Cryptography;
using Silkroad.Cryptography;
using Silkroad.Network.Messaging.Protocol;

namespace Silkroad.Network.Messaging.Handshake; 

/// <summary>
///     Implements Silkroad Server Handshake process.
/// </summary>
public class ServerHandshakeService {
    private readonly byte[] _key = new byte[sizeof(ulong)];
    private uint _generator;
    private uint _localPrivate;
    private uint _localPublic;
    private uint _prime;

    public async Task Begin(Session session) {
        var protocol = session.Protocol;
        if (protocol.State != MessageProtocolState.None) throw new DistortedHandshakeException();

        var msg = new Message(MessageID.HANDSHAKE, 25);
        msg.Write(protocol.Option);

        if (protocol.Option.HasFlag(MessageProtocolOption.Encryption)) {
            var key = new byte[sizeof(ulong)];
            RandomNumberGenerator.Fill(key.AsSpan());

            protocol.Blowfish = new Blowfish(key.AsSpan());
            msg.Write<byte>(key.AsSpan());
        }

        if (protocol.Option.HasFlag(MessageProtocolOption.Checksum)) {
            var seqSeed = (uint)RandomNumberGenerator.GetInt32(byte.MaxValue);
            var crcSeed = (uint)RandomNumberGenerator.GetInt32(byte.MaxValue);

            protocol.Sequence = new MessageSequence(seqSeed);
            protocol.Crc = new MessageCRC(crcSeed);

            msg.Write(seqSeed);
            msg.Write(crcSeed);
        }

        if (protocol.Option.HasFlag(MessageProtocolOption.KeyExchange)) {
            RandomNumberGenerator.Fill(this._key.AsSpan());
            this._localPrivate = (uint)RandomNumberGenerator.GetInt32(int.MaxValue);
            this._generator = (uint)RandomNumberGenerator.GetInt32(1, int.MaxValue);
            this._prime = (uint)RandomNumberGenerator.GetInt32(1, int.MaxValue);
            this._localPublic = HandshakeHelpers.PowMod(this._generator, this._localPrivate, this._prime);

            msg.Write<byte>(this._key);
            msg.Write(this._generator);
            msg.Write(this._prime);
            msg.Write(this._localPublic);

            protocol.State = MessageProtocolState.WaitChallenge;
        }
        else {
            protocol.State = MessageProtocolState.WaitAccept;
        }

        if (protocol.Option.HasFlag(MessageProtocolOption.KeyChallenge))
            protocol.State = MessageProtocolState.WaitChallenge;

        await session.SendAsync(msg);
    }

    [MessageHandler(MessageID.HANDSHAKE)]
    public Task Handshake(Session session, Message msg) {
        var protocol = session.Protocol;
        if (protocol.State != MessageProtocolState.WaitChallenge) throw new DistortedHandshakeException();

        var remotePublic = msg.Read<uint>();
        var remoteChallenge = new byte[sizeof(ulong)].AsSpan();
        msg.Read(remoteChallenge);

        var commonSecret = HandshakeHelpers.PowMod(remotePublic, this._localPrivate, this._prime);

        var key = HandshakeHelpers.GetKey(this._localPublic, remotePublic);
        HandshakeHelpers.KeyTransformValue(key.AsSpan(), commonSecret, (byte)(commonSecret & 3));
        protocol.Blowfish = new Blowfish(key.AsSpan());

        protocol.Blowfish.Decrypt(remoteChallenge);

        var expected = HandshakeHelpers.GetKey(remotePublic, this._localPublic).AsSpan();
        HandshakeHelpers.KeyTransformValue(expected, commonSecret, (byte)(remotePublic & 7));

        if (!remoteChallenge.SequenceEqual(expected)) throw new InvalidHandshakeException();

        var challenge = HandshakeHelpers.GetKey(this._localPublic, remotePublic).AsSpan();
        HandshakeHelpers.KeyTransformValue(challenge, commonSecret, (byte)(this._localPublic & 7));
        protocol.Blowfish.Encrypt(challenge);

        HandshakeHelpers.KeyTransformValue(this._key.AsSpan(), commonSecret, 3);
        protocol.Blowfish = new Blowfish(this._key.AsSpan());

        var res = new Message(MessageID.HANDSHAKE, 9);
        res.Write(MessageProtocolOption.KeyChallenge);
        res.Write<byte>(challenge);
        protocol.State = MessageProtocolState.WaitAccept;

        return session.SendAsync(res);
    }

    [MessageHandler(MessageID.HANDSHAKE_ACCEPT)]
    public Task HandshakeAccept(Session session, Message msg) {
        var protocol = session.Protocol;
        if (protocol.State != MessageProtocolState.WaitAccept) throw new DistortedHandshakeException();

        protocol.State = MessageProtocolState.Completed;

        return Task.CompletedTask;
    }
}