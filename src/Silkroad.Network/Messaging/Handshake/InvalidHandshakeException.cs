namespace Silkroad.Network.Messaging.Handshake {
    public class InvalidHandshakeException : Exception {
        public override string Message =>
            "Failed to complete the handshake process, the remote signature was not correct.";
    }
}