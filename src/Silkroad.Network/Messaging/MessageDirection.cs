namespace Silkroad.Network.Messaging;

/// <summary>
///     Part of the <see cref="MessageID" /> that indicates the direction of the <see cref="Message" />
/// </summary>
[Flags]
public enum MessageDirection : byte {
    /// <summary>
    ///     Indicates a one way <see cref="Message" /> (or doesn't have specific direction).
    /// </summary>
    NuN = 0,

    /// <summary>
    ///     Indicates a client request <see cref="Message" />.
    /// </summary>
    Req = 1,

    /// <summary>
    ///     Indicates a server acknowledge <see cref="Message" />.
    /// </summary>
    Ack = 2
}