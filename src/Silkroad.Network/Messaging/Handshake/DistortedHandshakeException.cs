namespace Silkroad.Network.Messaging.Handshake;

public class DistortedHandshakeException : Exception {
    public override string Message => "An attempt to distort the handshake process has been detected.";
}