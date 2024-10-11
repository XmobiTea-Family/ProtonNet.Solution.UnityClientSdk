using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using XmobiTea.Bean.Support;
using XmobiTea.ProtonNet.Client.Models;
using XmobiTea.ProtonNet.Client.Socket.Clients;
using XmobiTea.ProtonNet.Client.Socket.Types;
using XmobiTea.ProtonNet.Networking;
using XmobiTea.ProtonNet.RpcProtocol.Types;
using XmobiTea.ProtonNetClient.Options;
using XmobiTea.ProtonNetCommon;

#if UNITY_WEBGL && !UNITY_EDITOR
using XmobiTea.ProtonNet.Client.WebGL;
#endif

namespace XmobiTea.ProtonNet.Client.Socket
{
    /// <summary>
    /// Represents a Unity-specific implementation of the <see cref="SocketClientPeer"/> class, 
    /// tailored for handling WebSocket connections on the Unity WebGL platform.
    /// </summary>
    public class UnitySocketClientPeer : SocketClientPeer, ISocketClientPeer, IAfterAutoBind
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// Static counter used to assign unique WebSocket IDs for WebGL WebSocket instances.
        /// </summary>
        private static int WebGLWebSocketId = 1;

        /// <summary>
        /// A static instance of <see cref="IWebSocketPluginBridgeSupport"/> that handles 
        /// WebSocket communication on Unity WebGL.
        /// </summary>
        private static IWebSocketPluginBridgeSupport WebSocketPluginBridgeSupport;
#endif
        /// <summary>
        /// Private property representing the support class for handling MonoBehaviour functionalities in Unity.
        /// </summary>
        private IUnityMonoBehaviourSupport unityMonoBehaviourSupport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitySocketClientPeer"/> class for 
        /// a non-SSL TCP or UDP connection.
        /// </summary>
        /// <param name="serverAddress">The server address to connect to.</param>
        /// <param name="initRequest">The initial request for peer connection setup.</param>
        /// <param name="tcpClientOptions">Options for the TCP client.</param>
        /// <param name="udpClientOptions">Options for the UDP client.</param>
        /// <param name="protocol">The transport protocol (TCP, UDP, or WebSocket).</param>
        public UnitySocketClientPeer(string serverAddress, IClientPeerInitRequest initRequest, TcpClientOptions tcpClientOptions, UdpClientOptions udpClientOptions, TransportProtocol protocol, IUnityMonoBehaviourSupport unityMonoBehaviourSupport)
            : base(serverAddress, initRequest, tcpClientOptions, udpClientOptions, protocol) => this.unityMonoBehaviourSupport = unityMonoBehaviourSupport;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitySocketClientPeer"/> class for an SSL connection.
        /// </summary>
        /// <param name="serverAddress">The server address to connect to.</param>
        /// <param name="initRequest">The initial request for peer connection setup.</param>
        /// <param name="tcpClientOptions">Options for the TCP client.</param>
        /// <param name="udpClientOptions">Options for the UDP client.</param>
        /// <param name="sslProtocol">The SSL protocol to be used (e.g., TLS or WSS).</param>
        /// <param name="sslOptions">SSL-specific options for the connection.</param>
        public UnitySocketClientPeer(string serverAddress, IClientPeerInitRequest initRequest, TcpClientOptions tcpClientOptions, UdpClientOptions udpClientOptions, SslTransportProtocol sslProtocol, SslOptions sslOptions, IUnityMonoBehaviourSupport unityMonoBehaviourSupport)
            : base(serverAddress, initRequest, tcpClientOptions, udpClientOptions, sslProtocol, sslOptions) => this.unityMonoBehaviourSupport = unityMonoBehaviourSupport;

        /// <summary>
        /// Override on AutoReconnect call to reconnect the socket
        /// </summary>
        /// <param name="reconnectInSeconds">The reconnectInSeconds after</param>
        protected override void OnAutoReconnect(int reconnectInSeconds) => this.unityMonoBehaviourSupport.RunCoroutine(this.IEAutoReconnect(reconnectInSeconds));

        /// <summary>
        /// Coroutine Unity to reconnect
        /// </summary>
        /// <param name="reconnectInSeconds"></param>
        /// <returns></returns>
        private IEnumerator IEAutoReconnect(int reconnectInSeconds)
        {
            yield return new WaitForSeconds(reconnectInSeconds);

            this.Reconnect(this.autoReconnect, this.onImmediatelyConnected, this.onImmediatelyDisconnected);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// Checks if the <see cref="WebSocketPluginBridgeSupport"/> instance is initialized. If not, 
        /// it creates a new instance and initializes it for WebSocket communication on WebGL.
        /// </summary>
        void CheckOrCreateNewWebSocketPluginBridgeSupport()
        {
            if (WebSocketPluginBridgeSupport == null)
            {
                WebSocketPluginBridgeSupport = new WebSocketPluginBridgeSupport();
                WebSocketPluginBridge.Init();
            }
        }
#endif

        /// <summary>
        /// Creates a new TCP client peer. This method throws an exception on the WebGL platform 
        /// since TCP is not supported on WebGL.
        /// </summary>
        /// <param name="host">The server hostname.</param>
        /// <param name="port">The port number.</param>
        /// <param name="tcpClientOptions">TCP client options.</param>
        /// <returns>
        /// A new instance of <see cref="ISocketClient"/> for TCP connection, 
        /// or throws an exception on WebGL.
        /// </returns>
        protected override ISocketClient NewTcpClient(string host, int port, TcpClientOptions tcpClientOptions)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            throw new System.Exception("Unity WebGL platform does not support this socket client, please use Ws or Wss instead");
#endif
            return base.NewTcpClient(host, port, tcpClientOptions);
        }

        /// <summary>
        /// Creates a new SSL client peer. This method throws an exception on the WebGL platform 
        /// since SSL connections are not supported on WebGL.
        /// </summary>
        /// <param name="host">The server hostname.</param>
        /// <param name="port">The port number.</param>
        /// <param name="tcpClientOptions">TCP client options.</param>
        /// <param name="sslOptions">SSL connection options.</param>
        /// <returns>
        /// A new instance of <see cref="ISocketClient"/> for SSL connection, 
        /// or throws an exception on WebGL.
        /// </returns>
        protected override ISocketClient NewSslClient(string host, int port, TcpClientOptions tcpClientOptions, SslOptions sslOptions)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            throw new System.Exception("Unity WebGL platform does not support this socket client, please use Ws or Wss instead");
#endif
            return base.NewSslClient(host, port, tcpClientOptions, sslOptions);
        }

        /// <summary>
        /// Creates a new UDP client peer. This method throws an exception on the WebGL platform 
        /// since UDP connections are not supported on WebGL.
        /// </summary>
        /// <param name="host">The server hostname.</param>
        /// <param name="port">The port number.</param>
        /// <param name="udpClientOptions">UDP client options.</param>
        /// <returns>
        /// A new instance of <see cref="ISocketClient"/> for UDP connection, 
        /// or throws an exception on WebGL.
        /// </returns>
        protected override ISocketClient NewUdpClient(string host, int port, UdpClientOptions udpClientOptions)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            throw new System.Exception("Unity WebGL platform does not support this socket client, please use Ws or Wss instead");
#endif
            return base.NewUdpClient(host, port, udpClientOptions);
        }

        /// <summary>
        /// Creates a new WebSocket (Ws) client peer for WebGL platform. If WebSocketPluginBridgeSupport 
        /// is not initialized, it initializes it first.
        /// </summary>
        /// <param name="host">The server hostname.</param>
        /// <param name="port">The port number.</param>
        /// <param name="tcpClientOptions">TCP client options.</param>
        /// <returns>
        /// A new instance of <see cref="UnityWebGLSocketWsClient"/> for WebSocket connection on WebGL, 
        /// or a base Ws client for other platforms.
        /// </returns>
        protected override ISocketClient NewWsClient(string host, int port, TcpClientOptions tcpClientOptions)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            this.CheckOrCreateNewWebSocketPluginBridgeSupport();
            var answer = new UnityWebGLSocketWsClient(WebSocketPluginBridgeSupport, WebGLWebSocketId++, host, port, tcpClientOptions);

            answer.onConnected += this.OnSocketClientConnected;
            answer.onDisconnected += this.OnSocketClientDisconnected;
            answer.onReceived += this.OnSocketClientReceived;
            answer.onError += this.OnSocketClientError;

            return answer;
#endif
            return base.NewWsClient(host, port, tcpClientOptions);
        }

        /// <summary>
        /// Creates a new Secure WebSocket (Wss) client peer for WebGL platform. If WebSocketPluginBridgeSupport 
        /// is not initialized, it initializes it first.
        /// </summary>
        /// <param name="host">The server hostname.</param>
        /// <param name="port">The port number.</param>
        /// <param name="tcpClientOptions">TCP client options.</param>
        /// <param name="sslOptions">SSL connection options.</param>
        /// <returns>
        /// A new instance of <see cref="UnityWebGLSocketWssClient"/> for Secure WebSocket connection on WebGL, 
        /// or a base Wss client for other platforms.
        /// </returns>
        protected override ISocketClient NewWssClient(string host, int port, TcpClientOptions tcpClientOptions, SslOptions sslOptions)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            this.CheckOrCreateNewWebSocketPluginBridgeSupport();
            var answer = new UnityWebGLSocketWssClient(WebSocketPluginBridgeSupport, WebGLWebSocketId++, host, port, tcpClientOptions, sslOptions);
            
            answer.onConnected += this.OnSocketClientConnected;
            answer.onDisconnected += this.OnSocketClientDisconnected;
            answer.onReceived += this.OnSocketClientReceived;
            answer.onError += this.OnSocketClientError;

            return answer;
#endif
            return base.NewWssClient(host, port, tcpClientOptions, sslOptions);
        }

    }

}
