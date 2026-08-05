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

    private int maxSlots = 4;
    private int currentEquipped = 0;

    // Mock data for our 3 weapons
    private List<MockWeapon> inventory = new List<MockWeapon>();
    private MockWeapon selectedWeapon = null;

    private class MockWeapon
    {
        public string id;
        public string name;
        public int damage;
        public bool isEquipped;
        public GameObject uiSlot;
    }

    void Start()
    {
        // Initialize mock inventory
        inventory.Add(new MockWeapon { id = "w1", name = "Espada Básica", damage = 5, isEquipped = false });
        inventory.Add(new MockWeapon { id = "w2", name = "Sable de Fuego", damage = 12, isEquipped = false });
        inventory.Add(new MockWeapon { id = "w3", name = "Rebanador del Rey", damage = 25, isEquipped = false });

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
            
            // Try to load our generated sword sprite to display on the slot
            Sprite swordSprite = Resources.Load<Sprite>("WeaponIcon"); 
            // Actually it's in Assets/UI/Icons. At runtime Resources.Load won't work unless it's in a Resources folder. 
            // We can just color it blue if sprite is null, or rely on the fact that the prefab has no specific icon.
            iconImg.color = new Color(0.2f, 0.6f, 0.9f); // Placeholder color for the item

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
        detailsNameText.text = w.name;
        detailsStatsText.text = $"DAÑO {w.damage}";

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

        inventory.Remove(selectedWeapon);
        weaponDetailsPanel.SetActive(false);
        selectedWeapon = null;
        
        RefreshGrid();
        capacityText.text = $"{inventory.Count}/100";
        Debug.Log("Arma vendida (+15 Monedas)");
    }

    void UpdateHUD()
    {
        equippedCountText.text = $"{currentEquipped}/{maxSlots}";
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
