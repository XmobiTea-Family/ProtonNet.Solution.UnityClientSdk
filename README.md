# ProtonNet Server

![GitHub release](https://img.shields.io/github/release/XmobiTea-Family/ProtonNetSolution.svg)
![License](https://img.shields.io/github/license/XmobiTea-Family/ProtonNetSolution)
[![GitHub star chart](https://img.shields.io/github/stars/XmobiTea-Family/ProtonNetSolution?style=social)](https://star-history.com/#XmobiTea-Family/ProtonNetSolution)

# ProtonNet UnityClientSdk

Supports Unity platforms:
* `Android`
* `iOS`
* `WebGL`
* `Standalone`
* and more...

## I. Introduction
`ProtonNet Unity Client Sdk` is the ProtonNet Client support for Unity project.

## II. How to import
* import via `Unity Package` file [Release](https://github.com/XmobiTea-Family/ProtonNet.Solution.UnityClientSdk/releases)
* import via `Unity Package Manager`
    1. Open `Package Manager` by `Window/PackageManager`
    2. Press `+` -> `Add package from git URL...`
    3. Type `https://github.com/XmobiTea-Family/ProtonNet.Solution.UnityClientSdk` then press `Add`

## III. How to use

#### Create a MonoBehaviour Script

Create a new `MonoBehaviour` script in Unity with the following content:

```csharp
using UnityEngine;
using XmobiTea.Data;
using XmobiTea.Logging;
using XmobiTea.Logging.Unity;
using XmobiTea.ProtonNet.Client;
using XmobiTea.ProtonNet.Client.Socket;
using XmobiTea.ProtonNet.Networking;
using XmobiTea.ProtonNet.Networking.Extensions;

public class ProtonNetworkBehaviour : MonoBehaviour {
    IClientPeerFactory clientPeerFactory;
    ISocketClientPeer socketClientPeer;

    void Start()
    {
        LogManager.SetLoggerFactory(UnityLoggerFactory.Instance);

        clientPeerFactory = UnityClientPeerFactory.NewBuilder()
            .SetAutoCallService(true)
            .Build();

        socketClientPeer = clientPeerFactory.NewSocketClientPeer("http://127.0.0.1:32202", XmobiTea.ProtonNet.Client.Socket.Types.TransportProtocol.Tcp);
        socketClientPeer.Connect(true, (connectionId, serverSessionId) =>
        {
            Debug.Log("OnConnected");
            Debug.LogError(connectionId + " " + serverSessionId);
        }, (reason, message) =>
        {
            Debug.Log("OnDisconnected");
            Debug.LogError(reason + " " + message);
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            socketClientPeer.Send(new OperationRequest()
                .SetOperationCode("login")
                .SetParameters(GNHashtable.NewBuilder()
                    .Add("username", "admin")
                    .Add("password", "123456")
                    .Build()), response =>
                {
                    Debug.Log("Received from Server: " + response.ReturnCode + ", DebugMessage: " + response.DebugMessage);
                }, new SendParameters()
                {
                    Encrypted = false,
                });
        }
    }
}
```

- Attach this script to a `GameObject` in Unity and run the project. When you see the `OnConnected` log, press `Space` to send the login request.

## IV. Support

If you encounter issues or have any questions, feel free to share them on [ProtonNet Discussions](https://discussions.protonnetserver.com) to get help from the community, or contact directly via email at changx.develop@gmail.com.

**Enjoy your development with ProtonNet!**
