using UnityEngine;
using UnityEditor;
using UnityEditor.AI;

[InitializeOnLoad]
public class AutoBakeNavMesh
{
    static AutoBakeNavMesh()
    {
        EditorApplication.delayCall += DoBake;
    }

    static void DoBake()
    {
        if (SessionState.GetBool("NavMeshBakedOnce", false)) return;
        SessionState.SetBool("NavMeshBakedOnce", true);

        // Find floor objects and make them static
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int modified = 0;
        foreach (var obj in allObjects)
        {
            string n = obj.name.ToLower();
            if (n.Contains("floor") || n.Contains("suelo") || n.Contains("ground") || n.Contains("plane") || n.Contains("terrain") || n.Contains("grass"))
            {
                GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.NavigationStatic);
                modified++;
            }
        }

        if (modified > 0)
        {
            NavMeshBuilder.BuildNavMesh();
            Debug.Log($"<color=cyan><b>[IA Auto-Bake]</b> ¡NavMesh horneado automáticamente para ti! ({modified} objetos marcados como suelo)</color>");
        }
    }
}
