using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class SetupNetworkManager : Editor
{
    [MenuItem("Antigravity/Setup Network Manager")]
    public static void Setup()
    {
        // 1. Setup Player Prefab
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        GameObject playerPrefab = null;

        if (player != null)
        {
            // Add NetworkObject if it doesn't have one
            if (player.GetComponent<NetworkObject>() == null)
            {
                player.gameObject.AddComponent<NetworkObject>();
            }

            // Ensure Prefabs folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Save as Prefab
            string localPath = "Assets/Prefabs/NetworkedPlayer.prefab";
            // Make sure any old prefab is overwritten or uniquely named
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            playerPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(player.gameObject, localPath, InteractionMode.UserAction);
            
            // Delete player from scene (NetworkManager will spawn it)
            Undo.DestroyObjectImmediate(player.gameObject);
            Debug.Log("Antigravity: Jugador guardado como Prefab y removido de la escena.");
        }
        else
        {
            // Try to find if there's already a prefab
            playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NetworkedPlayer.prefab");
            if (playerPrefab == null)
            {
                Debug.LogWarning("Antigravity: No se encontró un Jugador en la escena para hacer Prefab.");
            }
        }

        // 2. Setup Network Manager
        NetworkManager nm = Object.FindFirstObjectByType<NetworkManager>();
        if (nm == null)
        {
            GameObject nmObj = new GameObject("NetworkManager");
            nm = nmObj.AddComponent<NetworkManager>();
            nmObj.AddComponent<UnityTransport>();

            // Configure NetworkManager
            nm.NetworkConfig = new Unity.Netcode.NetworkConfig();
            nm.NetworkConfig.NetworkTransport = nmObj.GetComponent<UnityTransport>();

            if (playerPrefab != null)
            {
                nm.NetworkConfig.PlayerPrefab = playerPrefab;
            }

            nmObj.AddComponent<NetworkUI>();

            Undo.RegisterCreatedObjectUndo(nmObj, "Create NetworkManager");
            Debug.Log("Antigravity: NetworkManager configurado exitosamente.");
        }
        else
        {
            Debug.Log("Antigravity: El NetworkManager ya existía en la escena.");
        }

        // UI script is now handled directly on NetworkManager
    }
}
