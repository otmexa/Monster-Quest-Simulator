using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SetupDecals : EditorWindow
{
    [MenuItem("Tools/Setup URP Decals")]
    public static void SetupURPDecals()
    {
        // 1. Enable Decals in URP Asset
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("URP Asset not found! Ensure URP is active in Graphics Settings.");
            return;
        }

        Debug.Log("URP Decals setup initiated. Please ensure Decals are checked in your URP Asset.");
        
        // 2. Create Decal Materials
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        Texture2D circleTex = CreateCircleTexture();
        CreateDecalMaterial("Assets/Resources/PlayerRangeDecal.mat", new Color(1f, 1f, 0f, 0.2f), circleTex);
        CreateDecalMaterial("Assets/Resources/TargetPrimaryDecal.mat", Color.green, circleTex);
        CreateDecalMaterial("Assets/Resources/TargetQueuedDecal.mat", new Color(1f, 0.5f, 0f, 1f), circleTex);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Decal Materials generated successfully!");
    }

    private static void CreateDecalMaterial(string path, Color color, Texture2D circleTex)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader decalShader = Shader.Find("Shader Graphs/Decal");
            if (decalShader == null) decalShader = Shader.Find("Universal Render Pipeline/Decal");
            if (decalShader == null)
            {
                Debug.LogError("Decal shader not found! Make sure the URP package is up to date.");
                return;
            }
            
            mat = new Material(decalShader);
            mat.SetColor("_BaseColor", color);
            if (circleTex != null) mat.SetTexture("_BaseMap", circleTex);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.SetColor("_BaseColor", color);
            if (circleTex != null) mat.SetTexture("_BaseMap", circleTex);
        }
    }

    private static Texture2D CreateCircleTexture()
    {
        string path = "Assets/Resources/CircleDecalTex.png";
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null) return tex;

        int size = 256;
        tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(1, 1, 1, 0);
        Color solid = new Color(1, 1, 1, 1);
        
        float radius = size / 2f;
        float center = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist < radius)
                {
                    // Smooth edges
                    float alpha = Mathf.Clamp01((radius - dist) / 2f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, transparent);
                }
            }
        }
        
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/Resources/CircleDecalTex.png", bytes);
        AssetDatabase.ImportAsset("Assets/Resources/CircleDecalTex.png");
        
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/CircleDecalTex.png");
        if (importer != null)
        {
            importer.alphaIsTransparency = true;
            importer.textureType = TextureImporterType.Default;
            importer.SaveAndReimport();
        }
        
        return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/CircleDecalTex.png");
    }
}
