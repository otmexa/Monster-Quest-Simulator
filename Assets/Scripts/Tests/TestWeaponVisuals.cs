using UnityEngine;
using MonsterQuest.Services;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.InputSystem;

namespace MonsterQuest.Tests
{
    public class TestWeaponVisuals : MonoBehaviour
    {
        private GameObject spawnedWeapon;

        void Start()
        {
            // Simular carga de catálogo
            var source = new MonsterQuest.Data.LocalWeaponCatalogSource();
            _ = MonsterQuest.Data.WeaponCatalogService.LoadCatalogAsync(source, 1);
        }

        async void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    Debug.Log("[TestWeaponVisuals] ¡Apretaste ESPACIO! Equipando Espada...");
                    if (spawnedWeapon != null)
                    {
                        WeaponAssetService.Instance.ReleaseWeaponInstance("weapon.visual.1", spawnedWeapon);
                        spawnedWeapon = null;
                    }
                    
                    spawnedWeapon = await WeaponAssetService.Instance.InstantiateWeaponAsync("weapon.visual.1", transform);
                }
                
                if (Keyboard.current.rKey.wasPressedThisFrame)
                {
                    Debug.Log("[TestWeaponVisuals] ¡Apretaste R! Desequipando y limpiando RAM...");
                    if (spawnedWeapon != null)
                    {
                        WeaponAssetService.Instance.ReleaseWeaponInstance("weapon.visual.1", spawnedWeapon);
                        spawnedWeapon = null;
                    }
                }
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tools/3. Iniciar Prueba Fase 3 (Addressables Visual)")]
        public static void RunTest()
        {
            // 1. Crear nueva escena o usar la actual
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);

            // 2. Crear cubo y cámara
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "JugadorDePrueba";
            cube.transform.position = Vector3.zero;

            Camera.main.transform.position = new Vector3(0, 2, -5);
            Camera.main.transform.LookAt(cube.transform);

            // 3. Añadir servicio de test
            cube.AddComponent<TestWeaponVisuals>();
            
            // Instanciar WeaponAssetService implícitamente
            _ = WeaponAssetService.Instance;

            // 4. Iniciar Play Mode
            EditorApplication.EnterPlaymode();
        }
#endif
    }
}
