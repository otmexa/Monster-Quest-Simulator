using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public float attackRange = 8.0f;
    public float attackSpeed = 2.0f;
    public GameObject swordPlaceholder;
    
    private Transform currentTarget;
    private Transform queuedTarget;
    private TargetVisuals targetVisuals;
    private NavMeshAgent agent;

    private float lastAttackTime = 0f;
    private Material rangeMaterial;

    void Start()
    {
        targetVisuals = GetComponent<TargetVisuals>();
        agent = GetComponent<NavMeshAgent>();

        Transform rangeCircle = transform.Find("PlayerRangeCircle");
        if (rangeCircle != null)
        {
            UnityEngine.Rendering.Universal.DecalProjector dp = rangeCircle.GetComponent<UnityEngine.Rendering.Universal.DecalProjector>();
            if (dp != null)
            {
                rangeMaterial = dp.material;
            }
        }
    }

    void Update()
    {
        HandleTargetSelection();
        UpdateProximity();
        HandleAutoAttack();

        if (rangeMaterial != null)
        {
            float alpha = (Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f) * 0.15f + 0.05f;
            Color c = rangeMaterial.color;
            c.a = alpha;
            rangeMaterial.color = c;
        }
    }

    void UpdateProximity()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
        foreach (Enemy e in allEnemies)
        {
            if (e == null) continue;
            
            float distance = Vector3.Distance(transform.position, e.transform.position);
            e.SetInRange(distance <= attackRange + 0.5f);
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
                        // No target yet, make it primary
                        currentTarget = hitT;
                        clickedEnemy.isTargeted = true;
                        if (targetVisuals != null) targetVisuals.SelectPrimaryTarget(currentTarget);
                    }
                    else if (currentTarget == hitT)
                    {
                        // Clicked primary again -> Cancel primary and promote queued
                        clickedEnemy.isTargeted = false;
                        currentTarget = null;
                        if (targetVisuals != null) targetVisuals.ClearPrimaryTarget();
                        StopWeaponOrbit();
                        
                        PromoteQueue();
                    }
                    else if (queuedTarget == hitT)
                    {
                        // Clicked queued again -> Cancel queued
                        clickedEnemy.isTargeted = false;
                        queuedTarget = null;
                        if (targetVisuals != null) targetVisuals.ClearQueuedTarget();
                    }
                    else
                    {
                        // Clicked a new target -> Queue it
                        if (queuedTarget != null)
                        {
                            Enemy prevQ = queuedTarget.GetComponent<Enemy>();
                            if (prevQ != null) prevQ.isTargeted = false;
                        }
                        queuedTarget = hitT;
                        clickedEnemy.isTargeted = true; // Use targeted state for queue as well so hover icon disappears
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
            StopWeaponOrbit();
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

        if (currentTarget == null)
        {
            StopWeaponOrbit();
            return;
        }

        // 2. Perform attack if in range
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange + 0.5f)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            direction.y = 0;
            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
            }

            if (swordPlaceholder != null)
            {
                FloatingWeapon fw = swordPlaceholder.GetComponent<FloatingWeapon>();
                if (fw != null) fw.Orbit(currentTarget);
            }

            if (Time.time >= lastAttackTime + attackSpeed)
            {
                Attack();
                
                Enemy e = currentTarget.GetComponent<Enemy>();
                if (e != null)
                {
                    e.TakeDamage(5); 
                }

                lastAttackTime = Time.time;
            }
        }
        else
        {
            StopWeaponOrbit();
            // Out of range -> lose primary target. Promote queue? Optional. Let's just lose it.
            Enemy e = currentTarget.GetComponent<Enemy>();
            if (e != null) e.isTargeted = false;
            
            PromoteQueue(); // Let's promote queue if we lose primary due to range
        }
    }

    void Attack()
    {
        if (swordPlaceholder != null)
        {
            FloatingWeapon fw = swordPlaceholder.GetComponent<FloatingWeapon>();
            if (fw != null) fw.PerformStrike();
        }
    }

    void StopWeaponOrbit()
    {
        if (swordPlaceholder != null)
        {
            FloatingWeapon fw = swordPlaceholder.GetComponent<FloatingWeapon>();
            if (fw != null) fw.StopAttack();
        }
    }
}
