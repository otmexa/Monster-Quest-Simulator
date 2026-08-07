using System;
using UnityEngine;

namespace MonsterQuest.Combat
{
    /// <summary>
    /// Estadísticas calculadas en tiempo de ejecución para un arma compañera.
    /// Incorpora el nivel, los encantamientos y buffs activos.
    /// </summary>
    public struct WeaponRuntimeStats
    {
        public int finalDamage;
        public float attackSpeed; // Attacks per second
        public float attackInterval; // Time between attacks (1f / attackSpeed)
        public float attackRange;
        public float moveSpeed;
        
        // Efectos futuros
        public string[] effectIds;
        public string[] passiveEffectIds;

        public WeaponRuntimeStats(int baseDamage, float baseAttackSpeed = 1f, float baseRange = 3f, float baseMoveSpeed = 15f)
        {
            finalDamage = baseDamage;
            attackSpeed = baseAttackSpeed > 0 ? baseAttackSpeed : 1f;
            attackInterval = 1f / attackSpeed;
            attackRange = baseRange;
            moveSpeed = baseMoveSpeed;
            
            effectIds = Array.Empty<string>();
            passiveEffectIds = Array.Empty<string>();
        }

        public void ApplyLevelAndEnhancement(int level, int enhancementLevel)
        {
            // Escala de daño base: +2 por nivel
            float damage = finalDamage + ((level - 1) * 2f);
            
            // Enhancement: +10% de daño multiplicativo por nivel de enhancement
            damage = damage * (1f + (enhancementLevel * 0.1f));

            finalDamage = Mathf.RoundToInt(damage);
        }

        public static WeaponRuntimeStats Calculate(MonsterQuest.Data.WeaponData catalogData, MonsterQuest.Data.WeaponInstanceData instanceData)
        {
            // Inicializar con valores base del catálogo (o defaults si faltan en el json)
            // Se asume attackSpeed de 1 y rango 3 como estándar.
            WeaponRuntimeStats stats = new WeaponRuntimeStats(
                catalogData.damage, 
                baseAttackSpeed: 1f, 
                baseRange: 3f, 
                baseMoveSpeed: 15f
            );
            
            // Aplicar modificadores del WeaponInstanceData
            stats.ApplyLevelAndEnhancement(instanceData.level, instanceData.enhancement);
            
            // Futuro: Aplicar encantamientos o buffs temporales aquí
            
            return stats;
        }
    }
}
