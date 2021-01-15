using System;
using System.Threading.Tasks;

namespace Silkroad.Network.Messaging {
    /// <summary>
    ///     A <see cref="Message" /> service endpoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MessageServiceAttribute : Attribute {
        public readonly ushort Opcode;

        public MessageServiceAttribute(ushort opcode) {
            this.Opcode = opcode;
        }
    }
}