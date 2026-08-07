using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using MonsterQuest.UI;

public class SetupPhase7
{
    [MenuItem("MonsterQuest/Setup Phase 7 UI")]
    public static void RunSetup()
    {
        // 1. Añadir el Layer
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        
        bool layerExists = false;
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);
            if (layerSP.stringValue == "WeaponPreview")
            {
                layerExists = true;
                break;
            }
        }
        
        if (!layerExists)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerSP.stringValue))
                {
                    layerSP.stringValue = "WeaponPreview";
                    tagManager.ApplyModifiedProperties();
                    Debug.Log("[Setup] Layer 'WeaponPreview' creado con éxito.");
                    break;
                }
            }
        }

        // 2 y 3. Buscar el UI, añadir el RawImage y conectarlo
        WeaponInventoryUI uiScript = Object.FindObjectOfType<WeaponInventoryUI>(true);
        
        if (uiScript != null)
        {
            if (uiScript.weaponDetailsPanel != null)
            {
                // Checar si ya existe
                Transform existingPreview = uiScript.weaponDetailsPanel.transform.Find("Preview3D_RawImage");
                if (existingPreview == null)
                {
                    GameObject rawImgObj = new GameObject("Preview3D_RawImage");
                    rawImgObj.transform.SetParent(uiScript.weaponDetailsPanel.transform, false);
                    
                    RawImage rawImage = rawImgObj.AddComponent<RawImage>();
                    rawImage.color = Color.white;
                    
                    RectTransform rect = rawImage.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.1f, 0.4f);
                    rect.anchorMax = new Vector2(0.9f, 0.9f);
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    
                    uiScript.previewImage = rawImage;
                    EditorUtility.SetDirty(uiScript);
                    
                    Debug.Log("[Setup] RawImage creado y conectado exitosamente al WeaponInventoryUI.");
                }
                else
                {
                    Debug.Log("[Setup] El RawImage ya existía. Asegurando conexión...");
                    uiScript.previewImage = existingPreview.GetComponent<RawImage>();
                    EditorUtility.SetDirty(uiScript);
                }
            }
            else
            {
                Debug.LogError("[Setup] No se encontró el weaponDetailsPanel en el WeaponInventoryUI.");
            }
        }
        else
        {
            Debug.LogError("[Setup] No se encontró el script WeaponInventoryUI en la escena actual.");
        }
    }
}
