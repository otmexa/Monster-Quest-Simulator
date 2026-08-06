using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

public class AddressablesSetupPhase1
{
    [MenuItem("Tools/Ejecutar Fase 1 (Configurar Addressables)")]
    public static void RunSetup()
    {
        // 1. Convertir el Placeholder de icono a Sprite
        string iconPlaceholderPath = "Assets/UI/Icons/WeaponPlaceholder.jpg";
        TextureImporter importer = AssetImporter.GetAtPath(iconPlaceholderPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
            Debug.Log("[Fase 1] Icono Placeholder convertido a Sprite.");
        }

        // 2. Crear Prefab Visual Placeholder
        string baseSwordPath = "Assets/3D_Assets/Items/3D Items - Mega Pack/Prefabs/Swords/1Hand Swords/sword1h01_blue.prefab";
        string placeholderPrefabPath = "Assets/3D_Assets/Items/3D Items - Mega Pack/Prefabs/Swords/1Hand Swords/WeaponPlaceholder.prefab";
        
        if (AssetDatabase.LoadAssetAtPath<GameObject>(placeholderPrefabPath) == null)
        {
            AssetDatabase.CopyAsset(baseSwordPath, placeholderPrefabPath);
            Debug.Log("[Fase 1] Visual Placeholder Prefab creado.");
        }

        // 3. Configurar Addressables
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("No se encontraron los Settings de Addressables.");
            return;
        }

        AddressableAssetGroup coreGroup = settings.FindGroup("Weapon_Core_Local");
        if (coreGroup == null) coreGroup = settings.CreateGroup("Weapon_Core_Local", false, false, false, settings.DefaultGroup.Schemas);

        AddressableAssetGroup iconsGroup = settings.FindGroup("Weapon_Icons");
        if (iconsGroup == null) iconsGroup = settings.CreateGroup("Weapon_Icons", false, false, false, settings.DefaultGroup.Schemas);

        // Armas Visuales
        AddAddressable(settings, coreGroup, baseSwordPath, "weapon.visual.000001");
        AddAddressable(settings, coreGroup, "Assets/3D_Assets/Items/3D Items - Mega Pack/Prefabs/Axes/1Hand Axes/axe1h01_red.prefab", "weapon.visual.000002");
        AddAddressable(settings, coreGroup, "Assets/3D_Assets/Items/3D Items - Mega Pack/Prefabs/Wands and Orbs/wand01_purple.prefab", "weapon.visual.000003");
        AddAddressable(settings, coreGroup, placeholderPrefabPath, "weapon.visual.placeholder");

        // Iconos
        string sharedIconPath = "Assets/UI/Icons/WeaponIcon.jpg";
        AddAddressable(settings, iconsGroup, sharedIconPath, "weapon.icon.000001");
        AddAddressable(settings, iconsGroup, sharedIconPath, "weapon.icon.000002");
        AddAddressable(settings, iconsGroup, sharedIconPath, "weapon.icon.000003");
        AddAddressable(settings, iconsGroup, iconPlaceholderPath, "weapon.icon.placeholder");

        AssetDatabase.SaveAssets();
        Debug.Log("<color=lime><b>[Fase 1] ¡Configuración de Addressables Completada con Éxito!</b></color>");
    }

    private static void AddAddressable(AddressableAssetSettings settings, AddressableAssetGroup group, string path, string key)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError($"[Fase 1] Asset no encontrado en la ruta: {path}");
            return;
        }

        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
        entry.address = key;
    }
}
