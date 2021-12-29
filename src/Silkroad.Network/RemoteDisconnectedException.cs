namespace Silkroad.Network {
    public class RemoteDisconnectedException : Exception {
        public override string Message =>
            "You have been disconnected, The connection was forcibly closed by the remote.";
    }
}