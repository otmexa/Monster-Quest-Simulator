using UnityEngine;
using UnityEngine.UI;
using MonsterQuest.Inventory;
using MonsterQuest.Data;
using System.Collections;
using WeaponData = MonsterQuest.Inventory.WeaponData;

namespace MonsterQuest.UI
{
    public class WeaponSelectionUI : MonoBehaviour
    {
        private static WeaponSelectionUI instance;

        private GameObject selectionPanel;
        private Button btnSword;
        private Button btnAxe;
        private Button btnConfirm;

        private string selectedWeaponId = "";
        
        private Color defaultColor = Color.white;
        private Color selectedColor = new Color(0.2f, 0.6f, 1.0f); // Blue
        private Color confirmDisabledColor = Color.gray;
        private Color confirmEnabledColor = Color.green;

        private string weaponIceId = "uni_1_w1_w1"; 
        private string weaponFireId = "uni_1_w1_w2";

        public static void Open()
        {
            if (instance == null)
            {
                GameObject go = new GameObject("WeaponSelectionUI");
                instance = go.AddComponent<WeaponSelectionUI>();
                instance.SetupUI();
            }
            
            Time.timeScale = 0f;
            instance.selectionPanel.SetActive(true);
        }

        private void SetupUI()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; 
            
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            gameObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Panel Background
            selectionPanel = new GameObject("Panel");
            selectionPanel.transform.SetParent(transform, false);
            Image bgImg = selectionPanel.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.95f);
            bgImg.rectTransform.anchorMin = Vector2.zero;
            bgImg.rectTransform.anchorMax = Vector2.one;
            bgImg.rectTransform.offsetMin = Vector2.zero;
            bgImg.rectTransform.offsetMax = Vector2.zero;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(selectionPanel.transform, false);
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 80;
            titleTxt.color = Color.white;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.text = "ELIGE TU ELEMENTO INICIAL";
            titleTxt.rectTransform.anchoredPosition = new Vector2(0, 300);
            titleTxt.rectTransform.sizeDelta = new Vector2(1200, 100);

            // Ice Button
            btnSword = CreateWeaponButton(selectionPanel.transform, "BtnIce", "❄️ Espada de Hielo", new Vector2(-300, 50), font);
            btnSword.onClick.AddListener(() => SelectWeapon(weaponIceId, btnSword));

            // Fire Button
            btnAxe = CreateWeaponButton(selectionPanel.transform, "BtnFire", "🔥 Hacha de Fuego", new Vector2(300, 50), font);
            btnAxe.onClick.AddListener(() => SelectWeapon(weaponFireId, btnAxe));

            // Confirm Button
            GameObject cBtnObj = new GameObject("BtnConfirm");
            cBtnObj.transform.SetParent(selectionPanel.transform, false);
            btnConfirm = cBtnObj.AddComponent<Button>();
            Image cBtnImg = cBtnObj.AddComponent<Image>();
            cBtnImg.color = confirmDisabledColor;
            RectTransform cBtnRt = cBtnObj.GetComponent<RectTransform>();
            cBtnRt.anchoredPosition = new Vector2(0, -250);
            cBtnRt.sizeDelta = new Vector2(400, 100);
            
            GameObject cTxtObj = new GameObject("Text");
            cTxtObj.transform.SetParent(cBtnObj.transform, false);
            Text cTxt = cTxtObj.AddComponent<Text>();
            cTxt.font = font;
            cTxt.fontSize = 50;
            cTxt.color = Color.white;
            cTxt.alignment = TextAnchor.MiddleCenter;
            cTxt.text = "CONFIRMAR";
            cTxt.rectTransform.sizeDelta = new Vector2(400, 100);

            btnConfirm.interactable = false;
            btnConfirm.onClick.AddListener(ConfirmSelection);
            
            selectionPanel.SetActive(false);
        }

        private Button CreateWeaponButton(Transform parent, string name, string textStr, Vector2 pos, Font font)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            Button btn = btnObj.AddComponent<Button>();
            Image img = btnObj.AddComponent<Image>();
            img.color = defaultColor;
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(450, 180);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            Text txt = txtObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 40;
            txt.color = Color.black;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = textStr;
            txt.rectTransform.sizeDelta = new Vector2(450, 180);

            return btn;
        }

        private void SelectWeapon(string weaponId, Button clickedBtn)
        {
            selectedWeaponId = weaponId;
            btnSword.GetComponent<Image>().color = defaultColor;
            btnAxe.GetComponent<Image>().color = defaultColor;
            clickedBtn.GetComponent<Image>().color = selectedColor;
            btnConfirm.interactable = true;
            btnConfirm.GetComponent<Image>().color = confirmEnabledColor;
        }

        private void ConfirmSelection()
        {
            if (string.IsNullOrEmpty(selectedWeaponId)) return;
            
            selectionPanel.SetActive(false);
            Time.timeScale = 1f;

            PlayerInventory[] invs = Object.FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None);
            PlayerInventory myInv = null;
            foreach(var inv in invs)
            {
                if (inv.IsOwner) myInv = inv;
            }

            if (myInv != null)
            {
                WeaponData data = WeaponDatabase.GetWeapon(selectedWeaponId);
                if (data != null)
                {
                    myInv.AddItem(data, 1);
                    EggHatchingUI.Open(data, false);
                    
                    if (DataManager.Instance != null)
                    {
                        DataManager.Instance.Data.hasReceivedStarterWeapon = true;
                        DataManager.Instance.SaveData();
                    }
                }
            }
            else
            {
                Debug.LogError("No se pudo encontrar el PlayerInventory dueño al confirmar el arma!");
            }
        }
    }
}
