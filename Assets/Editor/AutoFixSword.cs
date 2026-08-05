using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AutoFixSword
{
    static AutoFixSword()
    {
        EditorApplication.delayCall += RunFixOnce;
    }

    private static void RunFixOnce()
    {
        if (SessionState.GetBool("AutoFixSwordRun", false)) return;
        SessionState.SetBool("AutoFixSwordRun", true);

        // 1. Find the sword in the scene
        GameObject sword = GameObject.Find("SwordPlaceholder");
        if (sword == null)
        {
            Debug.Log("Antigravity: No encontré 'SwordPlaceholder' en la escena.");
            return;
        }

        // 2. Load the player prefab
        string prefabPath = "Assets/Prefabs/NetworkedPlayer.prefab";
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (playerPrefab == null)
        {
            Debug.LogError("Antigravity: No encontré el prefab NetworkedPlayer.");
            return;
        }

        // 3. Open prefab editing or modify it via instantiation
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        
        // Move sword to be a child of the instantiated player
        sword.transform.SetParent(instance.transform);
        // Reset local position so it stays in the hand/body relative to player center
        sword.transform.localPosition = new Vector3(0.5f, 1f, 0.5f); // approximate sword position
        sword.transform.localRotation = Quaternion.identity;

        // Apply back to prefab
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

        // 4. Clean up scene
        GameObject.DestroyImmediate(instance);

        Debug.Log("Antigravity: [AUTO] ¡La espada fue guardada exitosamente dentro del Prefab del jugador!");
        
        // 5. Delete this script
        string path = "Assets/Editor/AutoFixSword.cs";
        AssetDatabase.DeleteAsset(path);
    }
}
