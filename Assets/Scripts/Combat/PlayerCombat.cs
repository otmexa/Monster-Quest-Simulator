using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Netcode;

public class PlayerCombat : NetworkBehaviour
{
    public float attackRange = 8.0f;
    public float attackSpeed = 2.0f;
    
    // Lista de espadas equipadas (hasta 10 slots lógicos)
    public List<FloatingWeapon> equippedWeapons = new List<FloatingWeapon>();
    private Dictionary<string, FloatingWeapon> weaponMap = new Dictionary<string, FloatingWeapon>();
    
    // Referencia a la espada original para clonarla
    private GameObject baseSwordPrefab;
    
    private Transform currentTarget;
    private Transform queuedTarget;
    private TargetVisuals targetVisuals;
    private NavMeshAgent agent;

    private float lastAttackTime = 0f;
    private UnityEngine.Rendering.Universal.DecalProjector rangeProjector;

    void Start()
    {
        targetVisuals = GetComponent<TargetVisuals>();
        agent = GetComponent<NavMeshAgent>();

        Transform rangeCircle = transform.Find("PlayerRangeCircle");
        if (rangeCircle != null)
        {
            rangeProjector = rangeCircle.GetComponent<UnityEngine.Rendering.Universal.DecalProjector>();
        }
        
        // Guardamos el prefab original para clonarlo y lo ocultamos
        Transform sword = transform.Find("Sword");
        if (sword != null)
        {
            baseSwordPrefab = sword.gameObject;
            baseSwordPrefab.SetActive(false); // Escondemos la base
        }
    }

    public void EquipVisualWeapon(string id)
    {
        if (baseSwordPrefab == null) return;

        GameObject newSword = Instantiate(baseSwordPrefab, transform.position, transform.rotation);
        newSword.name = "Sword_" + id;
        newSword.SetActive(true);
        newSword.transform.SetParent(transform);

        FloatingWeapon fw = newSword.GetComponent<FloatingWeapon>();
        if (fw != null)
        {
            fw.player = transform;
            equippedWeapons.Add(fw);
            weaponMap[id] = fw;
        }

        RealignWeapons();
    }

    public void UnequipVisualWeapon(string id)
    {
        if (weaponMap.ContainsKey(id))
        {
            FloatingWeapon fw = weaponMap[id];
            equippedWeapons.Remove(fw);
            weaponMap.Remove(id);
            if (fw != null) Destroy(fw.gameObject);

            RealignWeapons();
        }
    }

    private void RealignWeapons()
    {
        int count = equippedWeapons.Count;
        for (int i = 0; i < count; i++)
        {
            FloatingWeapon fw = equippedWeapons[i];
            
            // Desincronizar su ángulo de órbita para que ataquen desde distintos flancos al enemigo
            // orbitTime actúa como el ángulo en radianes en la función de órbita
            fw.orbitTime = (Mathf.PI * 2f / count) * i;
            
            // Rotación fija para que apunten hacia la izquierda (Z = 90)
            // Y = 90 gira la espada sobre su propio eje (la hoja) para que el filo quede vertical y no plana.
            fw.idleRotationOffset = new Vector3(0f, 90f, 90f);

            // Regresamos a negativo (-0.6f) que es la verdadera espalda.
            float zBack = -0.6f; 
            
            // Apilarlas verticalmente dependiendo de cuántas haya
            if (count == 1)
            {
                fw.idleOffset = new Vector3(0f, 1.2f, zBack);
            }
            else if (count == 2)
            {
                float y = (i == 0) ? 1.4f : 1.0f;
                fw.idleOffset = new Vector3(0f, y, zBack - (i * 0.1f)); 
            }
            else if (count == 3)
            {
                float y = (i == 0) ? 1.6f : (i == 1) ? 1.2f : 0.8f;
                fw.idleOffset = new Vector3(0f, y, zBack - (i * 0.1f));
            }
            else
            {
                // Si equipa más de 3, las apilamos hacia arriba
                float y = 1.2f + (i * 0.3f);
                fw.idleOffset = new Vector3(0f, y, zBack - (i * 0.05f));
            }
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

            foreach (var weapon in equippedWeapons)
            {
                if (weapon != null) weapon.Orbit(currentTarget);
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
        // Simple combo stagger: attack with all weapons
        foreach (var weapon in equippedWeapons)
        {
            if (weapon != null) weapon.PerformStrike();
        }
    }

    void StopWeaponOrbit()
    {
        foreach (var weapon in equippedWeapons)
        {
            if (weapon != null) weapon.StopAttack();
        }
    }
}
