using UnityEngine;

public class FloatingWeapon : MonoBehaviour
{
    public Transform player;
    public Vector3 idleOffset = new Vector3(0, 1.5f, -1.5f);
    public float followSpeed = 5f;
    
    private bool isAttacking = false;
    private Transform attackTarget;
    private TrailRenderer trail;
    
    void Start()
    {
        trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null) trail.emitting = true; // ALWAYS ON
    }

    void Update()
    {
        if (player == null) return;

        if (!isAttacking)
        {
            // Hover behind player
            Vector3 targetPos = player.position + player.rotation * idleOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
            
            // Look in the same direction as player
            transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, followSpeed * Time.deltaTime);
        }
    }

    public void Attack(Transform target)
    {
        if (!isAttacking && target != null)
        {
            attackTarget = target;
            StartCoroutine(AggressiveAttackRoutine());
        }
    }

    public void StopAttack()
    {
        isAttacking = false;
        attackTarget = null;
        StopAllCoroutines();
    }

    System.Collections.IEnumerator AggressiveAttackRoutine()
    {
        isAttacking = true;
        
        while (attackTarget != null && isAttacking)
        {
            // Pick a random side of the enemy
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(0.5f, 2f), Random.Range(-2f, 2f)).normalized * 2f;
            Vector3 startPos = attackTarget.position + randomOffset;
            Vector3 endPos = attackTarget.position - randomOffset; // Opposite side

            // Instantly warp to start position (or fly very fast)
            transform.position = startPos;
            transform.LookAt(attackTarget.position + Vector3.up);

            // Slash through!
            float slashDuration = 0.1f;
            float elapsed = 0f;
            while (elapsed < slashDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / slashDuration);
                transform.Rotate(Vector3.forward, 3000f * Time.deltaTime); // Spin violently
                yield return null;
            }

            // Small pause before next slash
            yield return new WaitForSeconds(0.4f);
        }

        isAttacking = false;
    }
}
