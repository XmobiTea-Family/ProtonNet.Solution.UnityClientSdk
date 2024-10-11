#if UNITY_WEBGL && !UNITY_EDITOR
namespace XmobiTea.ProtonNet.Client.WebGL
{
    /// <summary>
    /// WebSocketPluginDelegates contains the event classes and delegate definitions for handling WebSocket events.
    /// These include open, close, error, and message events with their corresponding data.
    /// </summary>
    public class WebSocketPluginDelegates
    {
        /// <summary>
        /// Represents the event that occurs when a WebSocket connection is opened.
        /// </summary>
        public class OpenEvent
        {
            // Event data for the OnOpen event (currently empty).

        }

        /// <summary>
        /// Represents the event that occurs when a WebSocket connection is closed.
        /// </summary>
        public class CloseEvent
        {
            /// <summary>
            /// The WebSocket close code.
            /// </summary>
            public int Code;

            /// <summary>
            /// The reason why the WebSocket connection was closed.
            /// </summary>
            public string Reason;

            /// <summary>
            /// Indicates whether the connection was closed cleanly.
            /// </summary>
            public bool WasClean;

        }

        /// <summary>
        /// Represents the event that occurs when a WebSocket error happens.
        /// </summary>
        public class ErrorEvent
        {
            /// <summary>
            /// The error event data in JSON format.
            /// </summary>
            public string Event;

        }

        /// <summary>
        /// Represents the event that occurs when a message is received from the WebSocket.
        /// </summary>
        public class MessageEvent
        {
            /// <summary>
            /// The message data received from the WebSocket.
            /// </summary>
            public byte[] Buffer;

        }

        /// <summary>
        /// Delegate for handling the OnOpen event when a WebSocket connection is opened.
        /// </summary>
        /// <param name="event">The event data associated with the WebSocket opening.</param>
        public delegate void OnOpenDelegate(OpenEvent @event);

        /// <summary>
        /// Delegate for handling the OnClose event when a WebSocket connection is closed.
        /// </summary>
        /// <param name="event">The event data associated with the WebSocket closing.</param>
        public delegate void OnCloseDelegate(CloseEvent @event);

        /// <summary>
        /// Delegate for handling the OnError event when a WebSocket error occurs.
        /// </summary>
        /// <param name="event">The event data associated with the WebSocket error.</param>
        public delegate void OnErrorDelegate(ErrorEvent @event);

        /// <summary>
        /// Delegate for handling the OnMessage event when a message is received from the WebSocket.
        /// </summary>
        /// <param name="event">The event data associated with the received WebSocket message.</param>
        public delegate void OnMessageDelegate(MessageEvent @event);

    }

}
#endif
