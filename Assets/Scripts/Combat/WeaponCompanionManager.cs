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
        private List<WeaponCompanionAgent> activeAgents = new List<WeaponCompanionAgent>();
        private WeaponOrbitVisualController orbitController;
        private Transform currentTarget;

        void Awake()
        {
            orbitController = GetComponent<WeaponOrbitVisualController>();
        }

        public void EquipCompanion(string instanceId)
        {
            if (activeAgents.Exists(a => a.InstanceId == instanceId)) return;

            PlayerInventory inv = GetComponent<PlayerInventory>();
            if (inv == null) return;

            WeaponInstanceData instData = inv.GetWeapon(instanceId);
            if (instData == null) return;

            if (MonsterQuest.Data.WeaponCatalogService.TryGetWeapon(instData.weaponId, out MonsterQuest.Data.WeaponData cData))
            {
                GameObject agentObj = new GameObject($"Companion_{cData.localizationKey}_{instanceId}");
                agentObj.transform.SetParent(transform);
                agentObj.transform.localPosition = Vector3.zero;

                WeaponCompanionAgent agent = agentObj.AddComponent<WeaponCompanionAgent>();
                agent.Initialize(instData, cData);
                
                if (currentTarget != null)
                {
                    agent.AssignTarget(currentTarget);
                }

                activeAgents.Add(agent);
                orbitController.RealignWeapons(activeAgents);
            }
        }

        public void UnequipCompanion(string instanceId)
        {
            WeaponCompanionAgent agent = activeAgents.Find(a => a.InstanceId == instanceId);
            if (agent != null)
            {
                activeAgents.Remove(agent);
                Destroy(agent.gameObject); // El OnDestroy del agente liberará el AssetService
                orbitController.RealignWeapons(activeAgents);
            }
        }

        public void SetTarget(Transform target)
        {
            currentTarget = target;
            foreach (var agent in activeAgents)
            {
                agent.AssignTarget(target);
            }
        }

        public void ClearTarget()
        {
            currentTarget = null;
            foreach (var agent in activeAgents)
            {
                agent.ClearTarget();
            }
        }
        
        public void RecalculateAllStats()
        {
            foreach (var agent in activeAgents)
            {
                agent.RecalculateStats();
            }
        }
    }
}
