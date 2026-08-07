using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Unity.Netcode;
using MonsterQuest.Combat;

[RequireComponent(typeof(WeaponCompanionManager))]
public class PlayerCombat : NetworkBehaviour
{
    public float globalDetectionRange = 10.0f;
    
    private Transform currentTarget;
    private Transform queuedTarget;
    private TargetVisuals targetVisuals;
    private NavMeshAgent agent;

    private UnityEngine.Rendering.Universal.DecalProjector rangeProjector;
    
    private WeaponCompanionManager companionManager;

    void Start()
    {
        targetVisuals = GetComponent<TargetVisuals>();
        agent = GetComponent<NavMeshAgent>();
        companionManager = GetComponent<WeaponCompanionManager>();

        Transform rangeCircle = transform.Find("PlayerRangeCircle");
        if (rangeCircle != null)
        {
            rangeProjector = rangeCircle.GetComponent<UnityEngine.Rendering.Universal.DecalProjector>();
        }
        
        // Escondemos la base
        Transform sword = transform.Find("Sword");
        if (sword != null)
        {
            sword.gameObject.SetActive(false); 
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleTargetSelection();
        UpdateProximity();
        HandleAutoAttack();

        if (rangeProjector != null)
        {
            float alpha = (Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f) * 0.4f + 0.2f;
            rangeProjector.fadeFactor = alpha;
        }
    }

    void UpdateProximity()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
        foreach (Enemy e in allEnemies)
        {
            if (e == null) continue;
            
            float distance = Vector3.Distance(transform.position, e.transform.position);
            e.SetInRange(distance <= globalDetectionRange);
        }
    }

    void HandleTargetSelection()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
                
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Enemy clickedEnemy = hit.collider.GetComponent<Enemy>();
                if (clickedEnemy != null && !clickedEnemy.IsDead())
                {
                    Transform hitT = hit.collider.transform;

                    if (currentTarget == null)
                    {
                        currentTarget = hitT;
                        clickedEnemy.isTargeted = true;
                        if (targetVisuals != null) targetVisuals.SelectPrimaryTarget(currentTarget);
                    }
                    else if (currentTarget == hitT)
                    {
                        clickedEnemy.isTargeted = false;
                        currentTarget = null;
                        if (targetVisuals != null) targetVisuals.ClearPrimaryTarget();
                        
                        PromoteQueue();
                    }
                    else if (queuedTarget == hitT)
                    {
                        clickedEnemy.isTargeted = false;
                        queuedTarget = null;
                        if (targetVisuals != null) targetVisuals.ClearQueuedTarget();
                    }
                    else
                    {
                        if (queuedTarget != null)
                        {
                            Enemy prevQ = queuedTarget.GetComponent<Enemy>();
                            if (prevQ != null) prevQ.isTargeted = false;
                        }
                        queuedTarget = hitT;
                        clickedEnemy.isTargeted = true; 
                        if (targetVisuals != null) targetVisuals.SelectQueuedTarget(queuedTarget);
                    }
                }
            }
        }
    }

    void PromoteQueue()
    {
        if (queuedTarget != null)
        {
            currentTarget = queuedTarget;
            queuedTarget = null;
            if (targetVisuals != null)
            {
                targetVisuals.ClearQueuedTarget();
                targetVisuals.SelectPrimaryTarget(currentTarget);
            }
        }
        else
        {
            currentTarget = null;
            if (targetVisuals != null) targetVisuals.ClearAll();
        }
    }

    void HandleAutoAttack()
    {
        // 1. Check if targets died
        if (currentTarget != null)
        {
            Enemy e = currentTarget.GetComponent<Enemy>();
            
            if (e == null || e.IsDead())
            {
                if (e != null) e.isTargeted = false;
                PromoteQueue();
            }
        }

        if (queuedTarget != null)
        {
            Enemy e = queuedTarget.GetComponent<Enemy>();
            
            if (e == null || e.IsDead())
            {
                if (e != null) e.isTargeted = false;
                queuedTarget = null;
                if (targetVisuals != null) targetVisuals.ClearQueuedTarget();
            }
        }

        // 2. Propagate Target to Companions
        if (companionManager != null)
        {
            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.position);
                if (distance <= globalDetectionRange)
                {
                    // Rotar jugador hacia el enemigo si hay target
                    Vector3 direction = (currentTarget.position - transform.position).normalized;
                    direction.y = 0;
                    if (direction.magnitude > 0.1f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
                    }

                    companionManager.SetTarget(currentTarget);
                }
                else
                {
                    // Estamos muy lejos, las armas regresan pero mantenemos el objetivo seleccionado
                    companionManager.ClearTarget();
                }
            }
            else
            {
                companionManager.ClearTarget();
            }
        }
    }
}
