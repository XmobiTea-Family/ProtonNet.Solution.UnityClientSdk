using System.Collections;
using System.IO;
using UnityEngine.Networking;
using XmobiTea.ProtonNet.Client.Models;
using XmobiTea.ProtonNet.Client.WebApi.Types;
using XmobiTea.ProtonNet.Networking;
using XmobiTea.ProtonNet.RpcProtocol.Types;
using XmobiTea.ProtonNetClient.Options;

namespace XmobiTea.ProtonNet.Client.WebApi
{
    /// <summary>
    /// Represents a Web API client peer that communicates with the server using HTTP using in Unity
    /// Inherits from <see cref="AbstractWebApiClientPeer"/> and implements the 
    /// <see cref="IWebApiClientPeer"/> interface.
    /// </summary>
    public class UnityWebRequestClientPeer : AbstractWebApiClientPeer, IWebApiClientPeer
    {
        /// <summary>
        /// A custom <see cref="CertificateHandler"/> implementation that accepts all certificates, 
        /// bypassing the default certificate validation process.
        /// </summary>
        class AcceptAllCertificatesSignedWithASpecificKeyPublicKey : CertificateHandler
        {
            /// <summary>
            /// Overrides the certificate validation method to accept all certificates, 
            /// effectively disabling the certificate verification process.
            /// </summary>
            /// <param name="certificateData">The byte array containing the certificate data.</param>
            /// <returns>Always returns <c>true</c>, indicating the certificate is valid.</returns>
            protected override bool ValidateCertificate(byte[] certificateData) => true;
        }

        /// <summary>
        /// The default protocol provider type used for serialization.
        /// </summary>
        private static readonly ProtocolProviderType DefaultProtocolProviderType = ProtocolProviderType.MessagePack;

        /// <summary>
        /// The default crypto provider type used for encryption.
        /// </summary>
        private static readonly CryptoProviderType DefaultCryptoProviderType = CryptoProviderType.Aes;

        /// <summary>
        /// Private property representing the support class for handling MonoBehaviour functionalities in Unity.
        /// </summary>
        private IUnityMonoBehaviourSupport unityMonoBehaviourSupport { get; }

        /// <summary>
        /// Protected property representing the prefix used in logging for HTTP requests.
        /// </summary>
        protected override string logPrefix => "HTTP";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWebRequestClientPeer"/> class with the specified server address,
        /// initialization request, TCP client options, and Unity MonoBehaviour support.
        /// </summary>
        /// <param name="serverAddress">The server address for the client peer.</param>
        /// <param name="initRequest">The initialization request used to set up the client peer.</param>
        /// <param name="tcpClientOptions">The options for configuring the TCP client.</param>
        /// <param name="unityMonoBehaviourSupport">The MonoBehaviour support for Unity-specific features.</param>
        public UnityWebRequestClientPeer(string serverAddress, IClientPeerInitRequest initRequest, TcpClientOptions tcpClientOptions, IUnityMonoBehaviourSupport unityMonoBehaviourSupport)
            : base(serverAddress, initRequest, tcpClientOptions) => this.unityMonoBehaviourSupport = unityMonoBehaviourSupport;

        /// <summary>
        /// Sends an operation request to the server.
        /// </summary>
        /// <param name="operationRequestPending">The pending operation request to send.</param>
        protected override void SendOperation(OperationRequestPending operationRequestPending) => this.Execute(operationRequestPending);

        /// <summary>
        /// Executes the operation request by sending it to the server and processing the response.
        /// </summary>
        /// <param name="operationRequestPending">The pending operation request to execute.</param>
        private void Execute(OperationRequestPending operationRequestPending) => this.unityMonoBehaviourSupport.RunCoroutine(this.IEExecute(operationRequestPending));

        /// <summary>
        /// Executes an operation request pending asynchronously using a coroutine.
        /// </summary>
        /// <param name="operationRequestPending">The pending operation request to be executed.</param>
        /// <returns>
        /// Returns an IEnumerator, allowing the method to be used as a coroutine.
        /// </returns>
        private IEnumerator IEExecute(OperationRequestPending operationRequestPending)
        {
            var operationRequest = operationRequestPending.GetOperationRequest();
            var sendParameters = operationRequestPending.GetSendParameters();

            var fullUrl = this.serverAddress + "/proton/api";

            using (var unityWebRequest = new UnityWebRequest(fullUrl, UnityWebRequest.kHttpVerbPOST))
            {
                var certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                unityWebRequest.certificateHandler = certificateHandler;

                unityWebRequest.timeout = operationRequestPending.GetTimeoutInSeconds();

                unityWebRequest.SetRequestHeader(HeaderNames.ContentType, HeaderValues.RpcProtocol);
                unityWebRequest.SetRequestHeader(HeaderNames.SessionId, this.sessionId);

                byte[] dataBodyRequest;

                using (var mStream = new MemoryStream())
                {
                    if (sendParameters.Encrypted)
                        this.rpcProtocolService.WriteEncrypt(mStream, OperationType.OperationRequest, operationRequest, sendParameters, DefaultProtocolProviderType, DefaultCryptoProviderType, this.encryptKey);
                    else
                        this.rpcProtocolService.Write(mStream, OperationType.OperationRequest, operationRequest, sendParameters, DefaultProtocolProviderType);

                    dataBodyRequest = mStream.ToArray();
                }

                this.networkStatistics.ChangeBytesSent(dataBodyRequest.Length);

                unityWebRequest.uploadHandler = new UploadHandlerRaw(dataBodyRequest);

                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

                if (!string.IsNullOrEmpty(this.authToken))
                    unityWebRequest.SetRequestHeader(HeaderNames.Token, this.authToken);
                if (sendParameters.Encrypted)
                    unityWebRequest.SetRequestHeader(HeaderNames.EncryptKey, this.encryptKeyStr);

                yield return unityWebRequest.SendWebRequest();

                certificateHandler.Dispose();

                operationRequestPending.OnRecv();

                if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError
                    || unityWebRequest.result == UnityWebRequest.Result.ProtocolError
                    || unityWebRequest.result == UnityWebRequest.Result.DataProcessingError)
                {
                    var response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = ReturnCode.OperationInvalid,
                        ResponseId = operationRequest.RequestId,
                        DebugMessage = unityWebRequest.error,
                    };

                    operationRequestPending.SetOperationResponse(response);
                    operationRequestPending.SetResponseSendParameters(operationRequestPending.GetSendParameters());

                    this.logger.Error(unityWebRequest.error);
                }
                else if (unityWebRequest.responseCode != 200)
                {
                    var response = new OperationResponse(operationRequest.OperationCode)
                    {
                        ReturnCode = ReturnCode.OperationInvalid,
                        ResponseId = operationRequest.RequestId,
                        DebugMessage = unityWebRequest.error,
                    };

                    operationRequestPending.SetOperationResponse(response);
                    operationRequestPending.SetResponseSendParameters(operationRequestPending.GetSendParameters());

                    this.logger.Error("responseCode != 200, " + unityWebRequest.responseCode);
                }
                else
                {
                    try
                    {
                        var data = unityWebRequest.downloadHandler.data;

                        this.networkStatistics.ChangeBytesReceived(data.Length);

                        OperationResponse operationResponse;

                        using (var mStream = new MemoryStream(data))
                        {
                            if (!this.rpcProtocolService.TryRead(mStream, out var header, out var payload))
                            {
                                operationResponse = new OperationResponse(operationRequest.OperationCode)
                                {
                                    ReturnCode = ReturnCode.OperationInvalid,
                                    DebugMessage = "Cannot read data body",
                                };

                                operationRequestPending.SetResponseSendParameters(operationRequestPending.GetSendParameters());
                            }
                            else
                            {
                                if (header.SendParameters.Encrypted)
                                {
                                    if (!this.rpcProtocolService.TryDeserializeEncryptOperationModel(payload, header.OperationType, header.ProtocolProviderType, header.CryptoProviderType.GetValueOrDefault(), this.encryptKey, out var operationModel))
                                    {
                                        operationResponse = new OperationResponse(string.Empty)
                                        {
                                            ReturnCode = ReturnCode.OperationInvalid,
                                            DebugMessage = "Cannot read data body",
                                        };
                                    }
                                    else
                                    {
                                        operationResponse = (OperationResponse)operationModel;
                                    }
                                }
                                else
                                {
                                    if (!this.rpcProtocolService.TryDeserializeOperationModel(payload, header.OperationType, header.ProtocolProviderType, out var operationModel))
                                    {
                                        operationResponse = new OperationResponse(string.Empty)
                                        {
                                            ReturnCode = ReturnCode.OperationInvalid,
                                            DebugMessage = "Cannot read data body",
                                        };
                                    }
                                    else
                                    {
                                        operationResponse = (OperationResponse)operationModel;
                                    }
                                }

                                operationRequestPending.SetResponseSendParameters(header.SendParameters);
                            }
                        }

                        operationRequestPending.SetOperationResponse(operationResponse);
                    }
                    catch (System.Exception e)
                    {
                        var response = new OperationResponse(operationRequest.OperationCode)
                        {
                            ReturnCode = ReturnCode.OperationInvalid,
                            ResponseId = operationRequest.RequestId,
                            DebugMessage = e.Message,
                        };

                        operationRequestPending.SetOperationResponse(response);
                        operationRequestPending.SetResponseSendParameters(operationRequestPending.GetSendParameters());

                        this.logger.Error("Exception", e);
                    }
                }
            }
        }

        /// <summary>
        /// Sends a ping request to the server to check the connection status.
        /// </summary>
        /// <param name="onResponse">Callback method to handle the server's response.</param>
        /// <param name="timeoutInSeconds">Timeout period in seconds for the ping request.</param>
        public override void Ping(OnPingResponse onResponse, int timeoutInSeconds) => this.unityMonoBehaviourSupport.RunCoroutine(this.IEPing(onResponse, timeoutInSeconds));

        /// <summary>
        /// Sends a ping request and waits for a response asynchronously using a coroutine.
        /// </summary>
        /// <param name="onResponse">The callback invoked when a ping response is received.</param>
        /// <param name="timeoutInSeconds">The maximum amount of time to wait for a response, in seconds.</param>
        /// <returns>
        /// Returns an IEnumerator, allowing the method to be used as a coroutine.
        /// </returns>
        private IEnumerator IEPing(OnPingResponse onResponse, int timeoutInSeconds)
        {
            var fullUrl = this.serverAddress + "/proton/ping";

            using (var unityWebRequest = new UnityWebRequest(fullUrl, UnityWebRequest.kHttpVerbGET))
            {
                var certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                unityWebRequest.certificateHandler = certificateHandler;

                unityWebRequest.timeout = timeoutInSeconds;

                yield return unityWebRequest.SendWebRequest();

                certificateHandler.Dispose();

                onResponse?.Invoke(unityWebRequest.responseCode == 200);
            }
        }

        /// <summary>
        /// Sends a request to the server to get the current timestamp.
        /// </summary>
        /// <param name="onResponse">Callback method to handle the server's timestamp response.</param>
        /// <param name="timeoutInSeconds">Timeout period in seconds for the request.</param>
        public override void GetTs(OnGetTsResponse onResponse, int timeoutInSeconds) => this.unityMonoBehaviourSupport.RunCoroutine(this.IEGetTs(onResponse, timeoutInSeconds));

        /// <summary>
        /// Sends a request to retrieve a timestamp and waits for a response asynchronously using a coroutine.
        /// </summary>
        /// <param name="onResponse">The callback invoked when the timestamp response is received.</param>
        /// <param name="timeoutInSeconds">The maximum amount of time to wait for the response, in seconds.</param>
        /// <returns>
        /// Returns an IEnumerator, allowing the method to be used as a coroutine.
        /// </returns>
        private IEnumerator IEGetTs(OnGetTsResponse onResponse, int timeoutInSeconds)
        {
            var fullUrl = this.serverAddress + "/proton/getts";

            using (var unityWebRequest = new UnityWebRequest(fullUrl, UnityWebRequest.kHttpVerbGET))
            {
                var certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                unityWebRequest.certificateHandler = certificateHandler;

                unityWebRequest.timeout = timeoutInSeconds;

                yield return unityWebRequest.SendWebRequest();

                certificateHandler.Dispose();

                if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError
                    || unityWebRequest.result == UnityWebRequest.Result.ProtocolError
                    || unityWebRequest.result == UnityWebRequest.Result.DataProcessingError)
                {
                    onResponse?.Invoke(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                    this.logger.Error(unityWebRequest.error);
                }
                else
                {
                    try
                    {
                        onResponse?.Invoke(long.Parse(unityWebRequest.downloadHandler.text));
                    }
                    catch (System.Exception e)
                    {
                        onResponse?.Invoke(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                        this.logger.Error("Exception", e);
                    }
                }
            }
        }

    }

}
