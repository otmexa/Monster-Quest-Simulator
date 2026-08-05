using UnityEngine;
using Unity.Netcode;

namespace MonsterQuest.Inventory
{
    public class LootMagnet : NetworkBehaviour
    {
        [Header("Magnet Settings")]
        public float magnetRadius = 5f;
        public LayerMask lootLayer;
        public float scanRate = 0.2f;

        private float nextScanTime;
        private Collider[] hitColliders = new Collider[20];

        void Update()
        {
            // Only the server should decide what gets magnetized to prevent cheating
            if (!IsServer) return;

            if (Time.time >= nextScanTime)
            {
                ScanForLoot();
                nextScanTime = Time.time + scanRate;
            }
        }

        void ScanForLoot()
        {
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, magnetRadius, hitColliders, lootLayer);
            
            for (int i = 0; i < numColliders; i++)
            {
                PickupItem item = hitColliders[i].GetComponent<PickupItem>();
                if (item != null && item.CanBeMagnetized())
                {
                    // The server commands the item to fly to this player
                    item.StartMagneticFlight(transform, GetComponent<PlayerInventory>());
                }
            }
        }
    }
}
