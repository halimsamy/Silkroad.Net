using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Silkroad.Network.Messaging.Protocol;
using Silkroad.Security;

namespace Silkroad.Network.Messaging.Handshake {
    /// <summary>
    ///     Implements Silkroad Client Handshake process.
    /// </summary>
    public class ClientHandshakeService {
        private readonly byte[] _key = new byte[sizeof(ulong)];
        private uint _commonSecret;
        private uint _localPublic;
        private uint _remotePublic;

        [MessageHandler(Opcodes.HANDSHAKE)]
        public Task Handshake(Session session, Message msg) {
            var protocol = session.Protocol;

            var opt = msg.Read<MessageProtocolOption>();
            if (protocol.Option == MessageProtocolOption.None) {
                protocol.Option = opt;
            }

            if (opt.HasFlag(MessageProtocolOption.Encryption)) {
                var key = new byte[sizeof(ulong)];
                msg.Read(key.AsSpan());
                protocol.Blowfish = new Blowfish(key.AsSpan());
            }

            if (opt.HasFlag(MessageProtocolOption.Checksum)) {
                protocol.Sequence = new MessageSequence(msg.Read<uint>());
                protocol.Crc = new MessageCRC(msg.Read<uint>());
            }

            if (opt.HasFlag(MessageProtocolOption.KeyExchange)) {
                return session.SendAsync(this.Setup(session, msg));
            }

            if (opt.HasFlag(MessageProtocolOption.KeyChallenge)) {
                return session.SendAsync(this.Challenge(session, msg));
            }

            // Make sure we aren't in a state to accept handshake-less connections.
            if (protocol.State != MessageProtocolState.None) {
                throw new DistortedHandshakeException();
            }

            protocol.State = MessageProtocolState.Completed;
            return Task.CompletedTask;
        }

        [MessageHandler(Opcodes.HANDSHAKE_ACCEPT)]
        public Task HandshakeAccept(Session session, Message msg) {
            throw new DistortedHandshakeException();
        }

        private Message Setup(Session session, Message msg) {
            var protocol = session.Protocol;
            if (protocol.State != MessageProtocolState.WaitSetup) {
                throw new DistortedHandshakeException();
            }

            msg.Read(this._key.AsSpan());
            var generator = msg.Read<uint>();
            var prime = msg.Read<uint>();
            this._localPublic = msg.Read<uint>();

            var localPrivate = (uint) RandomNumberGenerator.GetInt32(1, int.MaxValue) & int.MaxValue;
            this._remotePublic = HandshakeHelpers.PowMod(generator, localPrivate, prime);
            this._commonSecret = HandshakeHelpers.PowMod(this._localPublic, localPrivate, prime);

            var key = HandshakeHelpers.GetKey(this._localPublic, this._remotePublic).AsSpan();
            HandshakeHelpers.KeyTransformValue(key, this._commonSecret, (byte) (this._commonSecret & 3));
            protocol.Blowfish = new Blowfish(key);

            var localChallenge = HandshakeHelpers.GetKey(this._remotePublic, this._localPublic).AsSpan();
            HandshakeHelpers.KeyTransformValue(localChallenge, this._commonSecret, (byte) (this._remotePublic & 7));
            protocol.Blowfish.Encrypt(localChallenge);

            var res = new Message(Opcodes.HANDSHAKE, sizeof(uint) + sizeof(ulong));
            res.Write(this._remotePublic);
            res.Write<byte>(localChallenge);

            protocol.State = MessageProtocolState.WaitChallenge;
            return res;
        }

        private Message Challenge(Session session, Message msg) {
            var protocol = session.Protocol;
            if (protocol.State != MessageProtocolState.WaitChallenge) {
                throw new DistortedHandshakeException();
            }

            var remoteChallenge = new byte[sizeof(ulong)].AsSpan();
            msg.Read(remoteChallenge);

            var expected = HandshakeHelpers.GetKey(this._localPublic, this._remotePublic).AsSpan();
            HandshakeHelpers.KeyTransformValue(expected, this._commonSecret, (byte) (this._localPublic & 7));
            protocol.Blowfish.Encrypt(expected);

            if (!remoteChallenge.SequenceEqual(expected)) {
                throw new InvalidHandshakeException();
            }

            HandshakeHelpers.KeyTransformValue(this._key.AsSpan(), this._commonSecret, 3);
            protocol.Blowfish = new Blowfish(this._key.AsSpan());

            protocol.State = MessageProtocolState.Completed;
            return new Message(Opcodes.HANDSHAKE_ACCEPT);
        }
    }
}