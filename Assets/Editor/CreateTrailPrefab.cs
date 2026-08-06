using UnityEngine;
using UnityEditor;

public class CreateTrailPrefab
{
    [MenuItem("Tools/Create Trail Prefab")]
    public static void CreatePrefab()
    {
        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        // 1. Asegurar que el material existe y está bien configurado
        string matPath = "Assets/Materials/TrailMamalon.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            // Crear el material si no existe, usando un shader súper seguro
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, matPath);
        }
        else 
        {
            // Forzar el shader correcto en caso de que esté corrupto
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            mat.shader = shader;
        }

        // Configurar el material para transparencia (Alpha Blend)
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);   // Alpha
        mat.SetColor("_BaseColor", Color.white); // Blanco para que el gradiente mande
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        
        EditorUtility.SetDirty(mat);

        // 2. Crear el GameObject y el Trail
        GameObject go = new GameObject("TrailMamalonPrefab");
        TrailRenderer trail = go.AddComponent<TrailRenderer>();
        
        trail.material = mat;
        trail.time = 0.8f;
        trail.startWidth = 0.15f;
        trail.endWidth = 0.0f;
        trail.minVertexDistance = 0.05f;

        // Configurar Degradado
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.2f, 0.7f), 0.0f), 
                new GradientColorKey(new Color(0.6f, 0f, 1f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.6f, 0.0f), // Empieza con 60% de opacidad
                new GradientAlphaKey(0.0f, 1.0f)  // Termina en 0%
            }
        );
        trail.colorGradient = grad;

        // 3. Guardar como Prefab
        string prefabPath = "Assets/Prefabs/TrailMamalonPrefab.prefab";
        
        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        GameObject.DestroyImmediate(go);
        AssetDatabase.SaveAssets();

        Debug.Log("¡Prefab TrailMamalonPrefab creado exitosamente en Assets/Prefabs!");
    }
}
