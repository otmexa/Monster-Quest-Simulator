using System;

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
        public float cooldown;    // Time between attacks
        public float range;
        
        // Efectos futuros
        public bool hasLifesteal;
        public float critChance;
        public float critMultiplier;

        public WeaponRuntimeStats(int baseDamage, float baseAttackSpeed = 1f, float baseRange = 3f)
        {
            finalDamage = baseDamage;
            attackSpeed = baseAttackSpeed;
            cooldown = 1f / baseAttackSpeed;
            range = baseRange;
            
            hasLifesteal = false;
            critChance = 0.05f;
            critMultiplier = 1.5f;
        }

        public void ApplyLevelBonus(int level)
        {
            finalDamage += (level - 1) * 2; // Ejemplo: +2 daño por nivel
        }

        public static WeaponRuntimeStats Calculate(MonsterQuest.Data.WeaponData catalogData, MonsterQuest.Data.WeaponInstanceData instanceData)
        {
            WeaponRuntimeStats stats = new WeaponRuntimeStats(catalogData.damage, 1f, 3f);
            stats.ApplyLevelBonus(instanceData.level);
            // Aquí se pueden aplicar más modificadores de instanceData.enhancement etc.
            return stats;
        }
    }
}
