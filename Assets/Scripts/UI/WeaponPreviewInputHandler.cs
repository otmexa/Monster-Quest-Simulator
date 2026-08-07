using UnityEngine;
using UnityEngine.EventSystems;
using MonsterQuest.Services;

namespace MonsterQuest.UI
{
    public class WeaponPreviewInputHandler : MonoBehaviour, IDragHandler
    {
        [Tooltip("Sensibilidad de rotación (cuánto gira por cada pixel arrastrado).")]
        public float rotationSpeed = 0.5f;

        public void OnDrag(PointerEventData eventData)
        {
            if (WeaponPreviewService.Instance != null)
            {
                // Multiplicamos por la sensibilidad para tener un giro suave
                Vector2 delta = eventData.delta * rotationSpeed;
                WeaponPreviewService.Instance.RotateCurrentModel(delta);
            }
        }
    }
}
