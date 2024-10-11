#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XmobiTea.ProtonNet.Client.WebGL
{
    /// <summary>
    /// Interface IWebSocketPluginBridgeSupport defines the contract for WebSocket plugin bridge support.
    /// This interface provides methods for initializing, connecting, sending, and managing WebSocket instances.
    /// It also allows subscribing to WebSocket events such as OnOpen, OnClose, OnError, and OnMessage.
    /// </summary>
    interface IWebSocketPluginBridgeSupport
    {
        /// <summary>
        /// Initializes a WebSocket instance with the given id and URL.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="url">The URL of the WebSocket server to connect to.</param>
        /// <returns>True if the instance is initialized successfully; otherwise, false.</returns>
        bool InitInstance(int id, string url);

        /// <summary>
        /// Connects to a WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <returns>True if the connection is successful; otherwise, false.</returns>
        bool Connect(int id);

        /// <summary>
        /// Sends a byte buffer over the WebSocket connection with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="buffer">The byte array to be sent.</param>
        /// <returns>True if the data is sent successfully; otherwise, false.</returns>
        bool Send(int id, byte[] buffer);

        /// <summary>
        /// Disconnects the WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <returns>True if the instance is disconnected successfully; otherwise, false.</returns>
        bool Disconnect(int id);

        /// <summary>
        /// Checks if the WebSocket instance with the specified id is connected.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <returns>True if the WebSocket is connected; otherwise, false.</returns>
        bool IsConnected(int id);

        /// <summary>
        /// Subscribes to the OnOpen event for the WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="onOpen">The delegate to handle the OnOpen event.</param>
        void SubscriberOnOpenDelegate(int id, WebSocketPluginDelegates.OnOpenDelegate onOpen);

        /// <summary>
        /// Subscribes to the OnClose event for the WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="onClose">The delegate to handle the OnClose event.</param>
        void SubscriberOnCloseDelegate(int id, WebSocketPluginDelegates.OnCloseDelegate onClose);

        /// <summary>
        /// Subscribes to the OnError event for the WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="onError">The delegate to handle the OnError event.</param>
        void SubscriberOnErrorDelegate(int id, WebSocketPluginDelegates.OnErrorDelegate onError);

        /// <summary>
        /// Subscribes to the OnMessage event for the WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="onMessage">The delegate to handle the OnMessage event.</param>
        void SubscriberOnMessageDelegate(int id, WebSocketPluginDelegates.OnMessageDelegate onMessage);

    }

    /// <summary>
    /// Class WebSocketPluginBridgeSupport implements the IWebSocketPluginBridgeSupport interface.
    /// This class provides support for WebSocket on Unity WebGL by managing WebSocket events
    /// such as OnOpen, OnClose, OnError, and OnMessage through delegates.
    /// </summary>
    class WebSocketPluginBridgeSupport : IWebSocketPluginBridgeSupport
    {
        /// <summary>
        /// Initializes a new WebSocket instance with a given id and URL.
        /// </summary>
        public bool InitInstance(int id, string url) => WebSocketPluginBridge.InitInstance(id, url);

        /// <summary>
        /// Establishes a WebSocket connection for a given id.
        /// </summary>
        public bool Connect(int id) => WebSocketPluginBridge.Connect(id);

        /// <summary>
        /// Sends a buffer of bytes over the WebSocket connection for a given id.
        /// The buffer is pinned in memory to ensure it is not moved by the garbage collector.
        /// </summary>
        public bool Send(int id, byte[] buffer)
        {
            var pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pointer = pinnedArray.AddrOfPinnedObject();

            try
            {
                return WebSocketPluginBridge.Send(id, pointer, buffer.Length);
            }
            finally
            {
                pinnedArray.Free();
            }
        }

        /// <summary>
        /// Disconnects the WebSocket connection for a given id.
        /// </summary>
        public bool Disconnect(int id) => WebSocketPluginBridge.Disconnect(id);

        /// <summary>
        /// Checks if the WebSocket connection for a given id is still connected.
        /// </summary>
        public bool IsConnected(int id) => WebSocketPluginBridge.IsConnected(id);

        /// <summary>
        /// Subscribes the OnOpen delegate for a specific WebSocket instance.
        /// </summary>
        public void SubscriberOnOpenDelegate(int id, WebSocketPluginDelegates.OnOpenDelegate onOpen) => WebSocketPluginBridge.onOpenDict[id] = onOpen;

        /// <summary>
        /// Subscribes the OnClose delegate for a specific WebSocket instance.
        /// </summary>
        public void SubscriberOnCloseDelegate(int id, WebSocketPluginDelegates.OnCloseDelegate onClose) => WebSocketPluginBridge.onCloseDict[id] = onClose;

        /// <summary>
        /// Subscribes the OnError delegate for a specific WebSocket instance.
        /// </summary>
        public void SubscriberOnErrorDelegate(int id, WebSocketPluginDelegates.OnErrorDelegate onError) => WebSocketPluginBridge.onErrorDict[id] = onError;

        /// <summary>
        /// Subscribes the OnMessage delegate for a specific WebSocket instance.
        /// </summary>
        public void SubscriberOnMessageDelegate(int id, WebSocketPluginDelegates.OnMessageDelegate onMessage) => WebSocketPluginBridge.onMessageDict[id] = onMessage;

    }

}
#endif
