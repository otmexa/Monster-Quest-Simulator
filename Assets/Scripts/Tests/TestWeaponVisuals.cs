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
        private WeaponVisualService visualService;

        void Start()
        {
            visualService = GetComponent<WeaponVisualService>();
            
            // Simular carga de catálogo
            var source = new MonsterQuest.Data.LocalWeaponCatalogSource();
            _ = MonsterQuest.Data.WeaponCatalogService.LoadCatalogAsync(source, 1);
        }

        void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    Debug.Log("[TestWeaponVisuals] ¡Apretaste ESPACIO! Equipando Espada...");
                    visualService.EquipWeapon("weapon.visual.1");
                }
                
                if (Keyboard.current.rKey.wasPressedThisFrame)
                {
                    Debug.Log("[TestWeaponVisuals] ¡Apretaste R! Desequipando y limpiando RAM...");
                    visualService.Unequip();
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

            // 3. Añadir servicios
            var visualSvc = cube.AddComponent<WeaponVisualService>();
            cube.AddComponent<TestWeaponVisuals>();

            // Configurar handBone
            var serializedObject = new SerializedObject(visualSvc);
            var handProp = serializedObject.FindProperty("handBone");
            handProp.objectReferenceValue = cube.transform;
            serializedObject.ApplyModifiedProperties();

            // 4. Iniciar Play Mode automáticamente
            EditorApplication.EnterPlaymode();
            
            Debug.Log("¡Escena de prueba creada y ejecutando Play Mode! APRIETA LA BARRA ESPACIADORA PARA EQUIPAR LA ESPADA.");
        }
#endif
    }
}
