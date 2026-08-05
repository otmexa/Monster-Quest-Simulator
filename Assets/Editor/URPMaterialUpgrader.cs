using UnityEditor;
using UnityEngine;

public class URPMaterialUpgrader : EditorWindow
{
    [MenuItem("Tools/Fix Pink Monsters (Upgrade to URP)")]
    public static void UpgradeMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/monsters" });
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        
        if (urpShader == null)
        {
            Debug.LogError("URP Lit shader not found!");
            return;
        }

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && mat.shader.name != urpShader.name)
            {
                // Simple upgrade: just swap the shader. 
                // Unity usually keeps the main texture (_MainTex -> _BaseMap) if names match or if we use the RenderPipelineConverter, but for simple diffuse it often works or we map it manually.
                Texture mainTex = mat.mainTexture;
                Color color = mat.color;
                
                mat.shader = urpShader;
                mat.SetTexture("_BaseMap", mainTex);
                mat.SetColor("_BaseColor", color);
                
                EditorUtility.SetDirty(mat);
                count++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Upgraded {count} materials to URP!");
    }
}
