using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;
using MonsterQuest.Data;
using MonsterQuest.Services;

namespace MonsterQuest.UI
{
    public class WeaponInventoryUI : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject leftHUDPanel;
        public static WeaponInventoryUI Instance { get; private set; }

        public GameObject weaponSlotPrefab;
        public Transform gridContainer;
        public Sprite placeholderSprite;
        public GameObject weaponDetailsPanel;
        
        public Text detailsNameText;
        public Text detailsStatsText;
        public Text capacityText;
        
        public Text equippedCountText;
        public Button btnEquip;
        public Text btnEquipText;
        public Text goldText;
        
        public int maxSlots = 15;
        private int currentEquipped = 0;
        
        private List<MockWeapon> inventory = new List<MockWeapon>();
        private MockWeapon selectedWeapon;
        
        private MonsterQuest.Inventory.PlayerInventory localPlayerInventory;
        
        public RawImage previewImage;
        public GameObject weaponsPopupPanel;
        public Button btnOpenWeapons;
        public Button btnSell;

        [Header("Dependencies")]
        public PlayerCombat playerCombat;

        private CancellationTokenSource popupCts;

        private class MockWeapon
        {
            public string instanceId;
            public int weaponId;
            public string name;
            public int damage;
            public int sellValue;
            public string rarityId;
            public string iconKey;
            public string visualKey;
            public bool isEquipped;
            public GameObject uiSlot;
        }

        void Start()
        {
            placeholderSprite = Resources.Load<Sprite>("UI/WeaponPlaceholder");

            ReloadInventory();
            Invoke("LoadEquippedWeapons3D", 0.5f);
            
            weaponsPopupPanel.SetActive(false);
            weaponDetailsPanel.SetActive(false);
            
            btnOpenWeapons.onClick.AddListener(() => OpenWeaponsPopup());
            btnEquip.onClick.AddListener(OnEquipClicked);
            btnSell.onClick.AddListener(OnSellClicked);
            
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

        void ReloadInventory()
        {
            inventory.Clear();
            currentEquipped = 0;

            if (DataManager.Instance == null) return;
            
            var owned = DataManager.Instance.Data.inventory.ownedWeapons;
            foreach (var inst in owned)
            {
                if (MonsterQuest.Data.WeaponCatalogService.TryGetWeapon(inst.weaponId, out WeaponData catalogData))
                {
                    bool isEquipped = DataManager.Instance.Data.inventory.equippedWeapons.Exists(e => e.instanceId == inst.instanceId);
                    if (isEquipped) currentEquipped++;
                    
                    inventory.Add(new MockWeapon 
                    { 
                        instanceId = inst.instanceId,
                        weaponId = catalogData.weaponId, 
                        name = catalogData.localizationKey,
                        damage = catalogData.damage, 
                        sellValue = catalogData.price / 2, // Dummy sell value
                        rarityId = catalogData.rarityId.ToLower(),
                        iconKey = catalogData.iconKey,
                        visualKey = catalogData.visualKey,
                        isEquipped = isEquipped 
                    });
                }
            }
        }

        void OpenWeaponsPopup()
        {
            ReloadInventory();
            weaponsPopupPanel.SetActive(true);
            capacityText.text = $"{inventory.Count}/100";
            
            popupCts?.Cancel();
            popupCts = new CancellationTokenSource();
            RefreshGrid();
        }

        public void CloseWeaponsPopup()
        {
            if (WeaponPreviewService.Instance != null)
            {
                WeaponPreviewService.Instance.HidePreview();
            }

            popupCts?.Cancel();
            popupCts = null;

            weaponsPopupPanel.SetActive(false);
            if (IconCacheService.Instance != null)
            {
                IconCacheService.Instance.ClearCache();
            }
        }

        void RefreshGrid()
        {
            foreach (Transform child in gridContainer) Destroy(child.gameObject);

            foreach (var w in inventory)
            {
                GameObject slot = Instantiate(weaponSlotPrefab, gridContainer);
                slot.SetActive(true); 
                w.uiSlot = slot;

                // En lugar de añadir componentes manualmente, intentamos obtener o añadir el WeaponSlotCell
                WeaponSlotCell cell = slot.GetComponent<WeaponSlotCell>();
                if (cell == null)
                {
                    cell = slot.AddComponent<WeaponSlotCell>();
                    
                    // Configurar referencias asumiendo la estructura del prefab actual
                    cell.background = slot.GetComponent<Image>();
                    
                    Transform iconTr = slot.transform.Find("Icon");
                    if (iconTr == null)
                    {
                        GameObject iconObj = new GameObject("Icon");
                        iconObj.transform.SetParent(slot.transform, false);
                        cell.iconImage = iconObj.AddComponent<Image>();
                        cell.iconImage.rectTransform.anchorMin = Vector2.zero;
                        cell.iconImage.rectTransform.anchorMax = Vector2.one;
                        cell.iconImage.rectTransform.offsetMin = new Vector2(10, 10);
                        cell.iconImage.rectTransform.offsetMax = new Vector2(-10, -10);
                    }
                    else
                    {
                        cell.iconImage = iconTr.GetComponent<Image>();
                    }

                    Transform checkmarkTr = slot.transform.Find("Checkmark");
                    if (checkmarkTr != null)
                    {
                        cell.checkmarkObj = checkmarkTr.gameObject;
                        cell.checkmarkObj.transform.SetAsLastSibling();
                    }
                }
                
                // Extraer WeaponInstanceData y WeaponData del MockWeapon
                if (WeaponCatalogService.TryGetWeapon(w.weaponId, out WeaponData catalogData))
                {
                    WeaponInstanceData instData = new WeaponInstanceData { instanceId = w.instanceId };
                    cell.Setup(instData, catalogData, w.isEquipped, placeholderSprite);
                }

                // Subscribirse al evento de selección
                cell.OnWeaponSelected += (instanceId) => 
                {
                    MockWeapon selected = inventory.Find(x => x.instanceId == instanceId);
                    if (selected != null) ShowDetails(selected);
                };
            }
        }

        void ShowDetails(MockWeapon w)
        {
            selectedWeapon = w;
            weaponDetailsPanel.SetActive(true);
            
            string rarityColor = "#FFFFFF";
            switch(w.rarityId)
            {
                case "uncommon": rarityColor = "#00FF00"; break;
                case "rare": rarityColor = "#0080FF"; break;
                case "epic": rarityColor = "#A335EE"; break;
                case "legendary": rarityColor = "#FF8000"; break;
                case "mythical": rarityColor = "#FF0040"; break; 
            }

            detailsNameText.text = $"<color={rarityColor}>{w.name}</color>";
            detailsStatsText.text = $"DAÑO {w.damage}  |  VALOR: {w.sellValue} G";

            if (w.isEquipped)
            {
                btnEquipText.text = "Desequipar";
                btnEquip.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f); 
            }
            else
            {
                btnEquipText.text = "Equipar";
                btnEquip.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f); 
            }

            // Mostrar Preview 3D
            if (previewImage != null && WeaponPreviewService.Instance != null)
            {
                WeaponPreviewService.Instance.ShowPreview(w.visualKey, previewImage);
                
                // Asegurar que tiene el drag handler
                WeaponPreviewInputHandler handler = previewImage.GetComponent<WeaponPreviewInputHandler>();
                if (handler == null)
                {
                    handler = previewImage.gameObject.AddComponent<WeaponPreviewInputHandler>();
                }
            }
        }

        void OnEquipClicked()
        {
            if (selectedWeapon == null) return;

            EnsurePlayerInventory();

            if (selectedWeapon.isEquipped)
            {
                if (localPlayerInventory != null)
                {
                    bool success = localPlayerInventory.UnequipWeapon(selectedWeapon.instanceId);
                    if (success)
                    {
                        selectedWeapon.isEquipped = false;
                        currentEquipped--;
                    }
                }
            }
            else
            {
                if (currentEquipped >= maxSlots)
                {
                    Debug.LogWarning("No tienes mǭs slots disponibles!");
                    return;
                }
                
                if (localPlayerInventory != null)
                {
                    // Determinar siguiente slot libre
                    int nextSlot = 0;
                    if (DataManager.Instance != null)
                    {
                        while (DataManager.Instance.Data.inventory.equippedWeapons.Exists(s => s.slotIndex == nextSlot))
                        {
                            nextSlot++;
                        }
                    }

                    bool success = localPlayerInventory.EquipWeapon(selectedWeapon.instanceId, nextSlot);
                    if (success)
                    {
                        selectedWeapon.isEquipped = true;
                        currentEquipped++;
                    }
                }
            }

            UpdateHUD();
            
            popupCts?.Cancel();
            popupCts = new CancellationTokenSource();
            RefreshGrid();
            
            ShowDetails(selectedWeapon); 
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
                DataManager.Instance.Data.inventory.ownedWeapons.RemoveAll(w => w.instanceId == selectedWeapon.instanceId);
                DataManager.Instance.SaveData();
            }

            inventory.Remove(selectedWeapon);
            weaponDetailsPanel.SetActive(false);
            selectedWeapon = null;
            
            popupCts?.Cancel();
            popupCts = new CancellationTokenSource();
            RefreshGrid();
            
            capacityText.text = $"{inventory.Count}/100";
            UpdateHUD(); 
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
            // Ya no es necesario instanciar desde UI; el WeaponCompanionManager 
            // se inicializar al arrancar leyendo PlayerInventory.
        }

        void EnsurePlayerInventory()
        {
            if (localPlayerInventory == null)
            {
                GameObject player = GameObject.Find("NetworkedPlayer(Clone)"); 
                if (player == null) player = GameObject.Find("NetworkedPlayer");
                if (player != null) localPlayerInventory = player.GetComponent<MonsterQuest.Inventory.PlayerInventory>();
            }
        }
    }
}
