using UnityEngine;
using System.Collections;

public class FloatingWeapon : MonoBehaviour
{
    public Transform player;
    public Vector3 idleOffset = new Vector3(0, 1.5f, -1.5f);
    public Vector3 idleRotationOffset = new Vector3(0, 0, 90f); // Default to horizontal
    public float followSpeed = 5f;
    
    private TrailRenderer trail;
    private Transform currentTarget;
    
    private enum WeaponState { Idle, Orbit, Strike }
    private WeaponState currentState = WeaponState.Idle;
    private Transform target;
    public float orbitTime = 0f;
    private float orbitTilt = 0f;
    public float orbitDistance = 2f;
    public float orbitSpeed = 5f;
    public int weaponIndex = 0;

    [Header("--- VISUAL DEBUG ---")]
    [Tooltip("Activa esto en PlayMode para sobreescribir la posicion y moverla a mano")]
    public bool useDebugValues = false;
    public Vector3 debugIdleOffset = new Vector3(0.5f, 0f, -0.9f);
    public Vector3 debugRotationOffset = new Vector3(0f, 90f, 90f);

    void Start()
    {
        trail = GetComponentInChildren<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.2f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0f;
            trail.emitting = false;
        }

        // Asegurarse de que no colisionen fisicamente con el jugador ni entre ellas
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

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
            Vector3 currentOffset = useDebugValues ? debugIdleOffset : idleOffset;
            Vector3 currentRotOffset = useDebugValues ? debugRotationOffset : idleRotationOffset;
            
            Vector3 targetPos = player.position + player.rotation * currentOffset;
            
            // Oscilacion vertical
            targetPos.y += Mathf.Sin(Time.time * 2f + orbitTime) * 0.1f;
            targetPos.y = Mathf.Max(targetPos.y, 0.5f); // Prevent clipping into ground

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
            
            Quaternion targetRot = player.rotation * Quaternion.Euler(currentRotOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
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

    void OnDrawGizmos()
    {
        // Dibuja los ejes locales de la espada para que veas hacia donde apunta cada cosa
        Gizmos.color = Color.blue; // Z (Adelante)
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        
        Gizmos.color = Color.red; // X (Derecha)
        Gizmos.DrawRay(transform.position, transform.right * 1.5f);
        
        Gizmos.color = Color.green; // Y (Arriba)
        Gizmos.DrawRay(transform.position, transform.up * 1.5f);
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

    // El menú OnGUI ha sido eliminado porque ya no es necesario.
}
