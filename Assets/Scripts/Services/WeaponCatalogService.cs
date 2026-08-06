using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MonsterQuest.Data
{
    [Serializable]
    public sealed class WeaponCatalogResponse
    {
        public int schemaVersion;
        public int dataVersion;
        public WeaponData[] weapons;
    }

    [Serializable]
    public sealed class WeaponData
    {
        public int weaponId;
        public string localizationKey;
        public int damage;
        public int price;
        public string rarityId;
        public string weaponTypeId;

        public string visualKey;
        public string iconKey;

        public int requiredContentReleaseId;
        public bool enabled = true;
    }

    [Serializable]
    public sealed class WeaponInstanceData
    {
        public string instanceId;
        public int weaponId;
        public int level;
        public int enhancement;
        public int durability;
        public string[] enchantmentIds;
    }

    /// <summary>
    /// Interfaz para abstraer el origen de los datos (Local vs HTTP).
    /// </summary>
    public interface IWeaponCatalogSource
    {
        Task<string> GetCatalogJsonAsync();
    }

    /// <summary>
    /// Implementación para desarrollo local sin VPS.
    /// Lee de un archivo en Resources temporalmente.
    /// </summary>
    public class LocalWeaponCatalogSource : IWeaponCatalogSource
    {
        private string resourcesPath;

        public LocalWeaponCatalogSource(string resourcesPath = "Data/DummyWeaponCatalog")
        {
            this.resourcesPath = resourcesPath;
        }

        public Task<string> GetCatalogJsonAsync()
        {
            TextAsset textAsset = Resources.Load<TextAsset>(resourcesPath);
            if (textAsset != null)
            {
                return Task.FromResult(textAsset.text);
            }
            return Task.FromResult<string>(null);
        }
    }

    public static class WeaponCatalogService
    {
        private static Dictionary<int, WeaponData> weaponsById = new Dictionary<int, WeaponData>();
        
        public static bool IsLoaded { get; private set; }

        public static bool TryGetWeapon(int weaponId, out WeaponData weapon)
        {
            return weaponsById.TryGetValue(weaponId, out weapon);
        }

        public static IEnumerable<WeaponData> GetAllWeapons()
        {
            return weaponsById.Values;
        }

        public static async Task LoadCatalogAsync(IWeaponCatalogSource source, int currentContentReleaseId)
        {
            string json = await source.GetCatalogJsonAsync();

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("No se pudo cargar el JSON del catálogo de armas.");
                return;
            }

            WeaponCatalogResponse response = ValidateAndParse(json);
            if (response == null)
            {
                Debug.LogError("El catálogo descargado está corrupto o es incompatible.");
                return;
            }

            Dictionary<int, WeaponData> newDict = new Dictionary<int, WeaponData>();
            foreach (var weapon in response.weapons)
            {
                if (!weapon.enabled) continue;
                
                if (weapon.requiredContentReleaseId > currentContentReleaseId)
                {
                    continue; 
                }

                // Validación de campos obligatorios
                if (weapon.weaponId <= 0 || 
                    string.IsNullOrEmpty(weapon.visualKey) || 
                    string.IsNullOrEmpty(weapon.iconKey))
                {
                    Debug.LogError($"Catálogo inválido: El arma ID {weapon.weaponId} tiene campos obligatorios vacíos (ID, visualKey o iconKey). Abortando actualización.");
                    return; 
                }

                if (newDict.ContainsKey(weapon.weaponId))
                {
                    Debug.LogError($"Catálogo inválido: ID duplicado encontrado ({weapon.weaponId}). Abortando actualización.");
                    return; 
                }

                newDict[weapon.weaponId] = weapon;
            }

            weaponsById = newDict;
            IsLoaded = true;
            Debug.Log($"Catálogo de Armas cargado exitosamente. Armas disponibles: {weaponsById.Count}");
        }

        private static WeaponCatalogResponse ValidateAndParse(string json)
        {
            try
            {
                var response = JsonUtility.FromJson<WeaponCatalogResponse>(json);
                if (response == null || response.weapons == null) return null;
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON corrupto: {ex.Message}");
                return null;
            }
        }
    }
}
