using UnityEngine;
using System.Collections.Generic;
using MonsterQuest.Data;
using MonsterQuest.Services;
using MonsterQuest.Inventory;

namespace MonsterQuest.Combat
{
    [RequireComponent(typeof(WeaponOrbitVisualController))]
    public class WeaponCompanionManager : MonoBehaviour
    {
        private Dictionary<string, WeaponCompanionAgent> activeAgents = new Dictionary<string, WeaponCompanionAgent>();
        private HashSet<string> pendingAgents = new HashSet<string>();
        
        private WeaponOrbitVisualController orbitController;
        private Transform currentTarget;
        private PlayerInventory inventory;
        
        private System.Threading.CancellationTokenSource initializationCts;

        void Awake()
        {
            orbitController = GetComponent<WeaponOrbitVisualController>();
            inventory = GetComponent<PlayerInventory>();
        }

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.OnWeaponEquipped += HandleWeaponEquipped;
                inventory.OnWeaponUnequipped += HandleWeaponUnequipped;
                inventory.OnWeaponStatsChanged += HandleWeaponStatsChanged;
            }

            initializationCts = new System.Threading.CancellationTokenSource();
            _ = InitializeSafeAsync(initializationCts.Token);
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnWeaponEquipped -= HandleWeaponEquipped;
                inventory.OnWeaponUnequipped -= HandleWeaponUnequipped;
                inventory.OnWeaponStatsChanged -= HandleWeaponStatsChanged;
            }

            if (initializationCts != null)
            {
                initializationCts.Cancel();
                initializationCts.Dispose();
                initializationCts = null;
            }
        }

        private async System.Threading.Tasks.Task InitializeSafeAsync(System.Threading.CancellationToken token)
        {
            try
            {
                // Esperar a que DataManager exista y est listo
                while (DataManager.Instance == null || DataManager.Instance.Data == null)
                {
                    await System.Threading.Tasks.Task.Yield();
                    if (token.IsCancellationRequested) return;
                }

                // Asegurar que las armas pre-equipadas estn instanciadas
                var equipped = inventory.GetEquippedWeaponsOrdered();
                foreach (var slot in equipped)
                {
                    if (token.IsCancellationRequested) return;
                    _ = EnsureAgentAsync(slot.instanceId, token);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[WeaponCompanionManager] Error en la inicializacin: {ex}");
            }
        }

        private void HandleWeaponEquipped(EquippedWeaponSlotData slotData)
        {
            if (initializationCts != null && !initializationCts.IsCancellationRequested)
            {
                _ = EnsureAgentAsync(slotData.instanceId, initializationCts.Token);
            }
        }

        private void HandleWeaponUnequipped(string instanceId)
        {
            if (activeAgents.TryGetValue(instanceId, out WeaponCompanionAgent agent))
            {
                activeAgents.Remove(instanceId);
                if (agent != null) Destroy(agent.gameObject);
                RealignOrbit();
            }
            else if (pendingAgents.Contains(instanceId))
            {
                // Se intent desequipar mientras estaba cargando. Removemos para que EnsureAgentAsync aborte si an no termina.
                pendingAgents.Remove(instanceId);
            }
        }

        private void HandleWeaponStatsChanged(string instanceId)
        {
            if (activeAgents.TryGetValue(instanceId, out WeaponCompanionAgent agent))
            {
                WeaponInstanceData instData = inventory.GetWeapon(instanceId);
                if (instData != null && WeaponCatalogService.TryGetWeapon(instData.weaponId, out MonsterQuest.Data.WeaponData catalogData))
                {
                    WeaponRuntimeStats newStats = WeaponRuntimeStats.Calculate(catalogData, instData);
                    agent.UpdateRuntimeStats(newStats);
                }
            }
        }

        private async System.Threading.Tasks.Task EnsureAgentAsync(string instanceId, System.Threading.CancellationToken token)
        {
            if (activeAgents.ContainsKey(instanceId) || pendingAgents.Contains(instanceId)) return;

            pendingAgents.Add(instanceId);

            try
            {
                WeaponInstanceData instData = inventory.GetWeapon(instanceId);
                if (instData == null) return; // No es del jugador

                if (WeaponCatalogService.TryGetWeapon(instData.weaponId, out MonsterQuest.Data.WeaponData catalogData))
                {
                    GameObject agentObj = new GameObject($"Companion_{catalogData.localizationKey}_{instanceId}");
                    agentObj.transform.SetParent(transform);
                    agentObj.transform.localPosition = Vector3.zero;

                    WeaponCompanionAgent agent = agentObj.AddComponent<WeaponCompanionAgent>();
                    
                    // Inicializacin Lgica Inmediata
                    agent.Initialize(instData, catalogData);
                    
                    if (currentTarget != null)
                    {
                        agent.AssignTarget(currentTarget);
                    }

                    if (token.IsCancellationRequested || !pendingAgents.Contains(instanceId))
                    {
                        Destroy(agentObj);
                        return; // Se desequip o cancel antes de registrar
                    }

                    activeAgents[instanceId] = agent;
                    RealignOrbit();

                    // La carga visual asncrona ocurre internamente en Initialize. Si falla, el agente se mantiene en estado "VisualFailed" pero sigue atacando.
                }
                else
                {
                    Debug.LogError($"[WeaponCompanionManager] Arma {instData.weaponId} no existe en el catlogo.");
                }
            }
            finally
            {
                pendingAgents.Remove(instanceId);
            }
        }

        private void RealignOrbit()
        {
            var equippedSlots = inventory.GetEquippedWeaponsOrdered();
            List<WeaponCompanionAgent> orderedAgents = new List<WeaponCompanionAgent>();
            
            foreach (var slot in equippedSlots)
            {
                if (activeAgents.TryGetValue(slot.instanceId, out WeaponCompanionAgent agent))
                {
                    orderedAgents.Add(agent);
                }
            }
            
            orbitController.RealignWeapons(orderedAgents);
        }

        public void SetTarget(Transform target)
        {
            currentTarget = target;
            foreach (var kvp in activeAgents)
            {
                kvp.Value.AssignTarget(target);
            }
        }

        public void ClearTarget()
        {
            currentTarget = null;
            foreach (var kvp in activeAgents)
            {
                kvp.Value.ClearTarget();
            }
        }
}
}
