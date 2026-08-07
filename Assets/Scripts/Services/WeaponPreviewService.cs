using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using MonsterQuest.Combat;

namespace MonsterQuest.Services
{
    public class WeaponPreviewService : MonoBehaviour
    {
        private static WeaponPreviewService _instance;
        public static WeaponPreviewService Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("WeaponPreviewService");
                    _instance = go.AddComponent<WeaponPreviewService>();
                    DontDestroyOnLoad(go);
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        [Header("Configuración")]
        public string previewLayerName = "WeaponPreview";
        public Vector2Int renderTextureSize = new Vector2Int(512, 512);
        
        private GameObject previewRoot;
        private Camera previewCamera;
        private RenderTexture renderTexture;
        private RawImage targetImage;

        private GameObject currentModel;
        private string currentVisualKey;
        
        private int requestVersion = 0;
        private int previewLayer;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            if (previewRoot != null) return;

            previewLayer = LayerMask.NameToLayer(previewLayerName);
            if (previewLayer == -1)
            {
                Debug.LogWarning($"[WeaponPreviewService] La capa '{previewLayerName}' no existe. Usando capa por defecto. ¡Por favor crea la capa en Unity!");
                previewLayer = 0;
            }

            // Crear el Root aislado
            previewRoot = new GameObject("PreviewRoot");
            previewRoot.transform.SetParent(transform);
            previewRoot.transform.position = new Vector3(0, -1000f, 0); // Lejos del mapa
            previewRoot.layer = previewLayer;

            // Crear Luz para el preview
            GameObject lightObj = new GameObject("PreviewLight");
            lightObj.transform.SetParent(previewRoot.transform);
            lightObj.transform.localPosition = new Vector3(0, 2f, -2f);
            lightObj.layer = previewLayer;
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.intensity = 1.2f;
            dirLight.cullingMask = 1 << previewLayer; // Solo ilumina el preview

            // Crear Cámara
            GameObject camObj = new GameObject("PreviewCamera");
            camObj.transform.SetParent(previewRoot.transform);
            camObj.transform.localPosition = new Vector3(0, 0, -5f);
            camObj.layer = previewLayer;
            
            previewCamera = camObj.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0,0,0,0); // Transparente
            previewCamera.cullingMask = 1 << previewLayer; // Solo ve el preview
            previewCamera.fieldOfView = 30f;
            previewCamera.enabled = false;
        }

        public async void ShowPreview(string visualKey, RawImage outputImage)
        {
            requestVersion++;
            int currentRequest = requestVersion;
            
            if (string.IsNullOrEmpty(visualKey))
            {
                HidePreview();
                return;
            }

            targetImage = outputImage;
            
            // Preparar RenderTexture si no existe o se destruyó
            if (renderTexture == null || !renderTexture.IsCreated())
            {
                renderTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 16, RenderTextureFormat.ARGB32);
                renderTexture.Create();
            }

            previewCamera.targetTexture = renderTexture;
            
            if (targetImage != null)
            {
                targetImage.texture = renderTexture;
                targetImage.gameObject.SetActive(true);
            }

            // Cargar el modelo
            GameObject newModel = await WeaponAssetService.Instance.InstantiateWeaponAsync(visualKey, previewRoot.transform);
            
            // Si hubo una solicitud más reciente, cancelamos esta
            if (currentRequest != requestVersion)
            {
                if (newModel != null)
                {
                    WeaponAssetService.Instance.ReleaseWeaponInstance(visualKey, newModel);
                }
                return;
            }

            // Si llegamos aquí, la carga fue exitosa y es la petición actual
            // 1. Limpiar el modelo viejo
            if (currentModel != null && !string.IsNullOrEmpty(currentVisualKey))
            {
                WeaponAssetService.Instance.ReleaseWeaponInstance(currentVisualKey, currentModel);
            }

            // 2. Asignar el nuevo
            currentModel = newModel;
            currentVisualKey = visualKey;

            if (currentModel != null)
            {
                SetLayerRecursively(currentModel, previewLayer);
                
                // Aplicar configuración visual
                WeaponVisualConfig config = currentModel.GetComponent<WeaponVisualConfig>();
                if (config != null)
                {
                    currentModel.transform.localPosition = config.previewPosition;
                    currentModel.transform.localRotation = Quaternion.Euler(config.previewRotation);
                    currentModel.transform.localScale = config.previewScale;
                }
                else
                {
                    currentModel.transform.localPosition = Vector3.zero;
                    currentModel.transform.localRotation = Quaternion.Euler(0, 45, 0);
                    currentModel.transform.localScale = Vector3.one;
                }
                
                previewCamera.enabled = true;
            }
            else
            {
                HidePreview();
            }
        }

        public void RotateCurrentModel(Vector2 delta)
        {
            if (currentModel != null)
            {
                // Rotar en el eje Y basado en el movimiento X del ratón
                currentModel.transform.Rotate(Vector3.up, -delta.x, Space.World);
                // Rotar en el eje X basado en el movimiento Y del ratón
                currentModel.transform.Rotate(Vector3.right, delta.y, Space.World);
            }
        }

        public void HidePreview()
        {
            requestVersion++; // Invalida cualquier carga pendiente

            if (currentModel != null && !string.IsNullOrEmpty(currentVisualKey))
            {
                WeaponAssetService.Instance.ReleaseWeaponInstance(currentVisualKey, currentModel);
                currentModel = null;
                currentVisualKey = null;
            }

            if (previewCamera != null)
            {
                previewCamera.enabled = false;
                previewCamera.targetTexture = null;
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }

            if (targetImage != null)
            {
                targetImage.texture = null;
                targetImage.gameObject.SetActive(false);
                targetImage = null;
            }
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (child == null) continue;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        private void OnDestroy()
        {
            HidePreview();
            if (previewRoot != null)
            {
                Destroy(previewRoot);
            }
        }
    }
}
