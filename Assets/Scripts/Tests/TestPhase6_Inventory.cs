using UnityEngine;
using System.Collections.Generic;
using MonsterQuest.Data;
using MonsterQuest.Inventory;

namespace MonsterQuest.Tests
{
    /// <summary>
    /// Script temporal para probar la Fase 6 (Rendimiento del UI de Inventario y Caché de Íconos).
    /// </summary>
    public class TestPhase6_Inventory : MonoBehaviour
    {
        [Header("Instrucciones: Añade este script a cualquier GameObject en la escena.")]
        [Header("Presiona T durante el juego para inyectar 50 armas de prueba al inventario.")]
        public bool activeTest = true;

        void Update()
        {
            if (!activeTest) return;

            if (Input.GetKeyDown(KeyCode.T))
            {
                InjectTestWeapons(50);
            }
        }

        private void InjectTestWeapons(int amount)
        {
            if (DataManager.Instance == null)
            {
                Debug.LogError("No se encontró DataManager. Asegúrate de dar Play primero.");
                return;
            }

            Debug.Log($"[Test Fase 6] Inyectando {amount} armas...");

            List<int> dummyIds = new List<int> { 1, 2, 3 }; // IDs de DummyWeaponCatalog

            for (int i = 0; i < amount; i++)
            {
                int randomId = dummyIds[Random.Range(0, dummyIds.Count)];
                
                // Simular que el servidor nos dio un GUID único
                WeaponInstanceData newWeapon = new WeaponInstanceData
                {
                    instanceId = System.Guid.NewGuid().ToString("N"),
                    weaponId = randomId,
                    level = Random.Range(1, 10),
                    enhancement = Random.Range(0, 3)
                };

                DataManager.Instance.Data.inventory.ownedWeapons.Add(newWeapon);
            }

            DataManager.Instance.SaveData();

            Debug.Log($"[Test Fase 6] ¡Inyección completada! Tienes {DataManager.Instance.Data.inventory.ownedWeapons.Count} armas en total.");
            Debug.Log("Ahora abre la ventana de Inventario en el juego y haz scroll rápidamente para probar cómo el CancellationToken y IconCacheService mantienen la RAM estable.");
        }
    }
}
