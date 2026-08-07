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

    public enum WeaponVisualState
    {
        Loading,
        Ready,
        Failed
    }

    public class WeaponCompanionAgent : MonoBehaviour
    {
        public string InstanceId { get; private set; }
        public int WeaponId { get; private set; }
        public WeaponRuntimeStats Stats { get; private set; }
        
        private CompanionState currentState = CompanionState.Orbiting;
        private WeaponVisualState visualState = WeaponVisualState.Loading;

        private WeaponInstanceData instanceData;
        private WeaponData catalogData;
        
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
            
            Stats = WeaponRuntimeStats.Calculate(catalogData, instanceData);
            
            LoadVisual();
        }

        public void UpdateRuntimeStats(WeaponRuntimeStats newStats)
        {
            Stats = newStats;
        }

        private async void LoadVisual()
        {
            visualState = WeaponVisualState.Loading;
            
            try
            {
                visualInstance = await WeaponAssetService.Instance.InstantiateWeaponAsync(catalogData.visualKey, transform);
                
                if (this == null) // Si el objeto fue destruido durante la carga
                {
                    if (visualInstance != null) WeaponAssetService.Instance.ReleaseWeaponInstance(catalogData.visualKey, visualInstance);
                    return;
                }

                if (visualInstance != null)
                {
                    visualState = WeaponVisualState.Ready;
                    WeaponVisualConfig config = visualInstance.GetComponent<WeaponVisualConfig>();
                    
                    if (config != null)
                    {
                        visualInstance.transform.localPosition = config.localPositionOffset;
                        idleRotationOffset = config.localRotationOffset;
                        visualInstance.transform.localRotation = Quaternion.Euler(idleRotationOffset);
                        visualInstance.transform.localScale = config.localScale;
                    }
                    else
                    {
                        visualInstance.transform.localPosition = Vector3.zero;
                        idleRotationOffset = new Vector3(0f, 90f, 90f);
                        visualInstance.transform.localRotation = Quaternion.Euler(idleRotationOffset);
                        visualInstance.transform.localScale = Vector3.one;
                    }
                }
                else
                {
                    visualState = WeaponVisualState.Failed;
                    Debug.LogWarning($"[WeaponCompanionAgent] Falla visual para {catalogData.visualKey}, pero el arma seguir activa.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[WeaponCompanionAgent] Excepcin en LoadVisual: {ex}");
                if (this != null) visualState = WeaponVisualState.Failed;
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

        private bool isFirstOrbitFrame = true;

        private void UpdateOrbit()
        {
            if (transform.parent == null) return;
            
            orbitTime += Time.deltaTime * orbitSpeed;
            float yOffset = Mathf.Sin(orbitTime) * 0.2f;

            Vector3 parentPos = transform.parent.position;
            Quaternion parentRot = transform.parent.rotation;

            Vector3 targetPos = parentPos + parentRot * (idleOffset + new Vector3(0, yOffset, 0));
            Quaternion targetRotQuat = parentRot * Quaternion.Euler(idleRotationOffset);

            if (isFirstOrbitFrame)
            {
                transform.position = targetPos;
                transform.rotation = targetRotQuat;
                isFirstOrbitFrame = false;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 10f * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotQuat, 10f * Time.deltaTime);
            }
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
