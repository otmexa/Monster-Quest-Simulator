using UnityEngine;
using UnityEngine.UI;
using MonsterQuest.Inventory;

namespace MonsterQuest.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        private PlayerInventory localInventory;

        private Text txtLevel;
        private Text txtExp;
        private Image imgExpFill;

        private Text txtSouls;
        private Text txtGold;

        private bool isSetup = false;

        void Start()
        {
            SetupCanvas();
        }

        void Update()
        {
            if (!isSetup)
            {
                // Buscamos el inventario del jugador local (Network)
                PlayerInventory[] invs = FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None);
                foreach (var inv in invs)
                {
                    if (inv.IsOwner)
                    {
                        localInventory = inv;
                        HookEvents();
                        isSetup = true;
                        
                        UpdateExp(localInventory.experienceAmount.Value);
                        UpdateLevel(localInventory.currentLevel.Value);
                        UpdateSouls(localInventory.soulAmount.Value);
                        UpdateGold(localInventory.goldAmount.Value);
                        break;
                    }
                }
            }
        }

        private void HookEvents()
        {
            localInventory.OnExpChanged += UpdateExp;
            localInventory.OnLevelChanged += UpdateLevel;
            localInventory.OnSoulsChanged += UpdateSouls;
            localInventory.OnGoldChanged += UpdateGold;
        }

        private void OnDestroy()
        {
            if (localInventory != null)
            {
                localInventory.OnExpChanged -= UpdateExp;
                localInventory.OnLevelChanged -= UpdateLevel;
                localInventory.OnSoulsChanged -= UpdateSouls;
                localInventory.OnGoldChanged -= UpdateGold;
            }
        }

        private void SetupCanvas()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // --- Experience Bar (Top Right) ---
            GameObject expBar = CreatePill(transform, "ExpBar", new Vector2(-60, -60), font);
            txtLevel = expBar.transform.Find("Text").GetComponent<Text>();
            txtLevel.rectTransform.anchoredPosition = new Vector2(0, 10); 
            txtLevel.text = "🌟 Nivel...";
            
            // Progress Bar bg
            GameObject progBg = new GameObject("ProgBg", typeof(RectTransform));
            progBg.transform.SetParent(expBar.transform, false);
            Image pBgImg = progBg.AddComponent<Image>();
            pBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            pBgImg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            pBgImg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            pBgImg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            pBgImg.rectTransform.anchoredPosition = new Vector2(-10, -15);
            pBgImg.rectTransform.sizeDelta = new Vector2(180, 12);

            // Progress Bar fill
            GameObject progFill = new GameObject("ProgFill", typeof(RectTransform));
            progFill.transform.SetParent(progBg.transform, false);
            imgExpFill = progFill.AddComponent<Image>();
            imgExpFill.color = Color.yellow;
            imgExpFill.rectTransform.anchorMin = new Vector2(0, 0.5f);
            imgExpFill.rectTransform.anchorMax = new Vector2(0, 0.5f);
            imgExpFill.rectTransform.pivot = new Vector2(0, 0.5f);
            imgExpFill.rectTransform.anchoredPosition = new Vector2(0, 0); 
            imgExpFill.rectTransform.sizeDelta = new Vector2(0, 12);

            // Icono
            Text expIcon = expBar.transform.Find("Icon").GetComponent<Text>();
            expIcon.text = "⚔️";

            // --- Souls Bar (Middle) ---
            GameObject soulsBar = CreatePill(transform, "SoulsBar", new Vector2(-60, -140), font);
            txtSouls = soulsBar.transform.Find("Text").GetComponent<Text>();
            txtSouls.text = "0";

            Text soulsIcon = soulsBar.transform.Find("Icon").GetComponent<Text>();
            soulsIcon.text = "👻";

            // --- Gold Bar (Bottom) ---
            GameObject goldBar = CreatePill(transform, "GoldBar", new Vector2(-60, -220), font);
            txtGold = goldBar.transform.Find("Text").GetComponent<Text>();
            txtGold.text = "0";

            goldBar.transform.Find("Icon").GetComponent<Text>().text = "💎";
        }

        private GameObject CreatePill(Transform parent, string name, Vector2 pos, Font font)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            
            Image img = go.AddComponent<Image>();
            img.color = Color.white;
            
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280, 60);

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.2f, 0.3f, 1f);
            outline.effectDistance = new Vector2(3, -3);

            GameObject txtObj = new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(go.transform, false);
            Text txt = txtObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 28;
            txt.color = new Color(0.1f, 0.2f, 0.3f, 1f);
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            
            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(go.transform, false);
            Text iconTxt = iconObj.AddComponent<Text>();
            iconTxt.font = font;
            iconTxt.fontSize = 45;
            iconTxt.alignment = TextAnchor.MiddleCenter;
            
            RectTransform irt = iconObj.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(1, 0.5f);
            irt.anchorMax = new Vector2(1, 0.5f);
            irt.pivot = new Vector2(1, 0.5f);
            irt.anchoredPosition = new Vector2(15, 0); 
            irt.sizeDelta = new Vector2(70, 70);

            // Left Plus button mockup
            GameObject plusObj = new GameObject("PlusIcon", typeof(RectTransform));
            plusObj.transform.SetParent(go.transform, false);
            Image plusImg = plusObj.AddComponent<Image>();
            plusImg.color = new Color(0.2f, 0.6f, 1f);
            RectTransform prt = plusObj.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0, 0.5f);
            prt.anchorMax = new Vector2(0, 0.5f);
            prt.pivot = new Vector2(0, 0.5f);
            prt.anchoredPosition = new Vector2(-20, 0); 
            prt.sizeDelta = new Vector2(50, 50);

            return go;
        }

        private void UpdateExp(int newExp)
        {
            if (localInventory == null) return;
            int required = localInventory.GetExpRequiredForLevel(localInventory.currentLevel.Value);
            float pct = Mathf.Clamp01((float)newExp / required);
            
            if (imgExpFill != null)
            {
                imgExpFill.rectTransform.sizeDelta = new Vector2(180f * pct, 12);
            }
        }

        private void UpdateLevel(int newLevel)
        {
            if (txtLevel != null) txtLevel.text = FormatUtility.GetQiRank(newLevel);
            if (localInventory != null) UpdateExp(localInventory.experienceAmount.Value);
        }

        private void UpdateSouls(int newVal)
        {
            if (txtSouls != null) txtSouls.text = FormatUtility.FormatNumber(newVal);
        }

        private void UpdateGold(int newVal)
        {
            if (txtGold != null) txtGold.text = FormatUtility.FormatNumber(newVal);
        }
    }
}
