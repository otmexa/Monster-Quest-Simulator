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

        Texture2D yellowTex = CreateNeonRingTexture("CircleDecalTex_Yellow", new Color(1f, 0.9f, 0f, 1f), 0.01f);
        Texture2D greenTex = CreateNeonRingTexture("CircleDecalTex_Green", new Color(0f, 1f, 0f, 1f), 0.08f);
        Texture2D orangeTex = CreateNeonRingTexture("CircleDecalTex_Orange", new Color(1f, 0.5f, 0f, 1f), 0.08f);
        
        CreateDecalMaterial("Assets/Resources/PlayerRangeDecal.mat", yellowTex);
        CreateDecalMaterial("Assets/Resources/TargetPrimaryDecal.mat", greenTex);
        CreateDecalMaterial("Assets/Resources/TargetQueuedDecal.mat", orangeTex);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Decal Materials generated successfully!");
    }

    private static void CreateDecalMaterial(string path, Texture2D circleTex)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader decalShader = Shader.Find("Shader Graphs/Decal");
            if (decalShader == null) decalShader = Shader.Find("Universal Render Pipeline/Decal");
            if (decalShader == null) return;
            
            mat = new Material(decalShader);
            if (circleTex != null) mat.SetTexture("Base_Map", circleTex);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            if (circleTex != null) mat.SetTexture("Base_Map", circleTex);
        }
    }

    private static Texture2D CreateNeonRingTexture(string name, Color color, float thicknessRatio = 0.1f)
    {
        string path = $"Assets/Resources/{name}.png";
        
        int size = 512; // Increased resolution so the thin ring doesn't look pixelated
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);
        
        float radius = size / 2f;
        float center = size / 2f;
        float ringCenter = radius * (1f - thicknessRatio);
        float thickness = radius * thicknessRatio;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float diff = Mathf.Abs(dist - ringCenter);
                
                if (diff < thickness)
                {
                    float alpha = 1f - (diff / thickness);
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha * color.a));
                }
                else
                {
                    tex.SetPixel(x, y, transparent);
                }
            }
        }
        
        tex.Apply();
        System.IO.File.WriteAllBytes(Application.dataPath + $"/Resources/{name}.png", tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.alphaIsTransparency = true;
            importer.textureType = TextureImporterType.Default;
            importer.SaveAndReimport();
        }
        
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
