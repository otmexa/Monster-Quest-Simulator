using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM

public class PlayerCombat : MonoBehaviour
{
    public float attackRange = 2.0f;
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
        HandleAutoAttack();
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
                if (hit.collider.GetComponent<Enemy>() != null)
                {
                    currentTarget = hit.collider.transform;
                    if (targetVisuals != null)
                        targetVisuals.SelectTarget(currentTarget);
                    
                    Debug.Log("Target Selected: " + currentTarget.name);
                }
                else
                {
                    // Clicked on ground or elsewhere
                    currentTarget = null;
                    if (targetVisuals != null)
                        targetVisuals.ClearTarget();
                }
            }
        }
    }

    void HandleAutoAttack()
    {
        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);
        
        if (distance <= attackRange)
        {
            // We are close enough to attack
            // Stop moving if NavMesh was driving us there
            agent.ResetPath();

            // Rotate towards enemy
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    void Attack()
    {
        Debug.Log("Attacking " + currentTarget.name);
        // We will trigger a simple swing animation on the sword here
        if (swordPlaceholder != null)
        {
            Animation anim = swordPlaceholder.GetComponent<Animation>();
            if (anim != null)
            {
                anim.Play();
            }
            else
            {
                // Fallback procedural swing
                StartCoroutine(ProceduralSwing());
            }
        }
    }

    System.Collections.IEnumerator ProceduralSwing()
    {
        TrailRenderer trail = swordPlaceholder.GetComponentInChildren<TrailRenderer>();
        if (trail != null) trail.emitting = true;

        float duration = 0.2f;
        float elapsed = 0f;
        Quaternion startRot = swordPlaceholder.transform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(90, 0, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            swordPlaceholder.transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            yield return null;
        }

        swordPlaceholder.transform.localRotation = startRot; // reset
        if (trail != null) trail.emitting = false;
    }
}
