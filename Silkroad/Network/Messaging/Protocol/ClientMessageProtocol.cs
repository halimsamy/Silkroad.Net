using System;

namespace Silkroad.Network.Messaging.Protocol {
    /// <summary>
    ///     Implements a Silkroad Client messaging protocol.
    /// </summary>
    internal class ClientMessageProtocol : MessageProtocol {
        /// <summary>
        ///     Initializes a new Client messaging protocol.
        /// </summary>
        public ClientMessageProtocol() {
            this.Option = MessageProtocolOption.None;
            this.State = MessageProtocolState.WaitSetup;
        }

        protected override void Validate(Message msg) {
            if (!this.Option.HasFlag(MessageProtocolOption.Checksum)) {
                return;
            }

            // Only the client signs the message, the server just validate them
            // to ensure that there is no third-party in the connection.
            // but we can also count on that to validate the messages coming from
            // the server as well, this won't harm anyway. 
            if (msg.Sequence != 0 || msg.CRC != 0) {
                throw new InvalidOperationException();
            }
        }

        protected override void Sign(Message msg) {
            if (!this.Option.HasFlag(MessageProtocolOption.Checksum)) {
                return;
            }

            msg.Sequence = this.Sequence.Next();
            msg.CRC = this.Crc.Compute(msg.AsSpan());
        }
    }
}