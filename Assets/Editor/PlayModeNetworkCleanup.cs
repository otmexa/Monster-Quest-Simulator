using UnityEditor;
using UnityEngine;
using Unity.Netcode;

[InitializeOnLoad]
public class PlayModeNetworkCleanup
{
    static PlayModeNetworkCleanup()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                Debug.Log("[PlayModeNetworkCleanup] Apagando NetworkManager forzadamente antes de salir de PlayMode...");
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
}
