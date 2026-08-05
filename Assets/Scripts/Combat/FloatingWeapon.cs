using UnityEngine;
using System.Collections;

public class FloatingWeapon : MonoBehaviour
{
    public Transform player;
    public Vector3 idleOffset = new Vector3(0, 1.5f, -1.5f);
    public float followSpeed = 5f;
    
    private TrailRenderer trail;
    private Transform currentTarget;
    
    private enum WeaponState { Idle, Orbit, Strike }
    private WeaponState currentState = WeaponState.Idle;

    private float orbitTime = 0f;
    private float orbitSpeed = 3f; 
    private float orbitDistance = 2.5f;
    private float orbitTilt = 0f;

    void Start()
    {
        trail = GetComponentInChildren<TrailRenderer>();
        if (player == null && transform.parent != null)
        {
            player = transform.parent;
        }
        // Desvincular del jugador para que flote suavemente en el mundo sin temblar
        transform.SetParent(null);
    }

    void Update()
    {
        if (player == null) return;

        if (currentState == WeaponState.Idle)
        {
            
            // Hover behind player
            Vector3 targetPos = player.position + player.rotation * idleOffset;
            targetPos.y = Mathf.Max(targetPos.y, 0.5f); // Prevent clipping into ground
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, followSpeed * Time.deltaTime);
        }
        else if (currentState == WeaponState.Orbit)
        {
            if (trail != null) trail.emitting = true;

            if (currentTarget != null)
            {
                // Trace "Atom" like 3D orbits
                orbitTime += Time.deltaTime * orbitSpeed;
                orbitTilt += Time.deltaTime * 15f; // Slowly tilt the orbital plane
                
                // Base flat circle
                Vector3 flatOffset = new Vector3(Mathf.Cos(orbitTime), 0, Mathf.Sin(orbitTime)) * orbitDistance;
                
                // Tilt the circle
                Quaternion orbitRotation = Quaternion.Euler(orbitTilt, orbitTilt * 0.5f, 0);
                Vector3 offset = orbitRotation * flatOffset;
                
                Vector3 targetPos = currentTarget.position + Vector3.up * 1.2f + offset;
                targetPos.y = Mathf.Max(targetPos.y, 0.5f); // Prevent clipping into ground

                Vector3 moveDir = (targetPos - transform.position).normalized;
                
                transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * 2f * Time.deltaTime);
                
                // Point forward along the orbit path like a dart
                if (moveDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 15f);
                }
            }
            else
            {
                StopAttack();
            }
        }
    }

    public void Orbit(Transform target)
    {
        if (currentState == WeaponState.Strike) return; // Don't interrupt strike
        
        currentTarget = target;
        currentState = WeaponState.Orbit;
    }

    public void PerformStrike()
    {
        if (currentTarget != null && currentState != WeaponState.Strike)
        {
            StartCoroutine(StrikeRoutine());
        }
    }

    public void StopAttack()
    {
        StopAllCoroutines();
        currentState = WeaponState.Idle;
        currentTarget = null;
    }

    IEnumerator StrikeRoutine()
    {
        currentState = WeaponState.Strike;
        if (trail != null) trail.emitting = true;

        if (currentTarget != null)
        {
            Vector3 targetHead = currentTarget.position + Vector3.up * 1.2f; 
            
            // Dash straight through the target's head from current position
            Vector3 startPos = transform.position;
            Vector3 dashDirection = (targetHead - startPos).normalized;
            Vector3 endPos = targetHead + dashDirection * (orbitDistance + 1f); 
            
            // Point the sword straight at the target during the dash
            if (dashDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dashDirection);
            }

            float dashDuration = 0.15f;
            float t = 0;

            while (t < dashDuration)
            {
                if (currentTarget == null) break;
                
                transform.position = Vector3.Lerp(startPos, endPos, t / dashDuration);
                t += Time.deltaTime;
                yield return null;
            }
            
            // Re-adjust orbit time so it smoothly continues orbiting near the end position
            if (currentTarget != null)
            {
                // Simple heuristic to resume orbiting without huge snaps
                orbitTime += Mathf.PI; 
            }
        }

        if (currentTarget != null)
        {
            currentState = WeaponState.Orbit;
        }
        else
        {
            currentState = WeaponState.Idle;
        }
    }
}
