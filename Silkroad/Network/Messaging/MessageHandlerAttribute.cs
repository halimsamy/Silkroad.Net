using System;

namespace Silkroad.Network.Messaging {
    /// <summary>
    ///     A <see cref="Message" /> service endpoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MessageHandlerAttribute : Attribute {
        public readonly ushort ID;

        public MessageHandlerAttribute(ushort id) {
            this.ID = id;
        }
    }
}