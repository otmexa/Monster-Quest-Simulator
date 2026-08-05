using UnityEngine;

namespace MonsterQuest.Inventory
{
    public enum ItemType
    {
        Currency,   // Gold, Gems
        Material,   // Crafting materials, monster drops
        Weapon      // Swords, Bows
    }

    [CreateAssetMenu(fileName = "New Item Data", menuName = "Monster Quest/Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ItemType type;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject dropPrefab; // The 3D model that drops on the ground
        
        [Header("Stats")]
        public int sellValue;
        
        // This can be expanded later with WeaponStats, etc.
    }
}
