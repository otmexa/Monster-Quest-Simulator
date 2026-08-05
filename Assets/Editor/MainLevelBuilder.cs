using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MainLevelBuilder : EditorWindow
{
    [MenuItem("Tools/Build Main Level (Core Mechanics)")]
    public static void BuildMainLevel()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "MainLevel";

        // 1. FLOOR
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(5, 1, 5);
        Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = new Color(0.2f, 0.2f, 0.2f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // 2. PLAYER CAPSULE
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1, 0);
        player.tag = "Player";

        // Add components
        player.AddComponent<NavMeshAgent>();
        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerController>();
        PlayerCombat combat = player.AddComponent<PlayerCombat>();
        TargetVisuals visuals = player.AddComponent<TargetVisuals>();

        // 3. WEAPON (CUBE)
        GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sword.name = "SwordPlaceholder";
        // Do not parent to player so it can float freely in world space
        sword.transform.position = new Vector3(0.6f, 1f, -1.5f);
        sword.transform.localScale = new Vector3(0.1f, 0.1f, 1.2f);
        sword.SetActive(false); // Hidden until selected

        // Add FloatingWeapon script
        FloatingWeapon floatingWeapon = sword.AddComponent<FloatingWeapon>();
        floatingWeapon.player = player.transform;

        // Material for Trail
        Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        trailMat.color = Color.cyan;

        // Trail Anchor (at the back tip of the sword)
        GameObject trailAnchor = new GameObject("TrailAnchor");
        trailAnchor.transform.SetParent(sword.transform);
        trailAnchor.transform.localPosition = new Vector3(0, 0, -0.5f); // Back edge of the unit cube

        // Trail Renderer on the Anchor
        TrailRenderer trail = trailAnchor.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0.0f;
        trail.emitting = false; // Only emit on swing
        trail.material = trailMat;

        combat.swordPlaceholder = sword;

        // 4. VISUAL EFFECTS PREFABS
        // 4a. Damage Number Prefab
        GameObject dmgObj = new GameObject("DamageNumberPrefab");
        Canvas dmgCanvas = dmgObj.AddComponent<Canvas>();
        dmgCanvas.renderMode = RenderMode.WorldSpace;
        dmgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 50);
        
        GameObject dmgTextObj = new GameObject("Text");
        dmgTextObj.transform.SetParent(dmgObj.transform, false);
        Text dmgText = dmgTextObj.AddComponent<Text>();
        dmgText.text = "-5";
        dmgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dmgText.color = Color.red;
        dmgText.fontSize = 40;
        dmgText.fontStyle = FontStyle.Bold;
        dmgText.alignment = TextAnchor.MiddleCenter;
        dmgTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 50);
        
        dmgObj.AddComponent<FaceCamera>();
        dmgObj.AddComponent<DamageNumber>();

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        GameObject damagePrefab = PrefabUtility.SaveAsPrefabAsset(dmgObj, "Assets/Prefabs/DamageNumber.prefab");
        DestroyImmediate(dmgObj);

        // 4b. Selection Ring (Line Renderer)
        GameObject selectionRing = new GameObject("SelectionRingPrefab");
        selectionRing.transform.rotation = Quaternion.identity; // FIX: Keep horizontal
        LineRenderer lr = selectionRing.AddComponent<LineRenderer>();
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        Material ringMat = new Material(Shader.Find("Sprites/Default")); // FIX: Sprites/Default doesn't turn magenta
        ringMat.color = Color.green; // Green for selected
        lr.sharedMaterial = ringMat;
        
        CircleRenderer cr = selectionRing.AddComponent<CircleRenderer>();
        cr.radius = 1.2f;
        cr.segments = 64;

        GameObject selectionPrefab = PrefabUtility.SaveAsPrefabAsset(selectionRing, "Assets/Prefabs/SelectionRing.prefab");
        DestroyImmediate(selectionRing);

        visuals.selectionCirclePrefab = selectionPrefab;

        // 5. ENEMY TEST
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        enemy.name = "TestEnemy";
        enemy.transform.position = new Vector3(4, 1, 4);
        Material enemyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        enemyMat.color = Color.red;
        enemyMat.EnableKeyword("_EMISSION");
        enemy.GetComponent<Renderer>().sharedMaterial = enemyMat;
        
        Enemy enemyScript = enemy.AddComponent<Enemy>();
        enemyScript.damageNumberPrefab = damagePrefab;

        // 5a. Enemy Range Circle (Line Renderer)
        GameObject eRangeCircle = new GameObject("RangeCircle");
        eRangeCircle.transform.SetParent(enemy.transform);
        eRangeCircle.transform.localPosition = new Vector3(0, -0.98f, 0);
        eRangeCircle.transform.rotation = Quaternion.identity; // FIX: Keep horizontal
        
        LineRenderer rLr = eRangeCircle.AddComponent<LineRenderer>();
        rLr.startWidth = 0.1f; // FIX: Thicker line
        rLr.endWidth = 0.1f;
        
        Material eRangeMat = new Material(Shader.Find("Sprites/Default")); // FIX: Sprites/Default
        eRangeMat.color = new Color(1, 1, 1, 0.3f);
        rLr.sharedMaterial = eRangeMat;
        
        CircleRenderer rCr = eRangeCircle.AddComponent<CircleRenderer>();
        rCr.radius = 8.0f; // matches 8.0f attack range
        rCr.segments = 64;
        
        enemyScript.rangeCircle = eRangeCircle;

        // 5b. Enemy UI (Canvas for Health + Hover)
        GameObject eCanvasObj = new GameObject("EnemyCanvas");
        eCanvasObj.transform.SetParent(enemy.transform);
        eCanvasObj.transform.localPosition = new Vector3(0, 1.8f, 0);
        Canvas eCanvas = eCanvasObj.AddComponent<Canvas>();
        eCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform eRect = eCanvasObj.GetComponent<RectTransform>();
        eRect.sizeDelta = new Vector2(200, 100);
        eRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        eCanvasObj.AddComponent<FaceCamera>();

        // Health Bar BG
        GameObject hbBg = new GameObject("HealthBarBG");
        hbBg.transform.SetParent(eCanvasObj.transform, false);
        Image bgImg = hbBg.AddComponent<Image>();
        bgImg.color = Color.black;
        RectTransform bgRect = hbBg.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(100, 15);
        bgRect.anchoredPosition = new Vector2(0, 20);

        // Health Bar Fill
        GameObject hbFill = new GameObject("HealthBarFill");
        hbFill.transform.SetParent(hbBg.transform, false);
        Image fillImg = hbFill.AddComponent<Image>();
        fillImg.color = Color.green;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        RectTransform fillRect = hbFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        enemyScript.healthBarFill = fillImg;

        // Hover Text ("Atacar")
        GameObject hoverObj = new GameObject("HoverIcon");
        hoverObj.transform.SetParent(eCanvasObj.transform, false);
        Text eText = hoverObj.AddComponent<Text>();
        eText.text = "Atacar";
        eText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        eText.color = Color.white;
        eText.fontSize = 32;
        eText.alignment = TextAnchor.MiddleCenter;
        RectTransform hRect = hoverObj.GetComponent<RectTransform>();
        hRect.sizeDelta = new Vector2(200, 50);
        hRect.anchoredPosition = new Vector2(0, -20);
        
        enemyScript.hoverCanvas = hoverObj; // Hide just this object on hover

        // 6. UI CANVAS
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("WeaponSelectionPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Buttons
        Button CreateButton(string name, string text, Vector2 pos, Transform parent)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = Color.white;
            Button btn = btnObj.AddComponent<Button>();
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchoredPosition = pos;
            btnRect.sizeDelta = new Vector2(160, 50);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            Text txt = txtObj.AddComponent<Text>();
            txt.text = text;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.color = Color.black;
            txt.alignment = TextAnchor.MiddleCenter;
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;

            return btn;
        }

        Button btnSword = CreateButton("BtnSword", "Equip Sword", new Vector2(-100, 50), panel.transform);
        Button btnAxe = CreateButton("BtnAxe", "Equip Axe", new Vector2(100, 50), panel.transform);
        Button btnConfirm = CreateButton("BtnConfirm", "CONFIRM", new Vector2(0, -50), panel.transform);

        WeaponSelectionUI uiScript = canvasObj.AddComponent<WeaponSelectionUI>();
        uiScript.selectionPanel = panel;
        uiScript.btnSword = btnSword;
        uiScript.btnAxe = btnAxe;
        uiScript.btnConfirm = btnConfirm;
        uiScript.playerCombat = combat;

        // Event System
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 10, -10);
            mainCam.transform.rotation = Quaternion.Euler(45, 0, 0);
            
            // Add Roblox Camera Controller
            CameraController camControl = mainCam.gameObject.AddComponent<CameraController>();
            camControl.target = player.transform;
        }

        // 7. SAVE SCENE FIRST (Before baking NavMesh, otherwise Unity asks for a path)
        string scenePath = "Assets/Scenes/MainLevel.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        // 8. BAKE NAVMESH
        GameObjectUtility.SetStaticEditorFlags(floor, StaticEditorFlags.NavigationStatic);
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        Debug.Log("Main Level created and saved to " + scenePath);
    }
}
