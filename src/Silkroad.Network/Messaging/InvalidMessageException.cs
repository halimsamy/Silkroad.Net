namespace Silkroad.Network.Messaging; 

public class InvalidMessageException : Exception {
    public readonly InvalidMessageReason Reason;

    public InvalidMessageException(InvalidMessageReason reason) {
        this.Reason = reason;
    }

    public override string Message => $"An invalid message was received: {this.Reason}.";
}