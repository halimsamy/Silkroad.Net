namespace Silkroad.Network.Messaging; 

/// <summary>
///     Part of the <see cref="MessageID" /> that indicates what this <see cref="Message" /> is used for.
/// </summary>
public enum MessageType : byte {
    /// <summary>
    ///     Indicates a not used message. (I have never seen that.)
    /// </summary>
    None = 0,

    /// <summary>
    ///     Indicates a special protocol <see cref="Message" />.
    ///     Usually those are HANDSHAKE (0x5000) and HANDSHAKE_ACCEPT (0x9000) and FILE_CHUNK (0x1001).
    ///     I have never seen any other <see cref="Message" /> that use this type.
    /// </summary>
    NetEngine = 1,

    /// <summary>
    ///     Indicates a framework <see cref="Message" />.
    ///     Usually those are GatewayServer/DownloadServer messages,
    ///     as well as messages that is used internally between modules in the server.
    /// </summary>
    Framework = 2,

    /// <summary>
    ///     Indicates a game <see cref="Message" />.
    ///     Usually those are AgentServer/GameServer messages. (e.g. movement, action, etc...)
    /// </summary>
    GameWorld = 3
}