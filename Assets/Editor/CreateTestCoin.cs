using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using MonsterQuest.Inventory;

public class CreateTestCoin : Editor
{
    [MenuItem("Antigravity/Create Test Coin")]
    public static void CreateCoin()
    {
        // 1. Create ItemData for Gold
        string folder = "Assets/Data";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }

        ItemData goldData = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/GoldCoin.asset");
        if (goldData == null)
        {
            goldData = ScriptableObject.CreateInstance<ItemData>();
            goldData.id = "gold";
            goldData.displayName = "Moneda de Oro";
            goldData.type = ItemType.Currency;
            AssetDatabase.CreateAsset(goldData, "Assets/Data/GoldCoin.asset");
            AssetDatabase.SaveAssets();
        }

        // 2. Create the Coin GameObject
        GameObject coinObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coinObj.name = "Test Gold Coin";
        coinObj.transform.position = new Vector3(3, 0.5f, 3); // A bit in front of the center
        coinObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Make it yellow to look like gold
        Renderer rend = coinObj.GetComponent<Renderer>();
        if (rend != null)
        {
            Material yellowMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            yellowMat.color = Color.yellow;
            rend.material = yellowMat;
        }

        // 3. Add components
        Rigidbody rb = coinObj.AddComponent<Rigidbody>();
        rb.mass = 1f;

        PickupItem pickup = coinObj.AddComponent<PickupItem>();
        pickup.itemData = goldData;
        pickup.amount = 10;
        pickup.initialFlightSpeed = 5f;
        pickup.flightAcceleration = 20f;

        coinObj.AddComponent<NetworkObject>();

        // Set layer to something standard, or 0 (Default)
        // If we set it to Default, we need to ensure LootMagnet scans Default
        coinObj.layer = 0; 

        // Update the LootMagnet on the player to scan the Default layer just for this test
        PlayerInventory[] inventories = Object.FindObjectsByType<PlayerInventory>(FindObjectsSortMode.None);
        foreach (var inv in inventories)
        {
            LootMagnet magnet = inv.GetComponent<LootMagnet>();
            if (magnet != null)
            {
                // Add Default to layer mask (Default is layer 0, so bitmask is 1 << 0)
                magnet.lootLayer.value |= (1 << 0);
            }
        }

        // Check if there is a prefab player, update its magnet too
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NetworkedPlayer.prefab");
        if (playerPrefab != null)
        {
            LootMagnet prefabMagnet = playerPrefab.GetComponent<LootMagnet>();
            if (prefabMagnet != null)
            {
                prefabMagnet.lootLayer.value |= (1 << 0);
                PrefabUtility.SavePrefabAsset(playerPrefab);
            }
        }

        Undo.RegisterCreatedObjectUndo(coinObj, "Create Test Coin");
        Selection.activeGameObject = coinObj;
        
        Debug.Log("Antigravity: Moneda de prueba creada en la escena.");
    }
}
