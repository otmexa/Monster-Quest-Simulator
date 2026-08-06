using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class CleanGarbageUI
{
    static CleanGarbageUI()
    {
        EditorApplication.delayCall += DoClean;
    }

    static void DoClean()
    {
        if (SessionState.GetBool("CleanGarbageUIDone", false)) return;
        SessionState.SetBool("CleanGarbageUIDone", true);

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int deleted = 0;

        foreach (var obj in allObjects)
        {
            if (obj != null && obj.name == "WeaponSelectionPanel")
            {
                Object.DestroyImmediate(obj);
                deleted++;
            }
        }

        if (deleted > 0)
        {
            Debug.Log($"<color=orange><b>[IA Cleanup]</b> ¡Basura eliminada automáticamente! Borrados {deleted} paneles fantasmas.</color>");
            
            // Forzar guardado de la escena
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }
    }
}
