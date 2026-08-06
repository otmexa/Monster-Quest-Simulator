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
            currentWorld.weapons.Add(new WeaponData { 
                id = currentWorld.worldId + "_w" + (currentWorld.weapons.Count + 1),
                displayName = "New Weapon"
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
        GUILayout.Label("Orientation", headerStyle, GUILayout.Width(80));
        GUILayout.FlexibleSpace();
        GUILayout.Label("Actions", headerStyle, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < currentWorld.weapons.Count; i++)
        {
            WeaponData w = currentWorld.weapons[i];
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            EditorGUI.BeginChangeCheck();

            // 1. Icon Preview
            Sprite currentSprite = string.IsNullOrEmpty(w.iconName) ? null : Resources.Load<Sprite>("Icons/" + w.iconName);
            Sprite newSprite = (Sprite)EditorGUILayout.ObjectField(currentSprite, typeof(Sprite), false, GUILayout.Width(60), GUILayout.Height(60));
            if (newSprite != currentSprite)
            {
                w.iconName = newSprite != null ? newSprite.name : "";
            }

            // 2. Prefab Preview
            GameObject currentPrefab = string.IsNullOrEmpty(w.prefabName) ? null : Resources.Load<GameObject>("Prefabs/" + w.prefabName);
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField(currentPrefab, typeof(GameObject), false, GUILayout.Width(80));
            if (newPrefab != currentPrefab)
            {
                w.prefabName = newPrefab != null ? newPrefab.name : "";
            }
            GUILayout.EndVertical();

            // 3. ID
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            w.id = EditorGUILayout.TextField(w.id, GUILayout.Width(100));
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

            // 8. Orientation
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            w.orientation = (WeaponOrientation)EditorGUILayout.EnumPopup(w.orientation, GUILayout.Width(80));
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // 9. Actions
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
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
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}
