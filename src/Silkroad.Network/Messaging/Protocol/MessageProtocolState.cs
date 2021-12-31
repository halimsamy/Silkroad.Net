namespace Silkroad.Network.Messaging.Protocol; 

/// <summary>
///     Determinate the status of the protocol.
/// </summary>
public enum MessageProtocolState : byte {
    /// <summary>
    ///     Indicates a not ready yet status.
    /// </summary>
    None,

    /// <summary>
    ///     Indicates waiting for setting up the Handshake with the pair.
    /// </summary>
    WaitSetup,

    /// <summary>
    ///     Indicates waiting for challenging the pair.
    /// </summary>
    WaitChallenge,

    /// <summary>
    ///     Indicates waiting for the pair to accept the challenge.
    /// </summary>
    WaitAccept,

    /// <summary>
    ///     Indicates a complete and ready to use protocol.
    /// </summary>
    Completed
}