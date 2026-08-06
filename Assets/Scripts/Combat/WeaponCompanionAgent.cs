using UnityEngine;
using MonsterQuest.Data;
using MonsterQuest.Services;

namespace MonsterQuest.Combat
{
    public enum CompanionState
    {
        Orbiting,
        MovingToTarget,
        Attacking,
        Returning
    }

    public class WeaponCompanionAgent : MonoBehaviour
    {
        public string InstanceId { get; private set; }
        public int WeaponId { get; private set; }
        public WeaponRuntimeStats Stats { get; private set; }
        
        private WeaponInstanceData instanceData;
        private WeaponData catalogData;
        
        private CompanionState currentState = CompanionState.Orbiting;
        private Transform sharedTarget;
        private Enemy targetEnemy;
        
        // Visual
        private GameObject visualInstance;
        private Vector3 idleOffset;
        private float orbitTime;
        private float orbitSpeed;
        private Vector3 idleRotationOffset = new Vector3(0f, 90f, 90f);

        // Combat Timers
        private float lastAttackTime = 0f;

        public void Initialize(WeaponInstanceData instData, WeaponData cData)
        {
            InstanceId = instData.instanceId;
            WeaponId = cData.weaponId;
            instanceData = instData;
            catalogData = cData;
            
            RecalculateStats();
            LoadVisual();
        }

        public void RecalculateStats()
        {
            // Base del catálogo + bonificaciones de nivel/encantamientos
            Stats = new WeaponRuntimeStats(catalogData.damage, 1f, 3f);
            Stats.ApplyLevelBonus(instanceData.level);
        }

        private async void LoadVisual()
        {
            visualInstance = await WeaponAssetService.Instance.InstantiateWeaponAsync(catalogData.visualKey, transform);
            if (visualInstance != null)
            {
                visualInstance.transform.localPosition = Vector3.zero;
                visualInstance.transform.localRotation = Quaternion.Euler(idleRotationOffset);
            }
        }

        public void SetOrbitParameters(Vector3 offset, float time, float speed)
        {
            idleOffset = offset;
            orbitTime = time;
            orbitSpeed = speed;
        }

        public void AssignTarget(Transform target)
        {
            sharedTarget = target;
            if (target != null)
            {
                targetEnemy = target.GetComponent<Enemy>();
                currentState = CompanionState.MovingToTarget;
            }
            else
            {
                targetEnemy = null;
                currentState = CompanionState.Returning;
            }
        }

        public void ClearTarget()
        {
            sharedTarget = null;
            targetEnemy = null;
            currentState = CompanionState.Returning;
        }

        void Update()
        {
            switch (currentState)
            {
                case CompanionState.Orbiting:
                    UpdateOrbit();
                    break;
                case CompanionState.MovingToTarget:
                    UpdateMovementToTarget();
                    break;
                case CompanionState.Attacking:
                    UpdateAttacking();
                    break;
                case CompanionState.Returning:
                    UpdateReturning();
                    break;
            }
        }

        private void UpdateOrbit()
        {
            if (transform.parent == null) return;
            
            orbitTime += Time.deltaTime * orbitSpeed;
            float yOffset = Mathf.Sin(orbitTime) * 0.2f;

            Vector3 parentPos = transform.parent.position;
            Quaternion parentRot = transform.parent.rotation;

            Vector3 targetPos = parentPos + parentRot * (idleOffset + new Vector3(0, yOffset, 0));
            transform.position = Vector3.Lerp(transform.position, targetPos, 10f * Time.deltaTime);

            Quaternion targetRot = parentRot * Quaternion.Euler(idleRotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        private void UpdateMovementToTarget()
        {
            if (sharedTarget == null || (targetEnemy != null && targetEnemy.IsDead()))
            {
                ClearTarget();
                return;
            }

            Vector3 direction = (sharedTarget.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, sharedTarget.position);

            if (distance <= Stats.range)
            {
                currentState = CompanionState.Attacking;
            }
            else
            {
                // Moverse hacia el objetivo (independiente del jugador)
                transform.position = Vector3.MoveTowards(transform.position, sharedTarget.position, 15f * Time.deltaTime);
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(idleRotationOffset);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
                }
            }
        }

        private void UpdateAttacking()
        {
            if (sharedTarget == null || (targetEnemy != null && targetEnemy.IsDead()))
            {
                ClearTarget();
                return;
            }

            float distance = Vector3.Distance(transform.position, sharedTarget.position);
            if (distance > Stats.range)
            {
                currentState = CompanionState.MovingToTarget;
                return;
            }

            // Animación / Visual de ataque
            Vector3 direction = (sharedTarget.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(idleRotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
            }

            if (Time.time >= lastAttackTime + Stats.cooldown)
            {
                PerformStrike();
                lastAttackTime = Time.time;
            }
        }

        private void PerformStrike()
        {
            if (targetEnemy != null && !targetEnemy.IsDead())
            {
                // Daño individual usando sus runtime stats
                targetEnemy.TakeDamage(Stats.finalDamage);
                
                // Efecto visual básico de empuje hacia adelante
                transform.position += transform.forward * 1.5f;
            }
        }

        private void UpdateReturning()
        {
            if (transform.parent == null) return;

            Vector3 parentPos = transform.parent.position;
            Quaternion parentRot = transform.parent.rotation;

            Vector3 targetPos = parentPos + parentRot * idleOffset;
            float distance = Vector3.Distance(transform.position, targetPos);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, 20f * Time.deltaTime);
            
            Quaternion targetRot = parentRot * Quaternion.Euler(idleRotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);

            if (distance < 0.1f)
            {
                currentState = CompanionState.Orbiting;
            }
        }

        private void OnDestroy()
        {
            if (visualInstance != null && catalogData != null)
            {
                WeaponAssetService.Instance.ReleaseWeaponInstance(catalogData.visualKey, visualInstance);
            }
        }
    }
}
