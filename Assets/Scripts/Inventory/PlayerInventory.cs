using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace MonsterQuest.Inventory
{
    public class PlayerInventory : NetworkBehaviour
    {
        [Header("Currencies")]
        // NetworkVariable ensures gold is synchronized from the server to all clients
        public NetworkVariable<int> goldAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        // Dictionary to store quantities of different items using their string ID
        // Note: Dictionaries aren't natively network-syncable easily. Usually, an MMO uses a custom NetworkList struct for the inventory.
        // For now, this is managed on the server, and we would send ClientRpc to update local UI.
        private Dictionary<string, int> itemInventory = new Dictionary<string, int>();

        // Events for UI Updates (Triggered locally)
        public event Action<int> OnGoldChanged;
        public event Action<WeaponData, int> OnItemAdded;

        public override void OnNetworkSpawn()
        {
            // Subscribe to gold changes to update UI
            goldAmount.OnValueChanged += (oldVal, newVal) => {
                OnGoldChanged?.Invoke(newVal);
            };
        }

        public override void OnNetworkDespawn()
        {
            goldAmount.OnValueChanged -= (oldVal, newVal) => {
                OnGoldChanged?.Invoke(newVal);
            };
        }

        // Only the Server should call this
        public void AddGold(int amount)
        {
            if (!IsServer) return;
            goldAmount.Value += amount;
            // The NetworkVariable automatically syncs to clients and triggers OnValueChanged
        }

        // Only the Server should call this
        public void AddItem(WeaponData itemData, int amount = 1)
        {
            if (!IsServer) return;
            if (itemData == null) return;

            if (itemData.type == ItemType.Currency)
            {
                if (itemData.id.ToLower() == "gold" || itemData.id.ToLower() == "coin")
                {
                    AddGold(amount);
                    return;
                }
            }

            if (itemInventory.ContainsKey(itemData.id))
            {
                itemInventory[itemData.id] += amount;
            }
            else
            {
                itemInventory.Add(itemData.id, amount);
            }

            // Tell the client that actually owns this player to update its UI
            UpdateClientInventoryRpc(itemData.id, itemInventory[itemData.id]);
            Debug.Log($"[Server] {gameObject.name} Collected {amount}x {itemData.displayName}. Total: {itemInventory[itemData.id]}");
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void UpdateClientInventoryRpc(string itemID, int newTotalAmount)
        {
            // Only update UI if this is our local player
            if (!IsOwner) return;

            // In a real scenario, we'd look up the ItemData from a database using the itemID
            // For now, we just invoke an update if we have a reference. 
            // The UI will likely need a more robust ItemDatabase system later.
        }

        public int GetItemCount(string itemID)
        {
            if (itemInventory.TryGetValue(itemID, out int amount))
            {
                return amount;
            }
            return 0;
        }
    }
}
