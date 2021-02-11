using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Silkroad.Network.Messaging;
using Silkroad.Network.Messaging.Handshake;
using Silkroad.Network.Messaging.Protocol;
using Silkroad.Security;

namespace Silkroad.Network {
    /// <summary>
    ///     Implements a Silkroad session interface.
    /// </summary>
    public class Session : IDisposable {
        public delegate Task MessageHandler(Session s, Message m);

        /// <summary>
        ///     A list of registered handlers.
        /// </summary>
        private readonly List<Tuple<ushort, MessageHandler>> _handlers = new List<Tuple<ushort, MessageHandler>>();

        /// <summary>
        ///     A list of registered services.
        /// </summary>
        private readonly List<object> _services = new List<object>();

        /// <summary>
        ///     The underlying socket.
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        ///     The Silkroad message protocol.
        /// </summary>
        internal readonly MessageProtocol Protocol;

        /// <summary>
        ///     Initializes a new session, this should be used by clients, as it will setup the client protocol.
        /// </summary>
        public Session() {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Protocol = new ClientMessageProtocol();
            this.RegisterService<ClientHandshakeService>();
        }

        /// <summary>
        ///     Initializes a new session with a giving <see cref="Socket" />, this should be only
        ///     used in servers, as it will setup the server protocol.
        /// </summary>
        /// <param name="socket"></param>
        public Session(Socket socket) {
            this._socket = socket;
            this.Protocol = new ServerMessageProtocol();
            this.RegisterService<ServerHandshakeService>();
        }

        /// <summary>
        ///     Indicates if the session is connected and not closed.
        /// </summary>
        public bool Connected => this._socket.Connected;

        /// <summary>
        ///     Indicates if the Handshake process is done, and the session is ready to use.
        /// </summary>
        public bool Ready => this.Protocol.Ready;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            this.ReleaseUnmanagedResources();
            if (disposing) {
                this._socket?.Dispose();
            }
        }

        ~Session() {
            this.Dispose(false);
        }

        private void ReleaseUnmanagedResources() {
            this._services.Clear();
            this._handlers.Clear();
        }

        /// <summary>
        ///     Registers a service to be invoked when <see cref="RespondAsync" /> is called.
        ///     The same service type cannot be registered twice.
        /// </summary>
        /// <param name="service">The service to be registered.</param>
        /// <typeparam name="T">The service type.</typeparam>
        public void RegisterService<T>(T service) where T : class {
            if (this.FindService<T>() != null) {
                return;
            }

            this._services.Add(service);

            foreach (var (method, attr) in from method in service.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                let attr = method.GetCustomAttribute<MessageHandlerAttribute>()
                where attr != null
                select (method, attr)) {
                var hnd = (MessageHandler) Delegate.CreateDelegate(typeof(MessageHandler), service, method);
                this._handlers.Add(Tuple.Create(attr.Opcode, hnd));
            }
        }

        /// <summary>
        ///     Registers a service with parameter-less constructor to be invoked
        ///     when <see cref="RespondAsync" /> is called. The same service type cannot be registered twice.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        public void RegisterService<T>() where T : class {
            this.RegisterService(Activator.CreateInstance<T>());
        }

        /// <summary>
        ///     Removes a registered service and it's registered handlers.
        /// </summary>
        /// <param name="removeHandlers">Whether to remove the registered handlers that belongs to the service.</param>
        /// <typeparam name="T">The service type.</typeparam>
        public void RemoveService<T>(bool removeHandlers = true) where T : class {
            var service = this.FindService<T>();
            if (service == null) {
                return;
            }

            this._services.Remove(service);

            if (!removeHandlers) {
                return;
            }

            foreach (var handler in this._handlers.Where(x => x.Item2.Target?.GetType() == typeof(T))) {
                this._handlers.Remove(handler);
            }
        }

        /// <summary>
        ///     Finds a service in the registered services.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The requested service, or <c>null</c> if it's registered.</returns>
        public T FindService<T>() where T : class {
            return (T) this._services.FirstOrDefault(s => s.GetType() == typeof(T));
        }

        /// <summary>
        ///     Registers a method handler to be invoked when <see cref="RespondAsync" /> is called. The same handler type cannot
        ///     be registered twice.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        public void RegisterHandler(MessageHandler handler) {
            if (this._handlers.FirstOrDefault(x => x.Item2 == handler) != null) {
                return;
            }

            var attr = handler.GetMethodInfo()?.GetCustomAttribute<MessageHandlerAttribute>();
            if (attr == null) {
                return;
            }

            this._handlers.Add(Tuple.Create(attr.Opcode, handler));
        }

        /// <summary>
        ///     Establishes a connection to a remote host.
        /// </summary>
        /// <param name="ip">The host's IP Address.</param>
        /// <param name="port">The host's Port.</param>
        /// <returns></returns>
        public Task ConnectAsync(string ip, int port) {
            // When attempting to reconnect we have to reset the protocol
            // or we will put it in a invalid status.
            this.Protocol.Option = MessageProtocolOption.None;
            this.Protocol.State = MessageProtocolState.WaitSetup;

            return this._socket.ConnectAsync(ip, port);
        }

        /// <summary>
        ///     Sends a <see cref="Message" /> to a connected session.
        /// </summary>
        /// <param name="msg">The message to be sent.</param>
        /// <returns></returns>
        public async Task SendAsync(Message msg) {
            if (msg.Massive) {
                var size = msg.Size;
                var chunks = (ushort) Math.Ceiling(size / (float) Message.BufferSize);

                var header = new Message(Opcodes.MASSIVE, 5);
                header.Write(true);
                header.Write(chunks);
                header.Write(msg.ID.Value);
                await this._socket.SendAsync(this.Protocol.Encode(header), SocketFlags.None).ConfigureAwait(false);

                for (var i = 0; i < chunks; i++) {
                    var len = Math.Min(Message.BufferSize, size);

                    var chunk = new Message(Opcodes.MASSIVE, (ushort) (len + 1));
                    chunk.Write(false);
                    chunk.Write<byte>(msg.AsDataSpan().Slice(i * Message.BufferSize, len));
                    await this._socket.SendAsync(this.Protocol.Encode(header), SocketFlags.None).ConfigureAwait(false);
                }
            } else {
                await this._socket.SendAsync(this.Protocol.Encode(msg), SocketFlags.None).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Closes the session connection and allow reuse of the underlying socket.
        ///     If the session is already closed, this method would do nothing.
        /// </summary>
        /// <returns></returns>
        public Task DisconnectAsync() {
            return !this._socket.Connected
                ? Task.CompletedTask
                : Task.Factory.FromAsync(this._socket.BeginDisconnect, this._socket.EndDisconnect, true, null);
        }

        /// <summary>
        ///     Receives a complete <see cref="Message" /> from a connected session. This method would not
        ///     close the session if a protocol error acquired, instead it leaves it up to you to catch it
        ///     and handle it the way you like by disconnecting or whatever.
        /// </summary>
        /// <returns>The received message.</returns>
        public async Task<Message> ReceiveAsync() {
            Message massiveMsg = null;
            ushort massiveCount = 0;

            // The loop was meant for receiving a complete MASSIVE message
            // instead of calling the method recursively on itself.
            // because doing so is a bad idea, and performance killer. (Ask Google if you don't know why)
            while (true) {
                var sizeBuffer = new byte[2]; // 2 = Unsafe.SizeOf<MessageSize>()
                await this.ReceiveExactAsync(sizeBuffer.AsMemory(), SocketFlags.None).ConfigureAwait(false);

                var size = MemoryMarshal.Read<MessageSize>(sizeBuffer);
                var remaining = size.Encrypted && this.Protocol.Option.HasFlag(MessageProtocolOption.Encryption)
                    ? Blowfish.GetOutputLength(Message.EncryptSize + size.DataSize)
                    : Message.EncryptSize + size.DataSize;

                var buffer = new byte[remaining];
                await this.ReceiveExactAsync(buffer.AsMemory(), SocketFlags.None).ConfigureAwait(false);

                var msg = this.Protocol.Decode(size, buffer.AsSpan());

                if (msg.ID.Value == Opcodes.MASSIVE) {
                    var isHeader = msg.Read<bool>();

                    if (isHeader) {
                        if (massiveMsg != null) {
                            throw new InvalidMessageException(InvalidMessageReason.Distorted);
                        }

                        massiveCount = msg.Read<ushort>();
                        var opcode = msg.Read<ushort>();

                        massiveMsg = new Message(opcode, false, true);
                    } else {
                        if (massiveMsg == null) {
                            throw new InvalidMessageException(InvalidMessageReason.Distorted);
                        }

                        massiveMsg.Write<byte>(msg.AsDataSpan().Slice(1));
                        massiveCount--;

                        if (massiveCount == 0) {
                            // Return the message in a ready-to-use status.
                            massiveMsg.Position = Message.DataOffset;

                            return massiveMsg;
                        }
                    }
                } else {
                    return msg;
                }
            }
        }

        /// <summary>
        ///     Receives from a connected session, until buffer is completely filled.
        /// </summary>
        /// <param name="buffer">The buffer to receive into.</param>
        /// <param name="flags">The socket receiving flag</param>
        /// <returns></returns>
        private async Task ReceiveExactAsync(Memory<byte> buffer, SocketFlags flags) {
            var received = 0;
            var remaining = buffer.Length;
            while (received < remaining) {
                // System.Net.Sockets.SocketException (10057)
                var receivedChunk = await this._socket.ReceiveAsync(buffer.Slice(received), flags)
                    .ConfigureAwait(false);

                if (receivedChunk == 0) {
                    await this.DisconnectAsync().ConfigureAwait(false);
                    throw new RemoteDisconnectedException();
                }

                received += receivedChunk;
            }
        }

        /// <summary>
        ///     Responds to a <see cref="Message" /> by invoking all the registered services and handlers,
        ///     doing nothing if the passed message is <c>null</c>.
        ///     You should catch any exceptions threw by any service or handler.
        /// </summary>
        /// <param name="msg">The message to respond to.</param>
        /// <returns></returns>
        public async Task RespondAsync(Message msg) {
            if (msg == null) {
                return;
            }

            foreach (var (opcode, handler) in this._handlers) {
                if (msg.ID.Value == opcode) {
                    await handler.Invoke(this, msg).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        ///     Completes the Handshake process, after calling this method, <see cref="Ready" /> should
        ///     return true, if the session is not closed somehow.
        /// </summary>
        /// <returns></returns>
        public async Task HandshakeAsync() {
            var server = this.FindService<ServerHandshakeService>();
            if (server != null) {
                await server.Begin(this).ConfigureAwait(false);
            }

            while (!this.Ready) {
                var msg = await this.ReceiveAsync().ConfigureAwait(false);
                await this.RespondAsync(msg).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Runs the session until it's closed somehow. This behavior is accomplished by continue
        ///     receiving via <see cref="ReceiveAsync" /> and responding with <see cref="RespondAsync" />
        ///     after completing the handshake process via <see cref="HandshakeAsync" />.
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync() {
            await this.HandshakeAsync().ConfigureAwait(false);

            while (true) {
                var msg = await this.ReceiveAsync().ConfigureAwait(false);
                await this.RespondAsync(msg).ConfigureAwait(false);
            }
        }
    }
}