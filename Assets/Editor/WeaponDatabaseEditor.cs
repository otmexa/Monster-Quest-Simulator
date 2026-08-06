using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MonsterQuest.Inventory;
using System.Linq;

public class WeaponDatabaseEditor : EditorWindow
{
    private GameDatabaseModel db;
    private int selectedUniverseIndex = 0;
    private int selectedWorldIndex = 0;
    
    private string previewingWeaponId = "";
    private GameObject previewObject = null;
    
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    
    private Vector2 scrollPos;
    private GUIStyle headerStyle;

    [MenuItem("Monster Quest/Weapon Database (JSON)")]
    public static void ShowWindow()
    {
        WeaponDatabaseEditor window = GetWindow<WeaponDatabaseEditor>("Weapon DB");
        window.minSize = new Vector2(900, 500);
        window.LoadDB();
    }

    private void OnEnable()
    {
        LoadDB();
    }

    private void OnDisable()
    {
        ClearPreview();
    }

    private void ClearPreview()
    {
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
            previewObject = null;
        }
        previewingWeaponId = "";
    }

    private void LoadDB()
    {
        WeaponDatabase.Load();
        db = WeaponDatabase.GetDatabase();
        if (db.universes.Count == 0)
        {
            UniverseData defaultUni = new UniverseData { universeId = "uni_1", universeName = "Universo Principal" };
            defaultUni.worlds.Add(new WorldData { worldId = "world_1", worldName = "New World" });
            db.universes.Add(defaultUni);
        }
    }

    private void SaveDB()
    {
        WeaponDatabase.SaveDatabaseEditorOnly(db);
    }

    private void OnGUI()
    {
        if (db == null) LoadDB();
        
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.alignment = TextAnchor.MiddleCenter;
        }

        DrawTopBar();
        
        if (db.universes.Count > 0 && selectedUniverseIndex < db.universes.Count)
        {
            UniverseData currentUniverse = db.universes[selectedUniverseIndex];
            DrawUniverseSettings(currentUniverse);
            
            if (currentUniverse.worlds.Count > 0 && selectedWorldIndex < currentUniverse.worlds.Count)
            {
                DrawWorldContent(currentUniverse.worlds[selectedWorldIndex]);
            }
        }
    }

    private void DrawTopBar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("Force Save JSON", EditorStyles.toolbarButton, GUILayout.Width(120)))
        {
            SaveDB();
        }
        
        GUILayout.Space(20);
        
        // UNIVERSE SELECTION
        GUILayout.Label("Universe:", GUILayout.Width(60));
        string[] uniNames = db.universes.Select(u => u.universeName).ToArray();
        
        EditorGUI.BeginChangeCheck();
        selectedUniverseIndex = EditorGUILayout.Popup(selectedUniverseIndex, uniNames, EditorStyles.toolbarPopup, GUILayout.Width(120));
        if (EditorGUI.EndChangeCheck())
        {
            selectedWorldIndex = 0; // Reset world index when changing universe
        }
        
        if (selectedUniverseIndex >= db.universes.Count) selectedUniverseIndex = 0;

        if (GUILayout.Button("+ Universe", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            UniverseData newUni = new UniverseData { 
                universeId = "uni_" + (db.universes.Count + 1), 
                universeName = "New Universe " + (db.universes.Count + 1) 
            };
            newUni.worlds.Add(new WorldData { worldId = "world_1", worldName = "New World" });
            db.universes.Add(newUni);
            selectedUniverseIndex = db.universes.Count - 1;
            selectedWorldIndex = 0;
            SaveDB();
        }

        GUILayout.Space(20);
        
        // WORLD SELECTION
        if (db.universes.Count > 0 && selectedUniverseIndex < db.universes.Count)
        {
            UniverseData currentUniverse = db.universes[selectedUniverseIndex];
            GUILayout.Label("World:", GUILayout.Width(50));
            string[] worldNames = currentUniverse.worlds.Select(w => w.worldName).ToArray();
            selectedWorldIndex = EditorGUILayout.Popup(selectedWorldIndex, worldNames, EditorStyles.toolbarPopup, GUILayout.Width(120));
            
            if (selectedWorldIndex >= currentUniverse.worlds.Count) selectedWorldIndex = 0;

            if (GUILayout.Button("+ World", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                currentUniverse.worlds.Add(new WorldData { 
                    worldId = currentUniverse.universeId + "_w" + (currentUniverse.worlds.Count + 1), 
                    worldName = "New World " + (currentUniverse.worlds.Count + 1) 
                });
                selectedWorldIndex = currentUniverse.worlds.Count - 1;
                SaveDB();
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    
    private void DrawUniverseSettings(UniverseData currentUniverse)
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Universe ID:", GUILayout.Width(75));
        GUILayout.Label(currentUniverse.universeId, EditorStyles.boldLabel, GUILayout.Width(100));
        
        GUILayout.Label("Universe Name:", GUILayout.Width(95));
        EditorGUI.BeginChangeCheck();
        currentUniverse.universeName = EditorGUILayout.TextField(currentUniverse.universeName, GUILayout.Width(150));
        if (EditorGUI.EndChangeCheck())
        {
            SaveDB();
        }
        GUILayout.EndHorizontal();
        
        // Draw a separator line
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }

    private void DrawWorldContent(WorldData currentWorld)
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("World ID:", GUILayout.Width(60));
        GUILayout.Label(currentWorld.worldId, EditorStyles.boldLabel, GUILayout.Width(150));
        
        GUILayout.Label("World Name:", GUILayout.Width(80));
        EditorGUI.BeginChangeCheck();
        currentWorld.worldName = EditorGUILayout.TextField(currentWorld.worldName, GUILayout.Width(200));
        if (EditorGUI.EndChangeCheck())
        {
            SaveDB();
        }
        
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Weapon to this World", GUILayout.Width(180), GUILayout.Height(25)))
        {
            WeaponOrientation lastOrientation = WeaponOrientation.Horizontal;
            Vector3 lastRotOffset = Vector3.zero;

            if (currentWorld.weapons.Count > 0)
            {
                var lastW = currentWorld.weapons[currentWorld.weapons.Count - 1];
                lastOrientation = lastW.orientation;
                lastRotOffset = lastW.customRotationOffset;
            }

            currentWorld.weapons.Add(new WeaponData { 
                id = currentWorld.worldId + "_w" + (currentWorld.weapons.Count + 1),
                displayName = "New Weapon",
                orientation = lastOrientation,
                customRotationOffset = lastRotOffset
            });
            SaveDB();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // Header Row
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label("Icon", headerStyle, GUILayout.Width(60));
        GUILayout.Label("Model Prefab", headerStyle, GUILayout.Width(80));
        GUILayout.Label("ID", headerStyle, GUILayout.Width(100));
        GUILayout.Label("Name", headerStyle, GUILayout.Width(120));
        GUILayout.Label("Rarity", headerStyle, GUILayout.Width(80));
        GUILayout.Label("Dmg", headerStyle, GUILayout.Width(40));
        GUILayout.Label("Buy/Sell", headerStyle, GUILayout.Width(90));
        GUILayout.Label("Orientation & Custom Rot", headerStyle, GUILayout.Width(160));
        GUILayout.FlexibleSpace();
        GUILayout.Label("Actions", headerStyle, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < currentWorld.weapons.Count; i++)
        {
            WeaponData w = currentWorld.weapons[i];
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            EditorGUI.BeginChangeCheck();

            // 1. 3D Prefab Preview
            GameObject currentPrefab = null;
            if (!string.IsNullOrEmpty(w.prefabName))
            {
                if (prefabCache.TryGetValue(w.prefabName, out GameObject cached)) currentPrefab = cached;
                else
                {
                    currentPrefab = Resources.Load<GameObject>("Prefabs/" + w.prefabName);
                    if (currentPrefab == null)
                    {
                        string[] guids = AssetDatabase.FindAssets(w.prefabName + " t:Prefab");
                        if (guids.Length > 0) currentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    }
                    prefabCache[w.prefabName] = currentPrefab;
                }
            }

            Texture2D previewTex = currentPrefab != null ? AssetPreview.GetAssetPreview(currentPrefab) : null;
            if (previewTex != null)
            {
                GUILayout.Box(previewTex, GUILayout.Width(60), GUILayout.Height(60));
            }
            else
            {
                GUILayout.Box("Loading", GUILayout.Width(60), GUILayout.Height(60));
            }

            // 2. Prefab Selection Field
            
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField(currentPrefab, typeof(GameObject), false, GUILayout.Width(80));
            if (newPrefab != currentPrefab)
            {
                w.prefabName = newPrefab != null ? newPrefab.name : "";
                prefabCache[w.prefabName] = newPrefab;
            }
            GUILayout.EndVertical();

            // 3. ID
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GUILayout.Label(w.id, EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.EndVertical();

            // 4. Name
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            w.displayName = EditorGUILayout.TextField(w.displayName, GUILayout.Width(120));
            GUILayout.EndVertical();

            // 5. Rarity
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            w.rarity = (ItemRarity)EditorGUILayout.EnumPopup(w.rarity, GUILayout.Width(80));
            GUILayout.EndVertical();

            // 6. Damage
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            w.damage = EditorGUILayout.IntField(w.damage, GUILayout.Width(40));
            GUILayout.EndVertical();

            // 7. Prices
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.Width(90));
            GUILayout.Label("B:", GUILayout.Width(15));
            w.buyValue = EditorGUILayout.IntField(w.buyValue, GUILayout.Width(35));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(90));
            GUILayout.Label("S:", GUILayout.Width(15));
            w.sellValue = EditorGUILayout.IntField(w.sellValue, GUILayout.Width(35));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            // 8. Orientation & Custom Rot
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            w.orientation = (WeaponOrientation)EditorGUILayout.EnumPopup(w.orientation, GUILayout.Width(150));
            w.customRotationOffset = EditorGUILayout.Vector3Field("", w.customRotationOffset, GUILayout.Width(150));
            GUILayout.BeginHorizontal(GUILayout.Width(150));
            if (GUILayout.Button("+90 X", EditorStyles.miniButtonLeft)) w.customRotationOffset.x = (w.customRotationOffset.x + 90f) % 360f;
            if (GUILayout.Button("+90 Y", EditorStyles.miniButtonMid)) w.customRotationOffset.y = (w.customRotationOffset.y + 90f) % 360f;
            if (GUILayout.Button("+90 Z", EditorStyles.miniButtonRight)) w.customRotationOffset.z = (w.customRotationOffset.z + 90f) % 360f;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // 9. Actions
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            
            bool isPreviewing = previewingWeaponId == w.id;
            GUI.color = isPreviewing ? Color.green : Color.white;
            if (GUILayout.Button(isPreviewing ? "Stop Preview" : "Preview 3D", GUILayout.Width(100)))
            {
                if (isPreviewing) ClearPreview();
                else StartPreview(w, currentPrefab);
            }
            GUI.color = Color.white;
            
            if (isPreviewing)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(100));
                if (GUILayout.Button("Top", EditorStyles.miniButtonLeft)) SetCameraView(Vector3.down);
                if (GUILayout.Button("Side", EditorStyles.miniButtonMid)) SetCameraView(Vector3.left);
                if (GUILayout.Button("Back", EditorStyles.miniButtonRight)) SetCameraView(Vector3.forward);
                GUILayout.EndHorizontal();
            }
            
            GUI.color = Color.red;
            if (GUILayout.Button("Delete", GUILayout.Width(100)))
            {
                if (isPreviewing) ClearPreview();
                currentWorld.weapons.RemoveAt(i);
                GUI.color = Color.white;
                SaveDB();
                break;
            }
            GUI.color = Color.white;
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                SaveDB();
                if (previewingWeaponId == w.id && previewObject != null)
                {
                    UpdatePreviewRotation(w);
                }
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
    
    private void StartPreview(WeaponData w, GameObject prefab)
    {
        ClearPreview();
        if (prefab == null) 
        {
            Debug.LogWarning("Cannot preview because the prefab is missing.");
            return;
        }

        previewingWeaponId = w.id;
        
        // We instantiate the prefab so it appears in the scene view
        previewObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        previewObject.name = "[PREVIEW] " + prefab.name;
        
        // DontSave prevents it from being saved, but keeps it visible in Hierarchy for debugging
        previewObject.hideFlags = HideFlags.DontSave;
        
        // Buscar al jugador en la escena para ponersela en la espalda
        PlayerCombat player = Object.FindFirstObjectByType<PlayerCombat>();
        if (player != null)
        {
            previewObject.transform.SetParent(player.transform);
            previewObject.transform.localPosition = new Vector3(0f, 1.0f, -2.5f); // Posición típica en la espalda
        }
        else
        {
            previewObject.transform.position = Vector3.zero;
        }
        
        // Focus the scene camera on the object
        Selection.activeGameObject = previewObject;
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }
        
        UpdatePreviewRotation(w);
    }

    private void UpdatePreviewRotation(WeaponData w)
    {
        if (previewObject == null) return;
        
        Vector3 baseRot = new Vector3(90f, 0f, 0f); // Horizontal
        if (w.orientation == WeaponOrientation.Vertical) baseRot = new Vector3(0f, 90f, 90f);
        else if (w.orientation == WeaponOrientation.Diagonal) baseRot = new Vector3(45f, 45f, 90f);
        
        previewObject.transform.localRotation = Quaternion.Euler(baseRot + w.customRotationOffset);
    }
    
    private void SetCameraView(Vector3 lookDirection)
    {
        if (SceneView.lastActiveSceneView == null || previewObject == null) return;
        
        Quaternion rot = Quaternion.LookRotation(lookDirection);
        SceneView.lastActiveSceneView.LookAt(previewObject.transform.position, rot, 3f, SceneView.lastActiveSceneView.orthographic);
    }
}
