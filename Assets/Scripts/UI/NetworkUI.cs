using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(10, 10, 200, 60), "Iniciar HOST (Server + Jugador)"))
            {
                NetworkManager.Singleton.StartHost();
            }
            if (GUI.Button(new Rect(10, 80, 200, 60), "Iniciar CLIENTE (Solo Jugador)"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }
        else
        {
            GUI.Label(new Rect(10, 10, 300, 30), "Modo: " + (NetworkManager.Singleton.IsHost ? "Host" : "Client"));
        }
    }
}
