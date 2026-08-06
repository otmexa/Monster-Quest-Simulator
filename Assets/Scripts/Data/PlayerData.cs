using System;

[Serializable]
public class PlayerData
{
    public string username = "Player";
    public int level = 1;
    public int gold = 0;
    public int experience = 0;
    public bool hasReceivedStarterWeapon = false;

    // Instancias de las sub-clases
    public PlayerSettings settings = new PlayerSettings();
    public PlayerInventoryData inventory = new PlayerInventoryData();
}
