using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MonsterQuest.Inventory;
using MonsterQuest.Data;

namespace MonsterQuest.UI
{
    public class EggHatchingUI : MonoBehaviour
    {
        private static EggHatchingUI instance;
        private Canvas canvas;
        private Image background;
        private Text eggTextMockup; 
        private Image glowImage;
        private Text weaponNameText;
        private Text rarityText;
        private Button closeButton;

        private GameObject spawnedWeapon3D;

        public static void Open(MonsterQuest.Inventory.WeaponData resultWeapon, bool isEgg = true)
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EggHatchingUI");
                instance = go.AddComponent<EggHatchingUI>();
                instance.SetupUI();
            }
            instance.gameObject.SetActive(true);
            instance.StartCoroutine(instance.HatchRoutine(resultWeapon, isEgg));
        }

        private void SetupUI()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 10f;
            canvas.sortingOrder = 999;
            
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);
            background = bgObj.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.9f);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(transform, false);
            glowImage = glowObj.AddComponent<Image>();
            // Use a rotating diamond to make it look cooler and not just a flat square
            glowImage.rectTransform.sizeDelta = new Vector2(800, 800);
            glowImage.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            glowImage.color = Color.clear;

            GameObject eggObj = new GameObject("EggMockup");
            eggObj.transform.SetParent(transform, false);
            eggTextMockup = eggObj.AddComponent<Text>();
            eggTextMockup.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            eggTextMockup.fontSize = 150;
            eggTextMockup.alignment = TextAnchor.MiddleCenter;
            eggTextMockup.text = "🥚";
            eggTextMockup.rectTransform.sizeDelta = new Vector2(400, 400);
            
            GameObject nameObj = new GameObject("WeaponNameText");
            nameObj.transform.SetParent(transform, false);
            weaponNameText = nameObj.AddComponent<Text>();
            weaponNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            weaponNameText.fontSize = 60;
            weaponNameText.alignment = TextAnchor.MiddleCenter;
            weaponNameText.rectTransform.anchoredPosition = new Vector2(0, -200);
            weaponNameText.rectTransform.sizeDelta = new Vector2(800, 100);

            GameObject rarityObj = new GameObject("RarityText");
            rarityObj.transform.SetParent(transform, false);
            rarityText = rarityObj.AddComponent<Text>();
            rarityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rarityText.fontSize = 40;
            rarityText.fontStyle = FontStyle.Bold;
            rarityText.alignment = TextAnchor.MiddleCenter;
            rarityText.rectTransform.anchoredPosition = new Vector2(0, -130);
            rarityText.rectTransform.sizeDelta = new Vector2(800, 100);

            GameObject btnObj = new GameObject("CloseButton");
            btnObj.transform.SetParent(transform, false);
            closeButton = btnObj.AddComponent<Button>();
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            // Anchor to bottom center so it's always visible regardless of aspect ratio
            btnRt.anchorMin = new Vector2(0.5f, 0);
            btnRt.anchorMax = new Vector2(0.5f, 0);
            btnRt.pivot = new Vector2(0.5f, 0);
            btnRt.anchoredPosition = new Vector2(0, 50); // 50 pixels up from the absolute bottom
            btnRt.sizeDelta = new Vector2(250, 80);
            
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            Text btnTxt = btnTextObj.AddComponent<Text>();
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontSize = 30;
            btnTxt.color = Color.white;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.text = "CONTINUAR";
            btnTxt.rectTransform.anchorMin = Vector2.zero;
            btnTxt.rectTransform.anchorMax = Vector2.one;
            btnTxt.rectTransform.offsetMin = Vector2.zero;
            btnTxt.rectTransform.offsetMax = Vector2.zero;

            closeButton.onClick.AddListener(() => {
                if (spawnedWeapon3D != null) Destroy(spawnedWeapon3D);
                gameObject.SetActive(false);
            });
        }

        private IEnumerator HatchRoutine(MonsterQuest.Inventory.WeaponData resultWeapon, bool isEgg)
        {
            eggTextMockup.gameObject.SetActive(isEgg);
            glowImage.color = Color.clear;
            weaponNameText.text = "";
            rarityText.text = "";
            closeButton.gameObject.SetActive(false);
            if (spawnedWeapon3D != null) Destroy(spawnedWeapon3D);

            if (isEgg)
            {
                // Phase 1: Shake
                Vector3 originalPos = eggTextMockup.rectTransform.anchoredPosition;
                float shakeTimer = 1.5f;
                while (shakeTimer > 0)
                {
                    shakeTimer -= Time.deltaTime;
                    eggTextMockup.rectTransform.anchoredPosition = originalPos + new Vector3(Random.Range(-15f, 15f), Random.Range(-15f, 15f), 0);
                    yield return null;
                }
                eggTextMockup.rectTransform.anchoredPosition = originalPos;
                eggTextMockup.gameObject.SetActive(false);
            }

            // Phase 2: Reveal
            if (!string.IsNullOrEmpty(resultWeapon.prefabName))
            {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/" + resultWeapon.prefabName);
                if (prefab != null)
                {
                    spawnedWeapon3D = Instantiate(prefab);
                    Camera cam = Camera.main;
                    if (cam != null)
                    {
                        // Ponemos el arma exactamente frente a la cámara, justo antes del canvas (que está a 10f)
                        spawnedWeapon3D.transform.SetParent(cam.transform);
                        spawnedWeapon3D.transform.localPosition = new Vector3(0, 0, 9.5f); 
                        spawnedWeapon3D.transform.localScale = Vector3.one * 2.5f; // Un poco más grande para que luzca
                        spawnedWeapon3D.transform.localRotation = Quaternion.Euler(resultWeapon.customRotationOffset);
                        
                        // Asegurar que el arma se dibuje sobre el fondo pero bajo la UI de texto
                        // Unity usa el SortingLayer/Order de los renderers para mezclarlos con Canvas "ScreenSpaceCamera"
                        Renderer[] renderers = spawnedWeapon3D.GetComponentsInChildren<Renderer>();
                        foreach (Renderer r in renderers)
                        {
                            r.sortingOrder = 1000; 
                        }
                    }
                }
            }

            weaponNameText.text = resultWeapon.displayName;
            rarityText.text = resultWeapon.rarity.ToString().ToUpper();
            closeButton.gameObject.SetActive(true);

            // Phase 3: Spin and Glow
            while (gameObject.activeSelf)
            {
                if (spawnedWeapon3D != null)
                {
                    spawnedWeapon3D.transform.Rotate(Vector3.up * 120f * Time.deltaTime, Space.World);
                }

                Color baseColor = RarityVisuals.GetAnimatedColor(resultWeapon.rarity, Time.time);
                
                Color glowColor = baseColor;
                glowColor.a = 0.3f + Mathf.PingPong(Time.time * 2f, 0.4f);
                glowImage.color = glowColor;

                rarityText.color = baseColor;
                weaponNameText.color = Color.white;

                yield return null;
            }
        }
    }
}
