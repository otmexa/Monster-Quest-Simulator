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
        public GameObject weaponsPopupPanel;
        public GameObject weaponDetailsPanel;

        [Header("Main HUD Elements")]
        public Text equippedCountText;
        public Button btnOpenWeapons;
        public Text goldText;

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

        private List<MockWeapon> inventory = new List<MockWeapon>();
        private MockWeapon selectedWeapon = null;
        private Sprite placeholderSprite;
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
                        weaponId = inst.weaponId, 
                        name = catalogData.localizationKey,
                        damage = catalogData.damage, 
                        sellValue = catalogData.price / 2,
                        rarityId = catalogData.rarityId,
                        iconKey = catalogData.iconKey,
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
            popupCts?.Cancel();
            popupCts = null;

            weaponsPopupPanel.SetActive(false);
            if (IconCacheService.Instance != null)
            {
                IconCacheService.Instance.ClearCache();
            }
        }

        async void LoadIconAsync(MockWeapon w, Image iconImg, CancellationToken token)
        {
            if (IconCacheService.Instance == null) return;
            
            Sprite sprite = await IconCacheService.Instance.GetIconAsync(w.iconKey, token);
            
            if (token.IsCancellationRequested) return;

            if (iconImg != null && sprite != null) 
            {
                iconImg.sprite = sprite;
                iconImg.color = Color.white;
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

                Button btn = slot.GetComponent<Button>();
                MockWeapon capturedW = w; 
                btn.onClick.AddListener(() => ShowDetails(capturedW));

                Image iconImg = new GameObject("Icon").AddComponent<Image>();
                iconImg.transform.SetParent(slot.transform, false);
                iconImg.rectTransform.anchorMin = Vector2.zero;
                iconImg.rectTransform.anchorMax = Vector2.one;
                iconImg.rectTransform.offsetMin = new Vector2(10, 10);
                iconImg.rectTransform.offsetMax = new Vector2(-10, -10);
                
                if (placeholderSprite != null)
                {
                    iconImg.sprite = placeholderSprite;
                    iconImg.color = new Color(1f, 1f, 1f, 0.5f); 
                }
                else
                {
                    switch(w.rarityId)
                    {
                        case "uncommon": iconImg.color = Color.green; break;
                        case "rare": iconImg.color = Color.blue; break;
                        case "epic": iconImg.color = new Color(0.5f, 0f, 0.5f); break; 
                        case "legendary": iconImg.color = new Color(1f, 0.5f, 0f); break; 
                        default: iconImg.color = new Color(0.2f, 0.6f, 0.9f); break;
                    }
                }

                if (popupCts != null)
                {
                    LoadIconAsync(w, iconImg, popupCts.Token);
                }

                Transform checkmark = slot.transform.Find("Checkmark");
                if (checkmark != null)
                {
                    checkmark.SetAsLastSibling(); 
                    checkmark.gameObject.SetActive(w.isEquipped);
                }
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
        }

        void OnEquipClicked()
        {
            if (selectedWeapon == null) return;

            if (selectedWeapon.isEquipped)
            {
                selectedWeapon.isEquipped = false;
                currentEquipped--;
                Update3DWeapon(selectedWeapon, false);
                
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.Data.inventory.equippedWeapons.RemoveAll(e => e.instanceId == selectedWeapon.instanceId);
                    DataManager.Instance.SaveData();
                }
            }
            else
            {
                if (currentEquipped >= maxSlots)
                {
                    Debug.LogWarning("No tienes más slots disponibles!");
                    return;
                }
                selectedWeapon.isEquipped = true;
                currentEquipped++;
                Update3DWeapon(selectedWeapon, true);
                
                if (DataManager.Instance != null)
                {
                    if (!DataManager.Instance.Data.inventory.equippedWeapons.Exists(e => e.instanceId == selectedWeapon.instanceId))
                    {
                        int nextSlot = DataManager.Instance.Data.inventory.equippedWeapons.Count;
                        DataManager.Instance.Data.inventory.equippedWeapons.Add(new EquippedWeaponSlotData { slotIndex = nextSlot, instanceId = selectedWeapon.instanceId });
                    }
                    DataManager.Instance.SaveData();
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
                    playerCombat.EquipVisualWeapon(w.instanceId); 
                else
                    playerCombat.UnequipVisualWeapon(w.instanceId);
            }
        }
    }
}
