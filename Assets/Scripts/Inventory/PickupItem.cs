using UnityEngine;
using Unity.Netcode;

namespace MonsterQuest.Inventory
{
    public class PickupItem : NetworkBehaviour
    {
        [Header("Data")]
        public ItemData itemData;
        public int amount = 1;

        [Header("Flight Settings")]
        public float initialFlightSpeed = 5f;
        public float flightAcceleration = 15f;
        
        [Header("VFX")]
        public GameObject collectVFX; 

        // Network variables to sync the flight state so all clients see the item flying
        private NetworkVariable<bool> isFlying = new NetworkVariable<bool>(false);
        
        private Transform target;
        private PlayerInventory targetInventory;
        private float currentSpeed;
        private Collider col;

        void Awake()
        {
            col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true; 
        }

        public bool CanBeMagnetized()
        {
            return !isFlying.Value;
        }

        // Only the Server calls this
        public void StartMagneticFlight(Transform playerTarget, PlayerInventory inventory)
        {
            if (!IsServer || isFlying.Value) return;

            target = playerTarget;
            targetInventory = inventory;
            currentSpeed = initialFlightSpeed;
            
            isFlying.Value = true;
            
            // Tell all clients this item is now flying (so they disable physics and start Lerping it visually)
            FlightStartedClientRpc();
        }

        [ClientRpc]
        private void FlightStartedClientRpc()
        {
            if (col != null) col.enabled = false; 
            
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; 
                rb.linearVelocity = Vector3.zero;
            }
        }

        void Update()
        {
            // If it's flying on the server, we move it
            // Ideally, for perfect smoothness, clients should also predict the flight, but server authority is safer for now.
            if (!IsServer || !isFlying.Value || target == null) return;

            currentSpeed += flightAcceleration * Time.deltaTime;
            Vector3 targetPos = target.position + Vector3.up * 1f;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.5f)
            {
                Collect(targetInventory);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return; // Only server can pick things up

            if (!isFlying.Value && other.CompareTag("Player"))
            {
                Collect(other.GetComponent<PlayerInventory>());
            }
        }

        private void Collect(PlayerInventory inventory)
        {
            if (!IsServer) return;

            if (inventory != null && itemData != null)
            {
                inventory.AddItem(itemData, amount);
            }

            // Spawn VFX via ClientRpc so everyone sees it
            SpawnVFXClientRpc(transform.position);

            // Destroy over the network
            GetComponent<NetworkObject>().Despawn(true);
        }

        [ClientRpc]
        private void SpawnVFXClientRpc(Vector3 pos)
        {
            if (collectVFX != null)
            {
                Instantiate(collectVFX, pos, Quaternion.identity);
            }
        }
    }
}
