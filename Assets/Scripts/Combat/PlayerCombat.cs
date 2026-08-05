using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM

public class PlayerCombat : MonoBehaviour
{
    public float attackRange = 8.0f;
    public GameObject swordPlaceholder;
    
    private Transform currentTarget;
    private TargetVisuals targetVisuals;
    private NavMeshAgent agent;

    private float attackCooldown = 1.0f;
    private float lastAttackTime = 0f;

    void Start()
    {
        targetVisuals = GetComponent<TargetVisuals>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        HandleTargetSelection();
        UpdateProximity();
        HandleAutoAttack();
    }

    void UpdateProximity()
    {
        // Find all enemies to update their range status
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
        foreach (Enemy e in allEnemies)
        {
            if (e == null) continue;
            
            float distance = Vector3.Distance(transform.position, e.transform.position);
            e.SetInRange(distance <= attackRange);
        }
    }

    void HandleTargetSelection()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
                
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Enemy clickedEnemy = hit.collider.GetComponent<Enemy>();
                if (clickedEnemy != null)
                {
                    // Clear previous target
                    if (currentTarget != null)
                    {
                        Enemy prevEnemy = currentTarget.GetComponent<Enemy>();
                        if (prevEnemy != null) prevEnemy.isTargeted = false;
                    }
                    
                    currentTarget = hit.collider.transform;
                    clickedEnemy.isTargeted = true;
                    
                    if (targetVisuals != null)
                    {
                        targetVisuals.SelectTarget(currentTarget);
                    }
                }
            }
        }
    }

    void HandleAutoAttack()
    {
        if (currentTarget == null)
        {
            StopWeaponOrbit();
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            // Rotate towards enemy
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            direction.y = 0;
            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
            }

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                
                Enemy e = currentTarget.GetComponent<Enemy>();
                if (e != null)
                {
                    e.TakeDamage(5); // 5 damage per hit
                }

                lastAttackTime = Time.time;
            }
        }
        else
        {
            StopWeaponOrbit();
        }
    }

    void Attack()
    {
        if (swordPlaceholder != null)
        {
            FloatingWeapon fw = swordPlaceholder.GetComponent<FloatingWeapon>();
            if (fw != null) fw.Attack(currentTarget);
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
