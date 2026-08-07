using UnityEngine;

namespace MonsterQuest.Combat
{
    public enum WeaponOrientation
    {
        Horizontal, // Espadas, dagas (corte horizontal)
        Vertical,   // Hachas, martillos (golpe de arriba a abajo)
        Forward     // Lanzas, varitas (estocada o disparo al frente)
    }

    /// <summary>
    /// Se adjunta al Prefab 3D de cada arma para corregir su posición y rotación base.
    /// </summary>
    public class WeaponVisualConfig : MonoBehaviour
    {
        [Header("Transformación en Combate (Órbita)")]
        [Tooltip("Ajuste para centrar el agarre del arma.")]
        public Vector3 localPositionOffset = Vector3.zero;
        
        [Tooltip("Rotación para asegurar que el arma apunte al frente correctamente.")]
        public Vector3 localRotationOffset = Vector3.zero;
        
        [Tooltip("Escala estándar (por si el modelo original es muy grande o pequeño).")]
        public Vector3 localScale = Vector3.one;
        
        [Header("Comportamiento de Ataque")]
        public WeaponOrientation attackOrientation = WeaponOrientation.Horizontal;

        [Header("Transformación en UI (Preview 3D)")]
        [Tooltip("Posición específica para centrar el arma en el recuadro del UI.")]
        public Vector3 previewPosition = Vector3.zero;
        
        [Tooltip("Escala específica para que se vea bien en el inventario o incubadora.")]
        public Vector3 previewScale = Vector3.one;
        
        [Tooltip("Rotación favorable para lucir el arma en la UI.")]
        public Vector3 previewRotation = new Vector3(0, 45, 0);
    }
}
