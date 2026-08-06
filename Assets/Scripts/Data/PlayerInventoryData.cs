using System;
using System.Collections.Generic;
using MonsterQuest.Data;

[Serializable]
public sealed class EquippedWeaponSlotData
{
    public int slotIndex;
    public string instanceId;
}

[Serializable]
public class PlayerInventoryData
{
    // Lista real de instancias de armas que el jugador posee
    public List<WeaponInstanceData> ownedWeapons = new List<WeaponInstanceData>();

    // Equipamiento actual, preservando el orden
    public List<EquippedWeaponSlotData> equippedWeapons = new List<EquippedWeaponSlotData>();
    
    public string equippedArmorId = "";
    
    public string equippedPetId = "";
}
