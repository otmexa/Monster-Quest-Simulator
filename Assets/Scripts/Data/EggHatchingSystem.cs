using UnityEngine;
using System.Collections.Generic;
using MonsterQuest.Inventory;

namespace MonsterQuest.Data
{
    public static class EggHatchingSystem
    {
        // Define base chances per rarity (Normal Egg)
        private static readonly Dictionary<ItemRarity, float> NormalChances = new Dictionary<ItemRarity, float>
        {
            { ItemRarity.Uncommon, 72f },
            { ItemRarity.Rare, 20f },
            { ItemRarity.Epic, 4.7f },
            { ItemRarity.Legendary, 0.72f },
            { ItemRarity.Mythical, 0.05f },
            { ItemRarity.Eternal, 0.01f },
            { ItemRarity.Exotic, 0.005f },
            { ItemRarity.Holo, 0.001f },
            { ItemRarity.Celestial, 0.0001f }
        };

        // VIP ignores Uncommon, shifts probabilities up
        private static readonly Dictionary<ItemRarity, float> VipChances = new Dictionary<ItemRarity, float>
        {
            { ItemRarity.Uncommon, 0f },
            { ItemRarity.Rare, 72f },
            { ItemRarity.Epic, 20f },
            { ItemRarity.Legendary, 4.7f },
            { ItemRarity.Mythical, 0.72f },
            { ItemRarity.Eternal, 0.1f },
            { ItemRarity.Exotic, 0.05f },
            { ItemRarity.Holo, 0.01f },
            { ItemRarity.Celestial, 0.005f }
        };

        public static WeaponData RollWeapon(WorldData world, bool isVip)
        {
            if (world.weapons == null || world.weapons.Count == 0) return null;

            var chances = isVip ? VipChances : NormalChances;

            float totalWeight = 0f;
            foreach (var kvp in chances) totalWeight += kvp.Value;

            float randomVal = Random.Range(0f, totalWeight);
            float currentSum = 0f;
            ItemRarity selectedRarity = ItemRarity.Uncommon;

            foreach (var kvp in chances)
            {
                currentSum += kvp.Value;
                if (randomVal <= currentSum && kvp.Value > 0)
                {
                    selectedRarity = kvp.Key;
                    break;
                }
            }

            List<WeaponData> validWeapons = new List<WeaponData>();
            foreach (var w in world.weapons)
            {
                if (w.rarity == selectedRarity) validWeapons.Add(w);
            }

            // Fallback if no weapon of that rarity exists in this world
            if (validWeapons.Count == 0)
            {
                return world.weapons[Random.Range(0, world.weapons.Count)];
            }

            return validWeapons[Random.Range(0, validWeapons.Count)];
        }
    }
}
