using XmobiTea.ProtonNet.Client.Socket;
using XmobiTea.ProtonNet.Client.Socket.Types;
using XmobiTea.ProtonNet.Client.WebApi;

namespace XmobiTea.ProtonNet.Client
{
    /// <summary>
    /// UnityClientPeerFactory is a specialized factory class for creating Unity-specific client peers,
    /// inheriting from the ClientPeerFactory class.
    /// </summary>
    public class UnityClientPeerFactory : ClientPeerFactory
    {
        /// <summary>
        /// UnityMonoBehaviourSupport provides a static reference to a Unity GameObject that supports MonoBehaviour.
        /// This component is added to the Unity scene at runtime to manage ProtonNet-related operations.
        /// </summary>
        private static IUnityMonoBehaviourSupport unityMonoBehaviourSupport { get; }

        /// <summary>
        /// Static constructor for UnityClientPeerFactory. It initializes the UnityMonoBehaviourSupport object by 
        /// creating a new GameObject in the Unity scene and attaching the UnityMonoBehaviourSupport component.
        /// </summary>
        static UnityClientPeerFactory() => unityMonoBehaviourSupport = new UnityEngine.GameObject("[ProtonNet] UnityMonoBehaviourSupport").AddComponent<UnityMonoBehaviourSupport>();

        /// <summary>
        /// Initializes a new instance of the UnityClientPeerFactory class. If AutoCallService is enabled,
        /// it adds this client peer factory to the UnityMonoBehaviourSupport.
        /// </summary>
        /// <param name="builder">Builder object used to configure the client peer factory.</param>
        protected UnityClientPeerFactory(Builder builder) : base(builder)
        {
        }

        /// <summary>
        /// Override for SetupAutoCallService when AutoCallService enable.
        /// </summary>
        protected override void SetupAutoCallService() => unityMonoBehaviourSupport.AddClientPeerFactory(this);

        /// <summary>
        /// Creates a new WebApiClientPeer for handling Web API client communication.
        /// </summary>
        /// <param name="serverAddress">The address of the server to connect to.</param>
        /// <returns>A new instance of UnityWebRequestClientPeer.</returns>
        protected override IWebApiClientPeer CreateNewWebApiClientPeer(string serverAddress) => new UnityWebRequestClientPeer(serverAddress, this.InitRequestProviderService.NewClientPeerInitRequest(), this.TcpClientOptions, unityMonoBehaviourSupport);

        /// <summary>
        /// Creates a new SocketClientPeer for handling standard socket communication.
        /// </summary>
        /// <param name="serverAddress">The address of the server to connect to.</param>
        /// <param name="protocol">The transport protocol to be used (e.g., TCP or UDP).</param>
        /// <returns>A new instance of UnitySocketClientPeer.</returns>
        protected override ISocketClientPeer CreateNewSocketClientPeer(string serverAddress, TransportProtocol protocol) => new UnitySocketClientPeer(serverAddress, this.InitRequestProviderService.NewClientPeerInitRequest(), this.TcpClientOptions, this.UdpClientOptions, protocol, unityMonoBehaviourSupport);

        /// <summary>
        /// Creates a new SslSocketClientPeer for handling SSL socket communication.
        /// </summary>
        /// <param name="serverAddress">The address of the server to connect to.</param>
        /// <param name="sslProtocol">The SSL transport protocol to be used (e.g., SSL, WSS).</param>
        /// <returns>A new instance of UnitySocketClientPeer.</returns>
        protected override ISocketClientPeer CreateNewSslSocketClientPeer(string serverAddress, SslTransportProtocol sslProtocol) => new UnitySocketClientPeer(serverAddress, this.InitRequestProviderService.NewClientPeerInitRequest(), this.TcpClientOptions, this.UdpClientOptions, sslProtocol, sslProtocol == SslTransportProtocol.Ssl ? this.SslOptions : this.WsSslOptions, unityMonoBehaviourSupport);

        /// <summary>
        /// Creates a new builder for constructing UnityClientPeerFactory instances.
        /// </summary>
        /// <returns>A new instance of the Builder class.</returns>
        public new static Builder NewBuilder() => new UnityBuilder();

        /// <summary>
        /// UnityBuilder is a concrete implementation of the Builder class for creating Unity-specific 
        /// ClientPeerFactory instances.
        /// </summary>
        public class UnityBuilder : Builder
        {
            /// <summary>
            /// Builds and returns a new instance of UnityClientPeerFactory.
            /// </summary>
            /// <returns>A new instance of UnityClientPeerFactory constructed with this builder instance.</returns>
            public override ClientPeerFactory Build() => new UnityClientPeerFactory(this);

        }

    }

}
