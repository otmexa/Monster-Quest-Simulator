using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class AutoFixWeapons
{
    static AutoFixWeapons()
    {
        EditorApplication.delayCall += DoFix;
    }

    static void DoFix()
    {
        if (SessionState.GetBool("AutoFixWeaponsDone", false)) return;
        SessionState.SetBool("AutoFixWeaponsDone", true);

        string resourcesFolder = "Assets/Resources/Prefabs";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(resourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");
        }

        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        int moved = 0;

        foreach (string guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(path);
            
            // We only care about weapons (daggers, axes, swords) or anything inside 3D_Assets that is a prefab
            if (path.Contains("3D_Assets") && path.Contains("Prefabs") && !path.StartsWith(resourcesFolder))
            {
                string targetPath = resourcesFolder + "/" + fileName;
                
                // If it's not already in Resources/Prefabs
                if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) == null)
                {
                    string result = AssetDatabase.MoveAsset(path, targetPath);
                    if (string.IsNullOrEmpty(result))
                    {
                        moved++;
                    }
                }
            }
        }

        if (moved > 0)
        {
            Debug.Log($"<color=lime><b>[IA Fix]</b> ¡Se movieron {moved} prefabs de armas a Assets/Resources/Prefabs para que el Huevo pueda instanciarlas!</color>");
            AssetDatabase.Refresh();
        }
    }
}
