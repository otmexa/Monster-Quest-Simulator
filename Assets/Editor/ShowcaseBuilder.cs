using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class ShowcaseBuilder : EditorWindow
{
    [MenuItem("Tools/Generate Monsters Showcase")]
    public static void GenerateShowcase()
    {
        // Create a new empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "Showcase";

        // Find all prefabs in the monsters folder
        string folderPath = "Assets/monsters/Monsters Ultimate Pack 01 Cute Series";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Folder not found: " + folderPath);
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        
        int cols = 5;
        float spacing = 3.0f;
        int i = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // Skip FX or other non-character prefabs if needed, but let's just spawn all
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                int row = i / cols;
                int col = i % cols;
                instance.transform.position = new Vector3((col - cols/2f) * spacing, 0, -row * spacing);
                
                // Add a simple rotation script so they look nice
                if (instance.GetComponent<ShowcaseEntity>() == null)
                {
                    instance.AddComponent<ShowcaseEntity>();
                }
                
                i++;
            }
        }

        // Adjust camera to see them all
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 8, 8);
            mainCam.transform.rotation = Quaternion.Euler(45, 180, 0);
        }

        // Add a floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(10, 1, 10);
        
        Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = new Color(0.2f, 0.6f, 0.3f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // Save the scene
        string scenePath = "Assets/Scenes/MonsterShowcase.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Showcase generated and saved to: " + scenePath);
    }
}
