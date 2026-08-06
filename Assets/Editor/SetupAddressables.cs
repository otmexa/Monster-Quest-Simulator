using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using System.IO;

public class SetupAddressables
{
    [MenuItem("Tools/1. Configurar Addressables Base")]
    public static void Setup()
    {
        // 1. Crear Settings si no existen
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder, AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
            AddressableAssetSettingsDefaultObject.Settings = settings;
            Debug.Log("Addressables Settings creados.");
        }

        // 2. Crear Grupo Core Local
        string coreGroupName = "Weapon_Core_Local";
        AddressableAssetGroup coreGroup = settings.FindGroup(coreGroupName);
        if (coreGroup == null)
        {
            coreGroup = settings.CreateGroup(coreGroupName, false, false, true, settings.DefaultGroup.Schemas);
            Debug.Log($"Grupo creado: {coreGroupName}");
        }

        // 3. Crear Grupo Iconos
        string iconsGroupName = "Weapon_Icons";
        AddressableAssetGroup iconsGroup = settings.FindGroup(iconsGroupName);
        if (iconsGroup == null)
        {
            iconsGroup = settings.CreateGroup(iconsGroupName, false, false, true, settings.DefaultGroup.Schemas);
            Debug.Log($"Grupo creado: {iconsGroupName}");
        }

        // 4. Marcar sword1h01_blue
        string[] guids = AssetDatabase.FindAssets("sword1h01_blue t:prefab");
        if (guids.Length > 0)
        {
            string guid = guids[0];
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, coreGroup, readOnly: false, postEvent: false);
            entry.SetAddress("weapon.visual.1");
            Debug.Log("sword1h01_blue marcada como weapon.visual.1");
        }
        else
        {
            Debug.LogWarning("No se encontró sword1h01_blue.prefab en el proyecto.");
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded, coreGroup, true);
        AssetDatabase.SaveAssets();
        Debug.Log("¡Addressables configurado con éxito!");
    }

    [MenuItem("Tools/2. Probar Carga JSON")]
    public static async void TestLoadJSON()
    {
        var source = new MonsterQuest.Data.LocalWeaponCatalogSource();
        await MonsterQuest.Data.WeaponCatalogService.LoadCatalogAsync(source, 1);
        
        if (MonsterQuest.Data.WeaponCatalogService.TryGetWeapon(2, out var w)) 
        {
            Debug.Log($"¡Fase 2 Completada! Arma 2 encontrada: Daño={w.damage}, Rarity={w.rarityId}, IconKey={w.iconKey}");
        }
        else
        {
            Debug.LogError("Falló la Fase 2: No se encontró el Arma 2 en el diccionario.");
        }
    }
}
