using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MonsterQuest.UI;
using MonsterQuest.Data;

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

        public event Action<int> OnGoldChanged;
        public event Action<int> OnSoulsChanged;
        public event Action<int> OnExpChanged;
        public event Action<int> OnLevelChanged;

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
            return 100 * level; 
        }

        // --- NEW WEAPON COMPANION ARCHITECTURE ---

        public void AddWeapon(MonsterQuest.Data.WeaponData itemData, int level = 1, int enhancement = 0, int durability = 100)
        {
            if (!IsServer) return;
            if (itemData == null) return;
            
            // Generate unique GUID for this specific weapon instance
            string newInstanceId = Guid.NewGuid().ToString();

            // Create instance data
            var instance = new WeaponInstanceData
            {
                instanceId = newInstanceId,
                weaponId = itemData.weaponId,
                level = level,
                enhancement = enhancement,
                durability = durability,
                enchantmentIds = new string[0]
            };

            // Save to DataManager (Server/Host)
            if (DataManager.Instance != null)
            {
                DataManager.Instance.Data.inventory.ownedWeapons.Add(instance);
                DataManager.Instance.SaveData();
            }

            // Sync to clients
            AddWeaponClientRpc(newInstanceId, itemData.weaponId, level, enhancement, durability);
            
            Debug.Log($"[Server] {gameObject.name} Collected new weapon instance: {newInstanceId} (Type: {itemData.weaponId})");
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void AddWeaponClientRpc(string instanceId, int weaponId, int level, int enhancement, int durability)
        {
            if (!IsOwner) return;

            // En un cliente puro sin DataManager de Host, esto serviría para actualizar la UI visualmente.
            // Actualmente en el prototipo, DataManager centraliza el Host, así que UI lee directo de ahí.
        }

        public WeaponInstanceData GetWeapon(string instanceId)
        {
            if (DataManager.Instance != null)
            {
                return DataManager.Instance.Data.inventory.ownedWeapons.Find(w => w.instanceId == instanceId);
            }
            return null;
        }

        public int GetWeaponCount(int weaponId)
        {
            if (DataManager.Instance != null)
            {
                int count = 0;
                foreach (var w in DataManager.Instance.Data.inventory.ownedWeapons)
                {
                    if (w.weaponId == weaponId) count++;
                }
                return count;
            }
            return 0;
        }
    }
}
