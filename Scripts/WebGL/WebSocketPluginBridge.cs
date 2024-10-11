#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XmobiTea.ProtonNet.Client.WebGL
{
    /// <summary>
    /// WebSocketPluginBridge provides a bridge to interact with WebSocket operations in the browser through WebGL.
    /// This class contains delegates and P/Invoke methods for initializing, connecting, sending data, and managing WebSocket instances.
    /// </summary>
    public class WebSocketPluginBridge
    {
        /// <summary>
        /// Delegate for handling the OnOpen event when a WebSocket connection is opened.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        public delegate void OnOpenDelegate(int id);

        /// <summary>
        /// Delegate for handling the OnClose event when a WebSocket connection is closed.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="code">The WebSocket close code.</param>
        /// <param name="reason">The reason for the WebSocket closure.</param>
        /// <param name="wasClean">Indicates whether the connection was closed cleanly.</param>
        public delegate void OnCloseDelegate(int id, int code, string reason, bool wasClean);

        /// <summary>
        /// Delegate for handling the OnError event when a WebSocket error occurs.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="event">The error event information.</param>
        public delegate void OnErrorDelegate(int id, string @event);

        /// <summary>
        /// Delegate for handling the OnMessage event when a message is received from the WebSocket.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="bufferPtr">Pointer to the message buffer.</param>
        /// <param name="length">The length of the message buffer.</param>
        public delegate void OnMessageDelegate(int id, IntPtr bufferPtr, int length);

        /// <summary>
        /// Initializes the WebSocket plugin with event handlers for open, close, error, and message events.
        /// </summary>
        /// <param name="onOpen">Delegate to handle the OnOpen event.</param>
        /// <param name="onClose">Delegate to handle the OnClose event.</param>
        /// <param name="onError">Delegate to handle the OnError event.</param>
        /// <param name="onMessage">Delegate to handle the OnMessage event.</param>
        [DllImport("__Internal")]
        public static extern void Init(OnOpenDelegate onOpen, OnCloseDelegate onClose, OnErrorDelegate onError, OnMessageDelegate onMessage);

        /// <summary>
        /// Initializes a WebSocket instance with the given id and URL.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="url">The WebSocket server URL.</param>
        /// <returns>True if initialization is successful; otherwise, false.</returns>
        [DllImport("__Internal")]
        public static extern bool InitInstance(int id, string url);

        /// <summary>
        /// Connects to the WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <returns>True if connection is successful; otherwise, false.</returns>
        [DllImport("__Internal")]
        public static extern bool Connect(int id);

        /// <summary>
        /// Sends data to the WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <param name="bufferPtr">Pointer to the buffer containing the data to send.</param>
        /// <param name="length">The length of the buffer.</param>
        /// <returns>True if the data is sent successfully; otherwise, false.</returns>
        [DllImport("__Internal")]
        public static extern bool Send(int id, IntPtr bufferPtr, int length);

        /// <summary>
        /// Disconnects the WebSocket instance with the specified id.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <returns>True if the WebSocket instance is disconnected successfully; otherwise, false.</returns>
        [DllImport("__Internal")]
        public static extern bool Disconnect(int id);

        /// <summary>
        /// Checks whether the WebSocket instance with the specified id is connected.
        /// </summary>
        /// <param name="id">The unique identifier for the WebSocket instance.</param>
        /// <returns>True if the WebSocket is connected; otherwise, false.</returns>
        [DllImport("__Internal")]
        public static extern bool IsConnected(int id);

        /// <summary>
        /// Dictionary to store OnOpen event delegates for each WebSocket instance.
        /// </summary>
        internal static IDictionary<int, WebSocketPluginDelegates.OnOpenDelegate> onOpenDict { get; }

        /// <summary>
        /// Dictionary to store OnClose event delegates for each WebSocket instance.
        /// </summary>
        internal static IDictionary<int, WebSocketPluginDelegates.OnCloseDelegate> onCloseDict { get; }

        /// <summary>
        /// Dictionary to store OnError event delegates for each WebSocket instance.
        /// </summary>
        internal static IDictionary<int, WebSocketPluginDelegates.OnErrorDelegate> onErrorDict { get; }

        /// <summary>
        /// Dictionary to store OnMessage event delegates for each WebSocket instance.
        /// </summary>
        internal static IDictionary<int, WebSocketPluginDelegates.OnMessageDelegate> onMessageDict { get; }

        /// <summary>
        /// Static constructor for WebSocketPluginBridge. Initializes dictionaries
        /// for storing WebSocket event delegates.
        /// </summary>
        static WebSocketPluginBridge()
        {
            onOpenDict = new Dictionary<int, WebSocketPluginDelegates.OnOpenDelegate>();
            onCloseDict = new Dictionary<int, WebSocketPluginDelegates.OnCloseDelegate>();
            onErrorDict = new Dictionary<int, WebSocketPluginDelegates.OnErrorDelegate>();
            onMessageDict = new Dictionary<int, WebSocketPluginDelegates.OnMessageDelegate>();
        }

        /// <summary>
        /// Initializes WebSocketPluginBridge with event handlers for WebSocket events
        /// such as OnOpen, OnClose, OnError, and OnMessage.
        /// </summary>
        public static void Init()
        {
            Init(OnOpenReceived, OnCloseReceived, OnErrorReceived, OnMessageReceived);
        }

        /// <summary>
        /// Handles the OnOpen WebSocket event by invoking the corresponding delegate from onOpenDict.
        /// </summary>
        [AOT.MonoPInvokeCallback(typeof(WebSocketPluginBridge.OnOpenDelegate))]
        private static void OnOpenReceived(int id)
        {
            if (onOpenDict.TryGetValue(id, out var onOpen))
                onOpen?.Invoke(new WebSocketPluginDelegates.OpenEvent());
        }

        /// <summary>
        /// Handles the OnClose WebSocket event by invoking the corresponding delegate from onCloseDict.
        /// </summary>
        [AOT.MonoPInvokeCallback(typeof(WebSocketPluginBridge.OnCloseDelegate))]
        private static void OnCloseReceived(int id, int code, string reason, bool wasClean)
        {
            if (onCloseDict.TryGetValue(id, out var onClose))
                onClose?.Invoke(new WebSocketPluginDelegates.CloseEvent()
                {
                    Code = code,
                    Reason = reason,
                    WasClean = wasClean,
                });
        }

        /// <summary>
        /// Handles the OnError WebSocket event by invoking the corresponding delegate from onErrorDict.
        /// </summary>
        [AOT.MonoPInvokeCallback(typeof(WebSocketPluginBridge.OnErrorDelegate))]
        private static void OnErrorReceived(int id, string eventJson)
        {
            if (onErrorDict.TryGetValue(id, out var onError))
                onError?.Invoke(new WebSocketPluginDelegates.ErrorEvent()
                {
                    Event = eventJson,
                });
        }

        /// <summary>
        /// Handles the OnMessage WebSocket event by invoking the corresponding delegate from onMessageDict.
        /// </summary>
        [AOT.MonoPInvokeCallback(typeof(WebSocketPluginBridge.OnMessageDelegate))]
        private static void OnMessageReceived(int id, IntPtr bufferPtr, int length)
        {
            byte[] buffer = new byte[length];
            Marshal.Copy(bufferPtr, buffer, 0, length);

            if (onMessageDict.TryGetValue(id, out var onMessage))
                onMessage?.Invoke(new WebSocketPluginDelegates.MessageEvent()
                {
                    Buffer = buffer,
                });
        }

    }

}
#endif
