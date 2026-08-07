using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Threading;
using MonsterQuest.Services;
using MonsterQuest.Data;

namespace MonsterQuest.UI
{
    public class WeaponSlotCell : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        public Image iconImage;
        public GameObject checkmarkObj;
        public Image background;

        public event Action<string> OnWeaponSelected;

        private string currentInstanceId;
        private CancellationTokenSource cts;
        private Color defaultColor = new Color(0.2f, 0.6f, 0.9f); // Default background

        public void Setup(WeaponInstanceData instanceData, WeaponData catalogData, bool isEquipped, Sprite placeholder)
        {
            currentInstanceId = instanceData.instanceId;
            
            if (checkmarkObj != null)
                checkmarkObj.SetActive(isEquipped);

            if (background != null)
            {
                switch(catalogData.rarityId.ToLower())
                {
                    case "uncommon": background.color = Color.green; break;
                    case "rare": background.color = Color.blue; break;
                    case "epic": background.color = new Color(0.5f, 0f, 0.5f); break; 
                    case "legendary": background.color = new Color(1f, 0.5f, 0f); break; 
                    default: background.color = defaultColor; break;
                }
            }

            if (iconImage != null)
            {
                if (placeholder != null)
                {
                    iconImage.sprite = placeholder;
                    iconImage.color = new Color(1f, 1f, 1f, 0.5f);
                }

                LoadIcon(catalogData.iconKey);
            }
        }

        private async void LoadIcon(string iconKey)
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            
            try
            {
                Sprite sprite = await IconCacheService.Instance.GetIconAsync(iconKey, cts.Token);
                
                if (sprite != null && !cts.Token.IsCancellationRequested)
                {
                    iconImage.sprite = sprite;
                    iconImage.color = Color.white;
                }
            }
            catch (OperationCanceledException)
            {
                // Ignorar si fue cancelado
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnWeaponSelected?.Invoke(currentInstanceId);
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }
    }
}
