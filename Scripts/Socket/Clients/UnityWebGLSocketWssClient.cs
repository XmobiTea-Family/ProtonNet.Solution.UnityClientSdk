#if UNITY_WEBGL && !UNITY_EDITOR
using System.Threading;
using XmobiTea.ProtonNet.Client.WebGL;
using XmobiTea.ProtonNetClient.Options;
using XmobiTea.ProtonNetCommon;

namespace XmobiTea.ProtonNet.Client.Socket.Clients
{
    /// <summary>
    /// Represents a WebSocket Secure (WSS) client, implementing the <see cref="ISocketClient"/> and <see cref="ISetEncryptKey"/> interfaces.
    /// </summary>
    class UnityWebGLSocketWssClient : ISocketClient, ISetEncryptKey
    {
        /// <summary>
        /// Event handler for when the client connects to the server.
        /// </summary>
        internal OnSocketClientConnected onConnected;

        /// <summary>
        /// Event handler for when the client disconnects from the server.
        /// </summary>
        internal OnSocketClientDisconnected onDisconnected;

        /// <summary>
        /// Event handler for when the client receives data from the server.
        /// </summary>
        internal OnSocketClientReceived onReceived;

        /// <summary>
        /// Event handler for when a socket error occurs.
        /// </summary>
        internal OnSocketClientError onError;

        /// <summary>
        /// The encryption key used for encrypting and decrypting data.
        /// </summary>
        private byte[] encryptKey { get; set; }

        /// <summary>
        /// Interface for tracking and updating network statistics.
        /// </summary>
        private IChangeNetworkStatistics networkStatistics { get; }

        /// <summary>
        /// Gets the instance of the WebSocket plugin bridge support used for managing 
        /// communication between the WebSocket and the application.
        /// </summary>
        private IWebSocketPluginBridgeSupport webSocketPluginBridgeSupport { get; }

        /// <summary>
        /// Gets the unique identifier for this instance, used for identifying connections 
        /// or operations within the WebSocket communication process.
        /// </summary>
        private int id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWebGLSocketWssClient"/> class with the specified server address, port, client options, and Ssl options.
        /// </summary>
        /// <param name="address">The server address.</param>
        /// <param name="port">The server port.</param>
        /// <param name="options">The TCP client options.</param>
        /// <param name="sslOptions">The Ssl options for secure communication.</param>
        public UnityWebGLSocketWssClient(IWebSocketPluginBridgeSupport webSocketPluginBridgeSupport, int id, string address, int port, TcpClientOptions options, ProtonNetCommon.SslOptions sslOptions)
        {
            this.webSocketPluginBridgeSupport = webSocketPluginBridgeSupport;
            this.id = id;
            this.networkStatistics = new ChangeNetworkStatistics();

            var finalAddress = "wss://" + address;
            if (port > 0) finalAddress += ":" + port;

            this.webSocketPluginBridgeSupport.InitInstance(this.id, finalAddress);
            this.webSocketPluginBridgeSupport.SubscriberOnOpenDelegate(this.id, this.OnWsOpen);
            this.webSocketPluginBridgeSupport.SubscriberOnCloseDelegate(this.id, this.OnWsClose);
            this.webSocketPluginBridgeSupport.SubscriberOnErrorDelegate(this.id, this.OnWsError);
            this.webSocketPluginBridgeSupport.SubscriberOnMessageDelegate(this.id, this.OnWsMessage);
        }

        /// <summary>
        /// Sets the encryption key used by the client.
        /// </summary>
        /// <param name="encryptKey">The encryption key as a byte array.</param>
        public void SetEncryptKey(byte[] encryptKey) => this.encryptKey = encryptKey;

        /// <summary>
        /// Gets the encryption key used by the client.
        /// </summary>
        /// <returns>The encryption key as a byte array.</returns>
        public byte[] GetEncryptKey() => this.encryptKey;

        /// <summary>
        /// Initiates a connection to the server asynchronously.
        /// </summary>
        /// <returns>True if the connection was successfully initiated, otherwise false.</returns>
        public bool Connect() => this.webSocketPluginBridgeSupport.Connect(this.id);

        /// <summary>
        /// Disconnects from the server asynchronously.
        /// </summary>
        /// <returns>True if the disconnection was successfully initiated, otherwise false.</returns>
        public bool Disconnect() => this.webSocketPluginBridgeSupport.Disconnect(this.id);

        /// <summary>
        /// Checks if the client is currently connected to the server.
        /// </summary>
        /// <returns>True if connected, otherwise false.</returns>
        public bool IsConnected() => this.webSocketPluginBridgeSupport.IsConnected(this.id);

        /// <summary>
        /// Reconnects to the server asynchronously.
        /// </summary>
        /// <returns>True if the reconnection was successfully initiated, otherwise false.</returns>
        public bool Reconnect()
        {
            this.Disconnect();

            while (this.IsConnected())
                Thread.Yield();

            return this.Connect();
        }

        /// <summary>
        /// Retrieves the current network statistics, including data on sent and received 
        /// packets and bytes.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="INetworkStatistics"/> representing the network statistics.
        /// </returns>
        public INetworkStatistics GetNetworkStatistics() => this.networkStatistics;

        /// <summary>
        /// Handles the WebSocket open event, invoking the onConnected action to notify 
        /// that the connection has been successfully established.
        /// </summary>
        /// <param name="event">
        /// The <see cref="WebSocketPluginDelegates.OpenEvent"/> representing the open event details.
        /// </param>
        private void OnWsOpen(WebSocketPluginDelegates.OpenEvent @event) => this.onConnected?.Invoke();

        /// <summary>
        /// Handles the WebSocket close event, invoking the onDisconnected action to notify 
        /// that the connection has been closed.
        /// </summary>
        /// <param name="event">
        /// The <see cref="WebSocketPluginDelegates.CloseEvent"/> representing the close event details.
        /// </param>
        private void OnWsClose(WebSocketPluginDelegates.CloseEvent @event) => this.onDisconnected?.Invoke();

        /// <summary>
        /// Handles the WebSocket error event, invoking the onError action to notify 
        /// that an error occurred during the WebSocket communication.
        /// </summary>
        /// <param name="event">
        /// The <see cref="WebSocketPluginDelegates.ErrorEvent"/> representing the error event details.
        /// </param>
        private void OnWsError(WebSocketPluginDelegates.ErrorEvent @event) => this.onError?.Invoke(System.Net.Sockets.SocketError.SocketError);

        /// <summary>
        /// Handles the WebSocket message event, updating network statistics and invoking 
        /// the onReceived action to process the received message buffer.
        /// </summary>
        /// <param name="event">
        /// The <see cref="WebSocketPluginDelegates.MessageEvent"/> representing the message event details.
        /// </param>
        private void OnWsMessage(WebSocketPluginDelegates.MessageEvent @event)
        {
            this.networkStatistics.ChangeBytesReceived(@event.Buffer.Length);
            this.networkStatistics.IncPacketReceived();

            this.onReceived?.Invoke(@event.Buffer, 0, @event.Buffer.Length);
        }

        /// <summary>
        /// Sends data to the server as a binary message.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <returns>The number of bytes sent.</returns>
        public int Send(byte[] buffer)
        {
            if (!this.webSocketPluginBridgeSupport.Send(this.id, buffer)) return 0;

            this.networkStatistics.IncPacketSent();
            this.networkStatistics.ChangeBytesSent(buffer.Length);

            return buffer.Length;
        }

        /// <summary>
        /// Sends data asynchronously to the server as a binary message.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <returns>True if the data was successfully queued for sending, otherwise false.</returns>
        public bool SendAsync(byte[] buffer) => this.Send(buffer) != 0;

    }

}
#endif
