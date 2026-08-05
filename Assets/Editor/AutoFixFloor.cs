using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AutoFixFloor
{
    static AutoFixFloor()
    {
        EditorApplication.delayCall += RunFixOnce;
    }

    private static void RunFixOnce()
    {
        // Prevent running multiple times
        if (SessionState.GetBool("AutoFixFloorRun", false)) return;
        SessionState.SetBool("AutoFixFloorRun", true);

        GameObject floorParent = GameObject.Find("Floor");
        if (floorParent == null) return;

        int collidersAdded = 0;
        MeshFilter[] meshes = floorParent.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in meshes)
        {
            if (mf.gameObject.GetComponent<Collider>() == null)
            {
                mf.gameObject.AddComponent<MeshCollider>();
                collidersAdded++;
            }
        }

        if (collidersAdded > 0)
        {
            Debug.Log($"Antigravity: [AUTO] ¡Le agregué MeshCollider a {collidersAdded} islas del piso para que la moneda no se caiga!");
            
            // Delete this script automatically so it doesn't clutter the project
            string path = "Assets/Editor/AutoFixFloor.cs";
            AssetDatabase.DeleteAsset(path);
        }
    }
}
