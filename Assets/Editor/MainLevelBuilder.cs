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
        sword.transform.SetParent(player.transform);
        sword.transform.localPosition = new Vector3(0.6f, 0, 0.5f);
        sword.transform.localScale = new Vector3(0.1f, 0.1f, 1.2f);
        sword.SetActive(false); // Hidden until selected

        // Trail Renderer
        TrailRenderer trail = sword.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.1f;
        trail.endWidth = 0.0f;
        trail.emitting = false; // Only emit on swing
        
        // Material for Trail
        Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        trailMat.color = Color.cyan;
        trail.material = trailMat;

        combat.swordPlaceholder = sword;

        // 4. VISUAL EFFECTS PREFABS
        GameObject clickParticle = GameObject.CreatePrimitive(PrimitiveType.Quad);
        clickParticle.name = "ClickParticlePrefab";
        clickParticle.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        clickParticle.transform.rotation = Quaternion.Euler(90, 0, 0);
        DestroyImmediate(clickParticle.GetComponent<Collider>());
        
        Material clickMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        clickMat.color = Color.yellow;
        clickParticle.GetComponent<Renderer>().sharedMaterial = clickMat;
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        GameObject clickPrefab = PrefabUtility.SaveAsPrefabAsset(clickParticle, "Assets/Prefabs/ClickParticle.prefab");
        DestroyImmediate(clickParticle);

        GameObject selectionRing = GameObject.CreatePrimitive(PrimitiveType.Quad);
        selectionRing.name = "SelectionRingPrefab";
        selectionRing.transform.localScale = new Vector3(2f, 2f, 2f);
        selectionRing.transform.rotation = Quaternion.Euler(90, 0, 0);
        DestroyImmediate(selectionRing.GetComponent<Collider>());
        
        Material ringMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        ringMat.color = Color.red;
        selectionRing.GetComponent<Renderer>().sharedMaterial = ringMat;

        GameObject selectionPrefab = PrefabUtility.SaveAsPrefabAsset(selectionRing, "Assets/Prefabs/SelectionRing.prefab");
        DestroyImmediate(selectionRing);

        player.GetComponent<PlayerController>().clickParticlePrefab = clickPrefab;
        visuals.selectionCirclePrefab = selectionPrefab;

        // 5. ENEMY TEST
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        enemy.name = "TestEnemy";
        enemy.AddComponent<Enemy>(); // Replaced Tag with Component for better architecture
        enemy.transform.position = new Vector3(4, 1, 4);
        Material enemyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        enemyMat.color = Color.red;
        enemy.GetComponent<Renderer>().sharedMaterial = enemyMat;

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
