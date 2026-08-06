using UnityEngine;
using System;
using System.Collections.Generic;

namespace MonsterQuest.Inventory
{
    public enum ItemType { Currency, Material, Weapon }
    public enum ItemRarity { Uncommon, Rare, Epic, Legendary, Mythical, Eternal, Exotic, Holo, Celestial }
    public enum WeaponOrientation { Horizontal, Vertical, Diagonal }

    [Serializable]
    public class WeaponData
    {
        public string id = "new_weapon";
        public string displayName = "New Weapon";
        public string description = "";
        public ItemType type = ItemType.Weapon;
        public ItemRarity rarity = ItemRarity.Uncommon;
        public WeaponOrientation orientation = WeaponOrientation.Horizontal;
        public Vector3 customRotationOffset = Vector3.zero;
        
        public int damage = 5;
        public int sellValue = 10;
        public int buyValue = 50;

        // String paths for visual resources
        public string iconName = "";
        public string prefabName = "";
    }

    [Serializable]
    public class WorldData
    {
        public string worldId = "world_1";
        public string worldName = "New World";
        public int normalEggPrice = 300;
        public int vipEggPrice = 1000;
        public List<WeaponData> weapons = new List<WeaponData>();
    }

    [Serializable]
    public class UniverseData
    {
        public string universeId = "universe_1";
        public string universeName = "Universo 1";
        public List<WorldData> worlds = new List<WorldData>();
    }

    [Serializable]
    public class GameDatabaseModel
    {
        public List<UniverseData> universes = new List<UniverseData>();
    }

    [System.Obsolete("Migrando a la nueva arquitectura Addressables. Usar WeaponCatalogService en su lugar.")]
    public static class WeaponDatabase
    {
        private static GameDatabaseModel _db;

        [Obsolete("Usar WeaponCatalogService.TryGetWeapon en su lugar.")]
        public static GameDatabaseModel GetDatabase()
        {
            if (_db == null) Load();
            return _db;
        }

        public static void Load()
        {
            TextAsset jsonText = Resources.Load<TextAsset>("Data/WeaponDatabase");
            if (jsonText != null)
            {
                _db = JsonUtility.FromJson<GameDatabaseModel>(jsonText.text);
            }
            else
            {
                // Create default structure if missing
                _db = new GameDatabaseModel();
                UniverseData defaultUniverse = new UniverseData { universeId = "uni_1", universeName = "Universo Principal" };
                defaultUniverse.worlds.Add(new WorldData { worldId = "world_1", worldName = "Bosque Inicial" });
                _db.universes.Add(defaultUniverse);
            }
        }

        public static WeaponData GetWeapon(string id)
        {
            if (_db == null) Load();
            foreach (var uni in _db.universes)
            {
                foreach (var world in uni.worlds)
                {
                    foreach (var weapon in world.weapons)
                    {
                        if (weapon.id == id) return weapon;
                    }
                }
            }
            return null;
        }

        // Obtener armas de un mundo específico
        public static List<WeaponData> GetWeaponsInWorld(string worldId)
        {
            if (_db == null) Load();
            foreach (var uni in _db.universes)
            {
                foreach (var world in uni.worlds)
                {
                    if (world.worldId == worldId) return world.weapons;
                }
            }
            return new List<WeaponData>();
        }

        #if UNITY_EDITOR
        public static void SaveDatabaseEditorOnly(GameDatabaseModel model)
        {
            _db = model;
            string json = JsonUtility.ToJson(model, true);
            string path = Application.dataPath + "/Resources/Data/WeaponDatabase.json";
            
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, json);
            // UnityEditor.AssetDatabase.Refresh(); // Removed to prevent extreme lag during typing.
        }
        #endif
    }
}
