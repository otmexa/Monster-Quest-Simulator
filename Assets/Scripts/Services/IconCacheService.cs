using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonsterQuest.Services
{
    public class IconCacheService : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int maxCacheSize = 50;

        private class CacheEntry
        {
            public string IconKey;
            public Sprite IconSprite;
            public AsyncOperationHandle<Sprite> Handle;
            public float LastAccessTime;
        }

        private Dictionary<string, CacheEntry> cache = new Dictionary<string, CacheEntry>();
        private Dictionary<string, Task<Sprite>> ongoingLoads = new Dictionary<string, Task<Sprite>>();

        private static IconCacheService _instance;
        public static IconCacheService Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("IconCacheService");
                    _instance = go.AddComponent<IconCacheService>();
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

        public async Task<Sprite> GetIconAsync(string iconKey, System.Threading.CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(iconKey)) return null;

            // 1. Hit en caché
            if (cache.TryGetValue(iconKey, out CacheEntry entry))
            {
                entry.LastAccessTime = Time.realtimeSinceStartup;
                return entry.IconSprite;
            }

            // 2. ¿Ya se está cargando? (Evita cargas duplicadas simultáneas)
            if (ongoingLoads.TryGetValue(iconKey, out Task<Sprite> loadTask))
            {
                Sprite s = await loadTask;
                if (cancellationToken.IsCancellationRequested) return null;
                return s;
            }

            // 3. Iniciar nueva carga
            var tcs = new TaskCompletionSource<Sprite>();
            ongoingLoads[iconKey] = tcs.Task;

            try
            {
                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(iconKey);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Sprite loadedSprite = handle.Result;
                    
                    // Añadir a caché y limpiar si es necesario
                    EnforceLRUPolicy();
                    
                    cache[iconKey] = new CacheEntry
                    {
                        IconKey = iconKey,
                        IconSprite = loadedSprite,
                        Handle = handle,
                        LastAccessTime = Time.realtimeSinceStartup
                    };

                    tcs.SetResult(loadedSprite);
                    ongoingLoads.Remove(iconKey);
                    
                    if (cancellationToken.IsCancellationRequested) return null;
                    return loadedSprite;
                }
                else
                {
                    Debug.LogWarning($"Fallo al cargar icono: {iconKey}");
                    Addressables.Release(handle);
                    tcs.SetResult(null);
                    ongoingLoads.Remove(iconKey);
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Excepción al cargar icono {iconKey}: {ex.Message}");
                tcs.SetResult(null);
                ongoingLoads.Remove(iconKey);
                return null;
            }
        }

        private void EnforceLRUPolicy()
        {
            if (cache.Count < maxCacheSize) return;

            string oldestKey = null;
            float oldestTime = float.MaxValue;

            foreach (var kvp in cache)
            {
                if (kvp.Value.LastAccessTime < oldestTime)
                {
                    oldestTime = kvp.Value.LastAccessTime;
                    oldestKey = kvp.Key;
                }
            }

            if (oldestKey != null)
            {
                // Liberar RAM
                Addressables.Release(cache[oldestKey].Handle);
                cache.Remove(oldestKey);
            }
        }

        public void ClearCache()
        {
            foreach (var kvp in cache)
            {
                if (kvp.Value.Handle.IsValid())
                {
                    Addressables.Release(kvp.Value.Handle);
                }
            }
            cache.Clear();
        }

        private void OnDestroy()
        {
            ClearCache();
        }
    }
}
