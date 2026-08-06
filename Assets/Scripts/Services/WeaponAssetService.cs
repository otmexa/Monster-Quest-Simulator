using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MonsterQuest.Services
{
    public class WeaponAssetService : MonoBehaviour
    {
        private static WeaponAssetService _instance;
        public static WeaponAssetService Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("WeaponAssetService");
                    _instance = go.AddComponent<WeaponAssetService>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private class AssetData
        {
            public string visualKey;
            public AsyncOperationHandle<GameObject> handle;
            public GameObject prefab;
            public int activeInstances = 0;
            public Task<GameObject> loadTask;
        }

        private Dictionary<string, AssetData> loadedAssets = new Dictionary<string, AssetData>();

        /// <summary>
        /// Obtiene o carga asíncronamente el prefab e instancia una copia de él.
        /// Incrementa el conteo de referencias.
        /// </summary>
        public async Task<GameObject> InstantiateWeaponAsync(string visualKey, Transform parent)
        {
            if (string.IsNullOrEmpty(visualKey)) return null;

            AssetData data;
            
            // 1. Si ya está cargado o cargándose
            if (loadedAssets.TryGetValue(visualKey, out data))
            {
                // Aumentamos referencias porque vamos a devolver una nueva instancia
                data.activeInstances++;

                GameObject prefab = null;
                if (data.loadTask != null)
                {
                    prefab = await data.loadTask;
                }
                else
                {
                    prefab = data.prefab;
                }

                if (prefab != null)
                {
                    return Instantiate(prefab, parent);
                }
                else
                {
                    // Falló la carga
                    data.activeInstances--;
                    CheckAndRelease(visualKey);
                    return null;
                }
            }

            // 2. Nueva carga
            data = new AssetData { visualKey = visualKey, activeInstances = 1 };
            loadedAssets[visualKey] = data;

            var tcs = new TaskCompletionSource<GameObject>();
            data.loadTask = tcs.Task;

            try
            {
                data.handle = Addressables.LoadAssetAsync<GameObject>(visualKey);
                await data.handle.Task;

                if (data.handle.Status == AsyncOperationStatus.Succeeded && data.handle.Result != null)
                {
                    data.prefab = data.handle.Result;
                    data.loadTask = null; // Ya cargó
                    tcs.SetResult(data.prefab);
                    return Instantiate(data.prefab, parent);
                }
                else
                {
                    Debug.LogWarning($"WeaponAssetService: No se encontró {visualKey}. Cargando placeholder...");
                    data.loadTask = null;
                    tcs.SetResult(null);
                    data.activeInstances--;
                    CheckAndRelease(visualKey);
                    
                    // Fallback al placeholder
                    return await InstantiateWeaponAsync("weapon.visual.placeholder", parent);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"WeaponAssetService: Excepción cargando {visualKey} - {ex.Message}");
                data.loadTask = null;
                tcs.SetResult(null);
                data.activeInstances--;
                CheckAndRelease(visualKey);
                return null;
            }
        }

        /// <summary>
        /// Destruye la instancia específica del arma y reduce el conteo de referencias.
        /// Si el conteo llega a 0, libera el prefab de Addressables.
        /// </summary>
        public void ReleaseWeaponInstance(string visualKey, GameObject instance)
        {
            if (instance != null)
            {
                Destroy(instance);
            }

            if (string.IsNullOrEmpty(visualKey)) return;

            if (loadedAssets.TryGetValue(visualKey, out AssetData data))
            {
                data.activeInstances--;
                CheckAndRelease(visualKey);
            }
        }

        private void CheckAndRelease(string visualKey)
        {
            if (loadedAssets.TryGetValue(visualKey, out AssetData data))
            {
                if (data.activeInstances <= 0)
                {
                    if (data.handle.IsValid())
                    {
                        Addressables.Release(data.handle);
                    }
                    loadedAssets.Remove(visualKey);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var kvp in loadedAssets)
            {
                if (kvp.Value.handle.IsValid())
                {
                    Addressables.Release(kvp.Value.handle);
                }
            }
            loadedAssets.Clear();
        }
    }
}
