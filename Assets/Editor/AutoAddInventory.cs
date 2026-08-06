using UnityEngine;
using UnityEditor;
using MonsterQuest.Inventory;

[InitializeOnLoad]
public class AutoAddInventory
{
    static AutoAddInventory()
    {
        EditorApplication.delayCall += DoAdd;
    }

    static void DoAdd()
    {
        if (SessionState.GetBool("AutoAddInventoryDone", false)) return;
        SessionState.SetBool("AutoAddInventoryDone", true);

        string prefabPath = "Assets/Prefabs/NetworkedPlayer.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject prefabContents = editingScope.prefabContentsRoot;
                bool modified = false;

                if (prefabContents.GetComponent<PlayerInventory>() == null)
                {
                    prefabContents.AddComponent<PlayerInventory>();
                    modified = true;
                }

                if (prefabContents.GetComponent<LootMagnet>() == null)
                {
                    prefabContents.AddComponent<LootMagnet>();
                    modified = true;
                }

                if (modified)
                {
                    Debug.Log("<color=lime><b>[IA Fix]</b> ¡Scripts vitales (PlayerInventory y LootMagnet) añadidos al prefab NetworkedPlayer!</color>");
                }
            }
        }
    }
}
