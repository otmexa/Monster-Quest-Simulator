using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeaponInventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject leftHUDPanel;
    public GameObject weaponsPopupPanel;
    public GameObject weaponDetailsPanel;

    [Header("Main HUD Elements")]
    public Text equippedCountText;
    public Button btnOpenWeapons;
    public Text goldText; // Nuevo campo para el oro

    [Header("Popup Elements")]
    public Transform gridContainer;
    public GameObject weaponSlotPrefab;
    public Text capacityText;

    [Header("Details Panel")]
    public Text detailsNameText;
    public Text detailsStatsText;
    public Button btnEquip;
    public Text btnEquipText;
    public Button btnSell;

    [Header("Dependencies")]
    public PlayerCombat playerCombat;

    private int maxSlots = 15;
    private int currentEquipped = 0;

    // Lista real de armas usando ItemData
    private List<MockWeapon> inventory = new List<MockWeapon>();
    private MockWeapon selectedWeapon = null;

    private class MockWeapon
    {
        public string id;
        public string name;
        public int damage;
        public int sellValue;
        public MonsterQuest.Inventory.ItemRarity rarity;
        public Sprite icon;
        public bool isEquipped;
        public GameObject uiSlot;
    }

    void Start()
    {
        // Cargar armas reales desde la base de datos central JSON
        var db = MonsterQuest.Inventory.WeaponDatabase.GetDatabase();
        
        // Juntamos todas las armas de todos los mundos para el inventario global
        List<MonsterQuest.Inventory.WeaponData> allItems = new List<MonsterQuest.Inventory.WeaponData>();
        foreach(var uni in db.universes)
        {
            foreach(var w in uni.worlds) 
            {
                allItems.AddRange(w.weapons);
            }
        }
        
        foreach (var item in allItems)
        {
            if (item.type != MonsterQuest.Inventory.ItemType.Weapon) continue;
            
            bool isSavedAsEquipped = false;
            
            // Revisar si está equipada en los datos guardados
            if (DataManager.Instance != null && DataManager.Instance.Data.inventory.equippedWeapons.Contains(item.id))
            {
                isSavedAsEquipped = true;
                currentEquipped++;
            }

            // Cargar el ícono desde Resources
            Sprite loadedIcon = null;
            if (!string.IsNullOrEmpty(item.iconName))
            {
                loadedIcon = Resources.Load<Sprite>("Icons/" + item.iconName);
            }

            inventory.Add(new MockWeapon 
            { 
                id = item.id, 
                name = item.displayName, 
                damage = item.damage, 
                sellValue = item.sellValue,
                rarity = item.rarity,
                icon = loadedIcon,
                isEquipped = isSavedAsEquipped 
            });
        }
        
        // Spawnear espadas equipadas físicamente en el jugador
        Invoke("LoadEquippedWeapons3D", 0.5f);

        weaponsPopupPanel.SetActive(false);
        weaponDetailsPanel.SetActive(false);

        btnOpenWeapons.onClick.AddListener(() => OpenWeaponsPopup());
        btnEquip.onClick.AddListener(OnEquipClicked);
        btnSell.onClick.AddListener(OnSellClicked);
        
        // Find player combat if not assigned
        // Auto-hook Close button if it exists
        if (weaponsPopupPanel != null)
        {
            Transform closeBtnTr = weaponsPopupPanel.transform.Find("Btn_Close");
            if (closeBtnTr != null)
            {
                Button closeBtn = closeBtnTr.GetComponent<Button>();
                if (closeBtn != null) closeBtn.onClick.AddListener(CloseWeaponsPopup);
            }
        }

        UpdateHUD();
    }

    void OpenWeaponsPopup()
    {
        weaponsPopupPanel.SetActive(true);
        capacityText.text = $"{inventory.Count}/100";
        RefreshGrid();
    }

    public void CloseWeaponsPopup()
    {
        weaponsPopupPanel.SetActive(false);
    }

    void RefreshGrid()
    {
        // Clear old
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        // Build new
        foreach (var w in inventory)
        {
            GameObject slot = Instantiate(weaponSlotPrefab, gridContainer);
            slot.SetActive(true); // VERY IMPORTANT: The prefab was inactive!
            w.uiSlot = slot;

            // Setup button
            Button btn = slot.GetComponent<Button>();
            MockWeapon capturedW = w; // capture for lambda
            btn.onClick.AddListener(() => ShowDetails(capturedW));

            // Set icon image (we add an Image child to show the weapon)
            Image iconImg = new GameObject("Icon").AddComponent<Image>();
            iconImg.transform.SetParent(slot.transform, false);
            iconImg.rectTransform.anchorMin = Vector2.zero;
            iconImg.rectTransform.anchorMax = Vector2.one;
            iconImg.rectTransform.offsetMin = new Vector2(10, 10);
            iconImg.rectTransform.offsetMax = new Vector2(-10, -10);
            
            if (w.icon != null)
            {
                iconImg.sprite = w.icon;
                iconImg.color = Color.white;
            }
            else
            {
                // Fallback color depending on rarity
                switch(w.rarity)
                {
                    case MonsterQuest.Inventory.ItemRarity.Common: iconImg.color = Color.gray; break;
                    case MonsterQuest.Inventory.ItemRarity.Uncommon: iconImg.color = Color.green; break;
                    case MonsterQuest.Inventory.ItemRarity.Rare: iconImg.color = Color.blue; break;
                    case MonsterQuest.Inventory.ItemRarity.Epic: iconImg.color = new Color(0.5f, 0f, 0.5f); break; // Purple
                    case MonsterQuest.Inventory.ItemRarity.Legendary: iconImg.color = new Color(1f, 0.5f, 0f); break; // Orange
                    default: iconImg.color = new Color(0.2f, 0.6f, 0.9f); break;
                }
            }

            // Setup checkmark
            Transform checkmark = slot.transform.Find("Checkmark");
            if (checkmark != null)
            {
                checkmark.SetAsLastSibling(); // Ensure it renders on top
                checkmark.gameObject.SetActive(w.isEquipped);
            }
        }
    }

    void ShowDetails(MockWeapon w)
    {
        selectedWeapon = w;
        weaponDetailsPanel.SetActive(true);
        
        string rarityColor = "#FFFFFF";
        switch(w.rarity)
        {
            case MonsterQuest.Inventory.ItemRarity.Common: rarityColor = "#A0A0A0"; break;
            case MonsterQuest.Inventory.ItemRarity.Uncommon: rarityColor = "#00FF00"; break;
            case MonsterQuest.Inventory.ItemRarity.Rare: rarityColor = "#0080FF"; break;
            case MonsterQuest.Inventory.ItemRarity.Epic: rarityColor = "#A335EE"; break;
            case MonsterQuest.Inventory.ItemRarity.Legendary: rarityColor = "#FF8000"; break;
        }

        detailsNameText.text = $"<color={rarityColor}>{w.name}</color>";
        detailsStatsText.text = $"DAÑO {w.damage}  |  VALOR: {w.sellValue} G";

        if (w.isEquipped)
        {
            btnEquipText.text = "Desequipar";
            btnEquip.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f); // Red
        }
        else
        {
            btnEquipText.text = "Equipar";
            btnEquip.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f); // Green
        }
    }

    void OnEquipClicked()
    {
        if (selectedWeapon == null) return;

        if (selectedWeapon.isEquipped)
        {
            // Unequip
            selectedWeapon.isEquipped = false;
            currentEquipped--;
            Update3DWeapon(selectedWeapon, false);
            
            // Guardar en JSON
            if (DataManager.Instance != null)
            {
                DataManager.Instance.Data.inventory.equippedWeapons.Remove(selectedWeapon.id);
                DataManager.Instance.SaveData();
            }
        }
        else
        {
            // Equip
            if (currentEquipped >= maxSlots)
            {
                Debug.LogWarning("No tienes más slots disponibles!");
                return;
            }
            selectedWeapon.isEquipped = true;
            currentEquipped++;
            Update3DWeapon(selectedWeapon, true);
            
            // Guardar en JSON
            if (DataManager.Instance != null)
            {
                if (!DataManager.Instance.Data.inventory.equippedWeapons.Contains(selectedWeapon.id))
                {
                    DataManager.Instance.Data.inventory.equippedWeapons.Add(selectedWeapon.id);
                }
                DataManager.Instance.SaveData();
            }
        }

        UpdateHUD();
        RefreshGrid();
        ShowDetails(selectedWeapon); // Refresh details button state
    }

    void OnSellClicked()
    {
        if (selectedWeapon == null) return;
        
        if (selectedWeapon.isEquipped)
        {
            Debug.LogWarning("No puedes vender un arma equipada.");
            return;
        }

        if (DataManager.Instance != null)
        {
            DataManager.Instance.Data.gold += selectedWeapon.sellValue;
            DataManager.Instance.SaveData();
        }

        inventory.Remove(selectedWeapon);
        weaponDetailsPanel.SetActive(false);
        selectedWeapon = null;
        
        RefreshGrid();
        capacityText.text = $"{inventory.Count}/100";
        UpdateHUD(); // To update gold text
        Debug.Log("Arma vendida");
    }

    void UpdateHUD()
    {
        equippedCountText.text = $"{currentEquipped}/{maxSlots}";
        
        if (goldText != null && DataManager.Instance != null)
        {
            goldText.text = $"Oro: {DataManager.Instance.Data.gold}";
        }
    }

    void LoadEquippedWeapons3D()
    {
        foreach (var w in inventory)
        {
            if (w.isEquipped)
            {
                Update3DWeapon(w, true);
            }
        }
    }

    void Update3DWeapon(MockWeapon w, bool equip)
    {
        if (playerCombat == null)
        {
            GameObject player = GameObject.Find("NetworkedPlayer(Clone)"); 
            if (player == null) player = GameObject.Find("NetworkedPlayer");
            if (player != null) playerCombat = player.GetComponent<PlayerCombat>();
        }

        if (playerCombat != null)
        {
            if (equip)
                playerCombat.EquipVisualWeapon(w.id);
            else
                playerCombat.UnequipVisualWeapon(w.id);
        }
    }
}
