using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class SetupDataManager
{
    static SetupDataManager()
    {
        EditorApplication.delayCall += RunOnce;
    }

    static void RunOnce()
    {
        // Solo ejecutamos si no estamos en modo Play
        if (Application.isPlaying) return;

        // Buscamos si ya existe
        DataManager existing = Object.FindAnyObjectByType<DataManager>();
        if (existing == null)
        {
            GameObject obj = new GameObject("DataManager");
            obj.AddComponent<DataManager>();
            
            // Forzamos que se guarde la escena
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Antigravity: ¡He creado el DataManager en tu escena automáticamente!");
        }
    }
}
