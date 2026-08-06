using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace MonsterQuest.Services
{
    public class WeaponVisualService : MonoBehaviour
    {
        public enum EquipState { None, Loading, Equipped, Failed }

        [Header("References")]
        [SerializeField] private Transform handBone;

        [Header("State")]
        [SerializeField] private EquipState currentState = EquipState.None;
        [SerializeField] private string currentVisualKey = string.Empty;

        private AsyncOperationHandle<GameObject> currentLoadHandle;
        private GameObject currentWeaponInstance;
        private int equipRequestVersion = 0;

        public EquipState CurrentState => currentState;
        public string CurrentVisualKey => currentVisualKey;

        public void EquipWeapon(string visualKey)
        {
            if (string.IsNullOrEmpty(visualKey))
            {
                Unequip();
                return;
            }

            if (currentVisualKey == visualKey && currentState == EquipState.Equipped)
            {
                // Ya tenemos esta arma equipada y cargada
                return;
            }

            // Aumentamos la versión para invalidar cargas asíncronas anteriores (spam de clics)
            equipRequestVersion++;
            int requestVersion = equipRequestVersion;

            // Desequipamos cualquier arma previa
            Unequip();

            currentState = EquipState.Loading;
            currentVisualKey = visualKey;
            
            LoadAndInstantiateWeapon(visualKey, requestVersion);
        }

        public void Unequip()
        {
            // Destruir la instancia actual si existe
            if (currentWeaponInstance != null)
            {
                Destroy(currentWeaponInstance);
                currentWeaponInstance = null;
            }

            // Liberar memoria (Addressables Release)
            if (currentLoadHandle.IsValid())
            {
                Addressables.Release(currentLoadHandle);
            }

            currentState = EquipState.None;
            currentVisualKey = string.Empty;
        }

        private async void LoadAndInstantiateWeapon(string visualKey, int requestVersion)
        {
            try
            {
                // Iniciar la carga asíncrona del prefab
                currentLoadHandle = Addressables.LoadAssetAsync<GameObject>(visualKey);
                
                await currentLoadHandle.Task;

                // 1. Protección contra carrera: si la versión cambió (el jugador clickeó otra arma), abortamos.
                if (requestVersion != equipRequestVersion)
                {
                    Debug.Log($"Carga abortada para {visualKey}: el jugador ya solicitó otra arma.");
                    // Addressables soltará la memoria cuando llamemos Unequip en la siguiente petición.
                    return; 
                }

                // 2. Verificar que la carga fue exitosa
                if (currentLoadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject prefab = currentLoadHandle.Result;
                    if (prefab != null)
                    {
                        currentWeaponInstance = Instantiate(prefab, handBone);
                        // Resetear transform para que encaje bien en la mano
                        currentWeaponInstance.transform.localPosition = Vector3.zero;
                        currentWeaponInstance.transform.localRotation = Quaternion.identity;
                        
                        currentState = EquipState.Equipped;
                        Debug.Log($"Arma visual instanciada: {visualKey}");
                    }
                    else
                    {
                        currentState = EquipState.Failed;
                        Debug.LogError($"El prefab para {visualKey} es null.");
                    }
                }
                else
                {
                    currentState = EquipState.Failed;
                    Debug.LogError($"Addressables falló al cargar {visualKey}");
                }
            }
            catch (System.Exception ex)
            {
                if (requestVersion == equipRequestVersion)
                {
                    currentState = EquipState.Failed;
                    Debug.LogError($"Excepción al cargar {visualKey}: {ex.Message}");
                }
            }
        }
    }
}
