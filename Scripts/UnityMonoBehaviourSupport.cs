using System.Collections;
using System.Collections.Generic;

namespace XmobiTea.ProtonNet.Client
{
    /// <summary>
    /// UnityMonoBehaviourSupport is a class that extends UnityEngine.MonoBehaviour and provides
    /// support for managing client peer factories and running coroutines within the Unity environment.
    /// </summary>
    public interface IUnityMonoBehaviourSupport
    {
        /// <summary>
        /// Adds a client peer factory to the managed list.
        /// </summary>
        /// <param name="clientPeerFactory">The client peer factory to add.</param>
        void AddClientPeerFactory(IClientPeerFactory clientPeerFactory);

        /// <summary>
        /// Removes a client peer factory from the managed list.
        /// </summary>
        /// <param name="clientPeerFactory">The client peer factory to remove.</param>
        void RemoveClientPeerFactory(IClientPeerFactory clientPeerFactory);

        /// <summary>
        /// Runs a coroutine using Unity's StartCoroutine method.
        /// </summary>
        /// <param name="routine">The coroutine to run.</param>
        /// <returns>A Unity Coroutine object.</returns>
        UnityEngine.Coroutine RunCoroutine(IEnumerator routine);

    }

    /// <summary>
    /// UnityMonoBehaviourSupport is a class that extends UnityEngine.MonoBehaviour and provides
    /// support for managing client peer factories and running coroutines within the Unity environment.
    /// </summary>
    public class UnityMonoBehaviourSupport : UnityEngine.MonoBehaviour, IUnityMonoBehaviourSupport
    {
        /// <summary>
        /// A list that stores the client peer factories that are currently being managed.
        /// </summary>
        private IList<IClientPeerFactory> clientPeerFactoryLst = new List<IClientPeerFactory>();

        /// <summary>
        /// Adds a client peer factory to the managed list.
        /// </summary>
        /// <param name="clientPeerFactory">The client peer factory to add.</param>
        public void AddClientPeerFactory(IClientPeerFactory clientPeerFactory) => this.clientPeerFactoryLst.Add(clientPeerFactory);

        /// <summary>
        /// Removes a client peer factory from the managed list.
        /// </summary>
        /// <param name="clientPeerFactory">The client peer factory to remove.</param>
        public void RemoveClientPeerFactory(IClientPeerFactory clientPeerFactory) => this.clientPeerFactoryLst.Remove(clientPeerFactory);

        /// <summary>
        /// Runs a coroutine using Unity's StartCoroutine method.
        /// </summary>
        /// <param name="routine">The coroutine to run.</param>
        /// <returns>A Unity Coroutine object.</returns>
        public UnityEngine.Coroutine RunCoroutine(IEnumerator routine) => this.StartCoroutine(routine);

        /// <summary>
        /// Updates all client peer factories in the list by calling their Service method.
        /// This method is called once per frame by Unity's Update method.
        /// </summary>
        private void Update()
        {
            foreach (var clientPeerFactory in this.clientPeerFactoryLst)
                clientPeerFactory?.Service();
        }

    }

}
