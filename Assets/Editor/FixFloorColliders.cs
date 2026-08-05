using UnityEngine;
using UnityEditor;

public class FixFloorColliders : Editor
{
    [MenuItem("Antigravity/Fix Floor Colliders")]
    public static void Fix()
    {
        GameObject floorParent = GameObject.Find("Floor");
        if (floorParent == null)
        {
            Debug.LogError("Antigravity: No encontré el objeto llamado 'Floor'.");
            return;
        }

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
            Debug.Log($"Antigravity: ¡Le agregué MeshCollider a {collidersAdded} islas del piso!");
        }
        else
        {
            Debug.Log("Antigravity: El piso ya tenía todos sus colisionadores.");
        }
    }
}
