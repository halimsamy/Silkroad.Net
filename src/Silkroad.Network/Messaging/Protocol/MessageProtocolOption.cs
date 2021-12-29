namespace Silkroad.Network.Messaging.Protocol {
    /// <summary>
    ///     Determinate what options are enabled in the protocol.
    /// </summary>
    [Flags]
    public enum MessageProtocolOption : byte {
        /// <summary>
        ///     This should never acquire.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Options are disabled for protocol.
        /// </summary>
        Disable = 1,

        /// <summary>
        ///     Indicates that Encryption are enabled for protocol.
        /// </summary>
        Encryption = 2,

        /// <summary>
        ///     Indicates that CRC and Sequence checksum are enabled for protocol.
        /// </summary>
        Checksum = 4,

        /// <summary>
        ///     Indicates that Handshake are enabled for protocol.
        /// </summary>
        KeyExchange = 8,

        /// <summary>
        ///     Indicates that Handshake Challenging are enabled for protocol.
        /// </summary>
        KeyChallenge = 16,
        
        /// <summary>
        ///     The server default protocol options.
        /// </summary>
        Default = Encryption | Checksum | KeyExchange
    }
}