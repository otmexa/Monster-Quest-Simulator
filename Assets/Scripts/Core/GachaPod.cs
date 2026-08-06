using UnityEngine;
using MonsterQuest.Data;
using MonsterQuest.Inventory;
using MonsterQuest.UI;

namespace MonsterQuest.Core
{
    [RequireComponent(typeof(BoxCollider))]
    public class GachaPod : MonoBehaviour
    {
        public string worldId = "uni_1_w1";
        public bool isVip = false;
        
        private bool playerInRange = false;
        private GUIStyle labelStyle;

        private void Start()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            
            labelStyle = new GUIStyle();
            labelStyle.fontSize = 30;
            labelStyle.normal.textColor = Color.yellow;
            labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) playerInRange = false;
        }

        private void Update()
        {
            if (playerInRange)
            {
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    RollGacha();
                }
            }
        }

        private void RollGacha()
        {
            WorldData world = GetWorld();
            if (world == null) return;

            int cost = isVip ? world.vipEggPrice : world.normalEggPrice;
            
            if (DataManager.Instance != null)
            {
                if (DataManager.Instance.Data.gold < cost)
                {
                    Debug.Log("No tienes suficiente oro.");
                    return; // No puede comprar
                }
                
                DataManager.Instance.Data.gold -= cost;
                DataManager.Instance.SaveData();

                // Actualizar PlayerInventory local si existe para el UI
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var pInv = player.GetComponent<MonsterQuest.Inventory.PlayerInventory>();
                    if (pInv != null && pInv.IsOwner) pInv.AddGold(-cost);
                }
            }

            Debug.Log($"Pagando {cost} oro por el huevo...");

            WeaponData result = EggHatchingSystem.RollWeapon(world, isVip);
            if (result != null)
            {
                EggHatchingUI.Open(result);
                
                // Add to inventory
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var pInv = player.GetComponent<MonsterQuest.Inventory.PlayerInventory>();
                    if (pInv != null && pInv.IsOwner) pInv.AddItem(result, 1);
                }
            }
        }

        private WorldData GetWorld()
        {
            var db = WeaponDatabase.GetDatabase();
            foreach (var uni in db.universes)
            {
                foreach (var world in uni.worlds)
                {
                    if (world.worldId == worldId) return world;
                }
            }
            return null;
        }

        private void OnGUI()
        {
            if (playerInRange)
            {
                WorldData world = GetWorld();
                int cost = world != null ? (isVip ? world.vipEggPrice : world.normalEggPrice) : 0;
                string type = isVip ? "VIP" : "Normal";
                
                GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 50, 400, 50), 
                    $"[E] Comprar Huevo {type}\nCosto: {cost} Oro", labelStyle);
            }
        }
    }
}
