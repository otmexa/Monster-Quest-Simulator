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
        private Vector3 idleRotationOffset = Vector3.zero; // Mejor default para modelos 3D

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
                        idleRotationOffset = Vector3.zero;
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
                Debug.LogError($"[WeaponCompanionAgent] Excepcion en LoadVisual: {ex}");
                if (this != null) visualState = WeaponVisualState.Failed;
            }
        }

        public void SetOrbitParameters(Vector3 offset, float time, float speed)
        {
            idleOffset = offset;
            orbitTime = time;
            orbitSpeed = speed;
        }

        private enum StrikeState
        {
            None,
            Thrust,
            Recoil,
            Wait
        }
        private StrikeState strikeState = StrikeState.None;
        private float strikeTimer = 0f;
        private Vector3 strikeStartPos;
        private bool hasDealtDamage = false;

        // Propiedades de la Orbita Atomica
        private Vector3 atomOrbitAxis;
        private float atomOrbitPhase;
        private float atomOrbitRadius = 4f; // Confirmado: 4 unidades

        public void AssignTarget(Transform target)
        {
            if (sharedTarget == target) return; // Evitar reiniciar el estado si es el mismo target

            sharedTarget = target;
            if (target != null)
            {
                targetEnemy = target.GetComponent<Enemy>();
                currentState = CompanionState.MovingToTarget;
                strikeState = StrikeState.None;
                
                // Generar un eje tridimensional aleatorio para formar el "tomo"
                atomOrbitAxis = Random.onUnitSphere;
                atomOrbitPhase = Random.Range(0f, Mathf.PI * 2f);
            }
            else
            {
                ClearTarget();
            }
        }

        public void ClearTarget()
        {
            sharedTarget = null;
            targetEnemy = null;
            currentState = CompanionState.Returning;
            strikeState = StrikeState.None;
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

        private Vector3 GetAtomOrbitPosition()
        {
            if (sharedTarget == null) return transform.position;
            
            atomOrbitPhase += Time.deltaTime * 2.5f; // Velocidad de giro del atomo
            
            Vector3 perpendicular1 = Vector3.Cross(atomOrbitAxis, Vector3.up);
            if (perpendicular1 == Vector3.zero) perpendicular1 = Vector3.right;
            perpendicular1.Normalize();
            Vector3 perpendicular2 = Vector3.Cross(atomOrbitAxis, perpendicular1).normalized;

            Vector3 offset = (perpendicular1 * Mathf.Cos(atomOrbitPhase) + perpendicular2 * Mathf.Sin(atomOrbitPhase)) * atomOrbitRadius;
            return sharedTarget.position + offset + Vector3.up * 1f; // Elevar ligeramente para apuntar al torso
        }

        // Ease In/Out para movimientos mas organicos
        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        private float EaseInBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t; // Da un efecto de "tomar vuelo" hacia atras antes de lanzarse
        }

        private void UpdateMovementToTarget()
        {
            if (sharedTarget == null || (targetEnemy != null && targetEnemy.IsDead()))
            {
                ClearTarget();
                return;
            }

            Vector3 targetAtomPos = GetAtomOrbitPosition();
            float distanceToEnemy = Vector3.Distance(transform.position, sharedTarget.position);

            if (distanceToEnemy <= atomOrbitRadius * 1.5f) // Entramos en rango de orbita
            {
                currentState = CompanionState.Attacking;
                strikeState = StrikeState.None;
            }
            else
            {
                // Moverse hacia la orbita atomica
                transform.position = Vector3.MoveTowards(transform.position, targetAtomPos, Stats.moveSpeed * Time.deltaTime);
                Vector3 direction = (sharedTarget.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(idleRotationOffset);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
                }
            }
        }

        private void UpdateAttacking()
        {
            // Cancelacion inmediata
            if (sharedTarget == null || (targetEnemy != null && targetEnemy.IsDead()))
            {
                ClearTarget();
                return;
            }

            // Timing
            float totalInterval = Stats.attackInterval;
            float thrustDuration = totalInterval * 0.20f;
            float recoilDuration = totalInterval * 0.40f;
            float waitDuration = totalInterval * 0.40f;

            if (strikeState == StrikeState.None || strikeState == StrikeState.Wait)
            {
                // Orbitando el atomo
                Vector3 targetAtomPos = GetAtomOrbitPosition();
                
                // Lerp suave para perseguir el objetivo sin saltos bruscos
                transform.position = Vector3.Lerp(transform.position, targetAtomPos, 12f * Time.deltaTime);
                
                Vector3 direction = (sharedTarget.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(idleRotationOffset);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
                }

                if (strikeState == StrikeState.Wait)
                {
                    strikeTimer += Time.deltaTime;
                    if (strikeTimer >= waitDuration)
                    {
                        strikeState = StrikeState.Thrust;
                        strikeTimer = 0f;
                        strikeStartPos = transform.position;
                        hasDealtDamage = false;
                    }
                }
                else
                {
                    // Primera vez, empezamos el ataque inmediatamente
                    strikeState = StrikeState.Thrust;
                    strikeTimer = 0f;
                    strikeStartPos = transform.position;
                    hasDealtDamage = false;
                }
            }
            else if (strikeState == StrikeState.Thrust)
            {
                strikeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(strikeTimer / thrustDuration);
                
                // Usamos EaseInBack para que haga un pequeo retroceso natural antes de lanzarse
                float organicT = EaseInBack(t); 
                
                Vector3 targetThrustPos = sharedTarget.position + Vector3.up * 1f; // Centro del enemigo
                transform.position = Vector3.LerpUnclamped(strikeStartPos, targetThrustPos, organicT);

                // Rotacion precisa durante el empuje
                Vector3 direction = (targetThrustPos - strikeStartPos).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(idleRotationOffset);
                }

                if (t >= 1f)
                {
                    if (!hasDealtDamage && targetEnemy != null && !targetEnemy.IsDead())
                    {
                        targetEnemy.TakeDamage(Stats.finalDamage);
                        hasDealtDamage = true;
                    }
                    
                    strikeState = StrikeState.Recoil;
                    strikeTimer = 0f;
                    strikeStartPos = transform.position;
                }
            }
            else if (strikeState == StrikeState.Recoil)
            {
                strikeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(strikeTimer / recoilDuration);
                
                // EaseInOut para un regreso suave a la orbita atomica
                float organicT = EaseInOutQuad(t);
                
                Vector3 targetAtomPos = GetAtomOrbitPosition(); 
                transform.position = Vector3.Lerp(strikeStartPos, targetAtomPos, organicT);

                // Rotar para no perder de vista al enemigo
                Vector3 direction = (sharedTarget.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(idleRotationOffset);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
                }

                if (t >= 1f)
                {
                    strikeState = StrikeState.Wait;
                    strikeTimer = 0f;
                }
            }
        }

        private void UpdateReturning()
        {
            if (transform.parent == null) return;

            Vector3 parentPos = transform.parent.position;
            Quaternion parentRot = transform.parent.rotation;

            Vector3 targetPos = parentPos + parentRot * idleOffset;
            float distance = Vector3.Distance(transform.position, targetPos);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, 25f * Time.deltaTime);
            
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
