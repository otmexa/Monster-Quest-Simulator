using UnityEngine;
using Unity.Netcode;

public class CleanNetworkShutdown : MonoBehaviour
{
    void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
