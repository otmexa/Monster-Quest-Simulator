using UnityEngine;
using UnityEditor;
using System.IO;

public class FixPinkMaterials
{
    [MenuItem("Antigravity/Fix Pink Materials")]
    public static void FixMaterials()
    {
        string folderPath = "Assets/3D_Assets/Items/3D Items - Mega Pack/Models";
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Antigravity: No se encontró la carpeta " + folderPath);
            // Let's try to find all FBX in the project instead if specific folder fails
            string[] allFbx = AssetDatabase.FindAssets("t:Model");
            foreach (string guid in allFbx)
            {
                ProcessModel(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
        else
        {
            string[] fbxAssets = AssetDatabase.FindAssets("t:Model", new[] { folderPath });
            foreach (string guid in fbxAssets)
            {
                ProcessModel(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        // Now fix all materials
        string[] allMaterials = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;
        foreach (string guid in allMaterials)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && mat.shader != null)
            {
                if (mat.shader.name == "Standard" || mat.shader.name == "Hidden/InternalErrorShader" || mat.shader.name.Contains("Error"))
                {
                    mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                    EditorUtility.SetDirty(mat);
                    fixedCount++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log("Antigravity: ¡Materiales arreglados con éxito! Se convirtieron " + fixedCount + " materiales a URP.");
    }

    private static void ProcessModel(string path)
    {
        if (!path.ToLower().EndsWith(".fbx") && !path.ToLower().EndsWith(".obj")) return;

        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer != null)
        {
            if (importer.materialImportMode == ModelImporterMaterialImportMode.ImportViaMaterialDescription || 
                importer.materialLocation == ModelImporterMaterialLocation.InPrefab)
            {
                importer.materialLocation = ModelImporterMaterialLocation.External;
                importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
                importer.materialSearch = ModelImporterMaterialSearch.Local;
                importer.SaveAndReimport();
            }
        }
    }
}
