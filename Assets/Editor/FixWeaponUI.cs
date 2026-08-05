using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixWeaponUI : EditorWindow
{
    [MenuItem("Tools/Fix Weapon UI")]
    public static void FixUI()
    {
        // 1. Fix Texture Import Settings
        string path = "Assets/UI/Icons/WeaponIcon.jpg";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
            Debug.Log("Fixed WeaponIcon texture type to Sprite.");
        }

        // 2. Destroy the broken UI
        GameObject oldUI = GameObject.Find("LeftHUDMenu");
        if (oldUI != null) DestroyImmediate(oldUI);
        GameObject oldPopup = GameObject.Find("WeaponsPopup");
        if (oldPopup != null) DestroyImmediate(oldPopup);

        WeaponInventoryUI uiLogic = Object.FindFirstObjectByType<WeaponInventoryUI>();
        if (uiLogic != null) DestroyImmediate(uiLogic);

        // 3. Find correct Canvas
        Canvas canvas = null;
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = c;
                break;
            }
        }

        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
        }

        // ==========================================
        // 1. LEFT HUD MENU
        // ==========================================
        GameObject leftHUD = new GameObject("LeftHUDMenu", typeof(RectTransform));
        leftHUD.transform.SetParent(canvas.transform, false);
        RectTransform rtLeft = leftHUD.GetComponent<RectTransform>();
        rtLeft.anchorMin = new Vector2(0, 0.5f);
        rtLeft.anchorMax = new Vector2(0, 0.5f);
        rtLeft.pivot = new Vector2(0, 0.5f);
        rtLeft.anchoredPosition = new Vector2(20, 0);
        rtLeft.sizeDelta = new Vector2(180, 500);

        VerticalLayoutGroup vlgLeft = leftHUD.AddComponent<VerticalLayoutGroup>();
        vlgLeft.spacing = 15;
        vlgLeft.childAlignment = TextAnchor.UpperCenter;
        vlgLeft.childControlWidth = false;
        vlgLeft.childControlHeight = false;

        GameObject btnShopObj = CreateButton(leftHUD.transform, "Btn_Shop", "SHOP", new Vector2(160, 80), new Color(0.3f, 0.7f, 1f));
        
        GameObject gridObj = new GameObject("HUDGrid", typeof(RectTransform));
        gridObj.transform.SetParent(leftHUD.transform, false);
        gridObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 240);
        GridLayoutGroup gridLeft = gridObj.AddComponent<GridLayoutGroup>();
        gridLeft.cellSize = new Vector2(75, 75);
        gridLeft.spacing = new Vector2(10, 10);

        GameObject btnSwordsObj = CreateButton(gridObj.transform, "Btn_Weapons", "", new Vector2(75, 75), new Color(0.2f, 0.6f, 0.9f));
        Image swordIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        swordIcon.transform.SetParent(btnSwordsObj.transform, false);
        swordIcon.rectTransform.sizeDelta = new Vector2(60, 60);
        swordIcon.rectTransform.anchoredPosition = new Vector2(0, 5);
        
        Sprite sIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/WeaponIcon.jpg");
        if (sIcon != null) swordIcon.sprite = sIcon;

        Text txtCount = new GameObject("TxtCount", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        txtCount.transform.SetParent(btnSwordsObj.transform, false);
        txtCount.rectTransform.sizeDelta = new Vector2(75, 25);
        txtCount.rectTransform.anchoredPosition = new Vector2(0, -20);
        txtCount.text = "0/4";
        txtCount.alignment = TextAnchor.MiddleCenter;
        txtCount.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtCount.color = Color.white;
        Outline countOut = txtCount.gameObject.AddComponent<Outline>();
        countOut.effectColor = Color.black;

        CreateButton(gridObj.transform, "Btn_Book", "Book", new Vector2(75, 75), new Color(0.2f, 0.6f, 0.9f));
        CreateButton(gridObj.transform, "Btn_Lock1", "Locked", new Vector2(75, 75), new Color(0.2f, 0.6f, 0.9f));
        CreateButton(gridObj.transform, "Btn_Wheel", "Wheel", new Vector2(75, 75), new Color(0.2f, 0.6f, 0.9f));
        CreateButton(gridObj.transform, "Btn_Lock2", "Locked", new Vector2(75, 75), new Color(0.2f, 0.6f, 0.9f));
        CreateButton(gridObj.transform, "Btn_Chest", "Chest", new Vector2(75, 75), new Color(0.2f, 0.6f, 0.9f));

        GameObject btnAutoObj = CreateButton(leftHUD.transform, "Btn_AutoBattle", "Luchar\nAutomáticamente", new Vector2(160, 60), new Color(0.3f, 0.7f, 1f));


        // ==========================================
        // 2. WEAPONS POPUP WINDOW
        // ==========================================
        GameObject weaponsPopup = new GameObject("WeaponsPopup", typeof(RectTransform), typeof(Image));
        weaponsPopup.transform.SetParent(canvas.transform, false);
        RectTransform rtPopup = weaponsPopup.GetComponent<RectTransform>();
        rtPopup.sizeDelta = new Vector2(800, 600);
        weaponsPopup.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);

        GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(Image));
        headerObj.transform.SetParent(weaponsPopup.transform, false);
        RectTransform rtHeader = headerObj.GetComponent<RectTransform>();
        rtHeader.anchorMin = new Vector2(0, 1); rtHeader.anchorMax = new Vector2(1, 1);
        rtHeader.pivot = new Vector2(0.5f, 1);
        rtHeader.sizeDelta = new Vector2(0, 80);
        headerObj.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.9f);
        
        Text txtTitle = new GameObject("Title", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        txtTitle.transform.SetParent(headerObj.transform, false);
        txtTitle.rectTransform.sizeDelta = new Vector2(300, 60);
        txtTitle.text = "Armas";
        txtTitle.fontSize = 40;
        txtTitle.alignment = TextAnchor.MiddleCenter;
        txtTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtTitle.color = Color.white;
        
        Text txtCapacity = new GameObject("Capacity", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        txtCapacity.transform.SetParent(weaponsPopup.transform, false);
        txtCapacity.rectTransform.anchorMin = new Vector2(0.5f, 1); txtCapacity.rectTransform.anchorMax = new Vector2(0.5f, 1);
        txtCapacity.rectTransform.anchoredPosition = new Vector2(0, -100);
        txtCapacity.rectTransform.sizeDelta = new Vector2(200, 40);
        txtCapacity.text = "3/100";
        txtCapacity.fontSize = 24;
        txtCapacity.alignment = TextAnchor.MiddleCenter;
        txtCapacity.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtCapacity.color = Color.black;

        GameObject btnClose = CreateButton(weaponsPopup.transform, "Btn_Close", "X", new Vector2(50, 50), Color.red);
        btnClose.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        btnClose.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        btnClose.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, -40);

        GameObject inventoryGrid = new GameObject("InventoryGrid", typeof(RectTransform));
        inventoryGrid.transform.SetParent(weaponsPopup.transform, false);
        RectTransform rtInvGrid = inventoryGrid.GetComponent<RectTransform>();
        rtInvGrid.anchorMin = new Vector2(0, 0); rtInvGrid.anchorMax = new Vector2(1, 1);
        rtInvGrid.offsetMin = new Vector2(30, 30); rtInvGrid.offsetMax = new Vector2(-350, -130); // Leave space on right for details
        GridLayoutGroup glgInv = inventoryGrid.AddComponent<GridLayoutGroup>();
        glgInv.cellSize = new Vector2(100, 100);
        glgInv.spacing = new Vector2(20, 20);

        GameObject slotPrefab = new GameObject("WeaponSlotPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
        slotPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        slotPrefab.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
        
        Image checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        checkmark.transform.SetParent(slotPrefab.transform, false);
        checkmark.rectTransform.anchorMin = new Vector2(1, 1); checkmark.rectTransform.anchorMax = new Vector2(1, 1);
        checkmark.rectTransform.anchoredPosition = new Vector2(-20, -20);
        checkmark.rectTransform.sizeDelta = new Vector2(40, 40);
        checkmark.color = Color.green;
        checkmark.gameObject.SetActive(false); 
        
        slotPrefab.SetActive(false);
        slotPrefab.transform.SetParent(canvas.transform, false);

        // ==========================================
        // 3. WEAPON DETAILS PANEL
        // ==========================================
        GameObject detailsPanel = new GameObject("DetailsPanel", typeof(RectTransform), typeof(Image));
        detailsPanel.transform.SetParent(weaponsPopup.transform, false);
        RectTransform rtDetails = detailsPanel.GetComponent<RectTransform>();
        rtDetails.anchorMin = new Vector2(1, 0.5f); rtDetails.anchorMax = new Vector2(1, 0.5f);
        rtDetails.pivot = new Vector2(1, 0.5f);
        rtDetails.anchoredPosition = new Vector2(-30, -20);
        rtDetails.sizeDelta = new Vector2(300, 400);
        detailsPanel.GetComponent<Image>().color = Color.white;
        Outline outDet = detailsPanel.AddComponent<Outline>();
        outDet.effectColor = Color.gray;

        Text txtDetName = new GameObject("Name", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        txtDetName.transform.SetParent(detailsPanel.transform, false);
        txtDetName.rectTransform.anchoredPosition = new Vector2(0, 150);
        txtDetName.rectTransform.sizeDelta = new Vector2(280, 60);
        txtDetName.text = "Weapon Name";
        txtDetName.fontSize = 24;
        txtDetName.alignment = TextAnchor.MiddleCenter;
        txtDetName.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtDetName.color = new Color(0.2f, 0.6f, 0.9f);
        
        Text txtDetStats = new GameObject("Stats", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        txtDetStats.transform.SetParent(detailsPanel.transform, false);
        txtDetStats.rectTransform.anchoredPosition = new Vector2(0, 50);
        txtDetStats.rectTransform.sizeDelta = new Vector2(280, 50);
        txtDetStats.text = "DAÑO 0";
        txtDetStats.fontSize = 20;
        txtDetStats.alignment = TextAnchor.MiddleCenter;
        txtDetStats.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtDetStats.color = Color.black;

        GameObject btnEquipObj = CreateButton(detailsPanel.transform, "Btn_Equip", "Equipar", new Vector2(240, 60), Color.green);
        btnEquipObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
        
        GameObject btnSellObj = CreateButton(detailsPanel.transform, "Btn_Sell", "Vender(15)", new Vector2(240, 60), new Color(1f, 0.8f, 0f));
        btnSellObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140);

        // ==========================================
        // 4. ATTACH SCRIPT
        // ==========================================
        WeaponInventoryUI uiLogicNew = canvas.gameObject.AddComponent<WeaponInventoryUI>();
        uiLogicNew.leftHUDPanel = leftHUD;
        uiLogicNew.weaponsPopupPanel = weaponsPopup;
        uiLogicNew.weaponDetailsPanel = detailsPanel;
        
        uiLogicNew.equippedCountText = txtCount;
        uiLogicNew.btnOpenWeapons = btnSwordsObj.GetComponent<Button>();
        
        uiLogicNew.gridContainer = inventoryGrid.transform;
        uiLogicNew.weaponSlotPrefab = slotPrefab;
        uiLogicNew.capacityText = txtCapacity;
        
        uiLogicNew.detailsNameText = txtDetName;
        uiLogicNew.detailsStatsText = txtDetStats;
        uiLogicNew.btnEquip = btnEquipObj.GetComponent<Button>();
        uiLogicNew.btnEquipText = btnEquipObj.GetComponentInChildren<Text>();
        uiLogicNew.btnSell = btnSellObj.GetComponent<Button>();

        btnClose.GetComponent<Button>().onClick.AddListener(() => uiLogicNew.CloseWeaponsPopup());
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 size, Color color)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        btnObj.GetComponent<RectTransform>().sizeDelta = size;
        btnObj.GetComponent<Image>().color = color;
        
        if (!string.IsNullOrEmpty(text))
        {
            Text txt = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            txt.transform.SetParent(btnObj.transform, false);
            txt.rectTransform.anchorMin = Vector2.zero; txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.sizeDelta = Vector2.zero;
            txt.text = text;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.color = Color.white;
            txt.resizeTextForBestFit = true;
            Outline outTxt = txt.gameObject.AddComponent<Outline>();
            outTxt.effectColor = Color.black;
        }
        return btnObj;
    }
}
