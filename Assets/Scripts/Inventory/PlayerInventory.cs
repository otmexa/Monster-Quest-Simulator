using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MonsterQuest.UI; // <-- AGREGADO

namespace MonsterQuest.Inventory
{
    public class PlayerInventory : NetworkBehaviour
    {
        [Header("Currencies")]
        public NetworkVariable<int> goldAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> soulAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        [Header("Progression")]
        public NetworkVariable<int> experienceAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> currentLevel = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Dictionary<string, int> itemInventory = new Dictionary<string, int>();

        public event Action<int> OnGoldChanged;
        public event Action<int> OnSoulsChanged;
        public event Action<int> OnExpChanged;
        public event Action<int> OnLevelChanged;
        public event Action<WeaponData, int> OnItemAdded;

        public override void OnNetworkSpawn()
        {
            goldAmount.OnValueChanged += (oldVal, newVal) => OnGoldChanged?.Invoke(newVal);
            soulAmount.OnValueChanged += (oldVal, newVal) => OnSoulsChanged?.Invoke(newVal);
            experienceAmount.OnValueChanged += (oldVal, newVal) => OnExpChanged?.Invoke(newVal);
            currentLevel.OnValueChanged += (oldVal, newVal) => OnLevelChanged?.Invoke(newVal);

            // Cargar datos si somos dueños
            if (IsOwner && DataManager.Instance != null)
            {
                // Solo para que el UI se actualice la primera vez con nuestro dinero guardado
                AddGoldServerRpc(DataManager.Instance.Data.gold);

                // Si es su primera vez, abrimos el tutorial de selección
                if (!DataManager.Instance.Data.hasReceivedStarterWeapon)
                {
                    Debug.Log("Primera vez jugando! Llamando a WeaponSelectionUI.Open()...");
                    WeaponSelectionUI.Open();
                }
                else
                {
                    Debug.Log("El jugador ya tiene su arma inicial (hasReceivedStarterWeapon = true).");
                }
            }
        }

        [ServerRpc]
        private void AddGoldServerRpc(int initialGold)
        {
            goldAmount.Value = initialGold;
        }

        public override void OnNetworkDespawn()
        {
            goldAmount.OnValueChanged -= (oldVal, newVal) => OnGoldChanged?.Invoke(newVal);
            soulAmount.OnValueChanged -= (oldVal, newVal) => OnSoulsChanged?.Invoke(newVal);
            experienceAmount.OnValueChanged -= (oldVal, newVal) => OnExpChanged?.Invoke(newVal);
            currentLevel.OnValueChanged -= (oldVal, newVal) => OnLevelChanged?.Invoke(newVal);
        }

        // Only the Server should call this
        public void AddGold(int amount)
        {
            if (!IsServer) return;
            goldAmount.Value += amount;
        }

        public void AddSouls(int amount)
        {
            if (!IsServer) return;
            soulAmount.Value += amount;
        }

        public void AddExperience(int amount)
        {
            if (!IsServer) return;
            experienceAmount.Value += amount;
            
            while (experienceAmount.Value >= GetExpRequiredForLevel(currentLevel.Value))
            {
                experienceAmount.Value -= GetExpRequiredForLevel(currentLevel.Value);
                currentLevel.Value++;
            }
        }

        public int GetExpRequiredForLevel(int level)
        {
            return 100 * level; // Escala base, se puede ajustar después
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
