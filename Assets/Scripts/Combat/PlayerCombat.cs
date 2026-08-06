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
            
            // Asignamos el índice para que el menú de Debug sepa dónde dibujarse sin encimarse
            fw.weaponIndex = i;
            
            // Desincronizar su ángulo de órbita para que ataquen desde distintos flancos al enemigo
            // orbitTime actúa como el ángulo en radianes en la función de órbita
            fw.orbitTime = (Mathf.PI * 2f / count) * i;
            
            // Rotación fija que encontramos en el Debug (horizontales, filo hacia atrás)
            fw.idleRotationOffset = new Vector3(0f, 90f, 90f);
            
            // Distancia de la espalda encontrada en el Debug
            float zBack = -2.5f; 
            
            if (count == 1)
            {
                // 1 Espada: Centrada en la espalda
                fw.idleOffset = new Vector3(0f, 1.0f, zBack);
            }
            else if (count == 2)
            {
                // 2 Espadas: Una a cada lado
                float x = (i == 0) ? 0.6f : -0.6f;
                fw.idleOffset = new Vector3(x, 1.0f, zBack);
            }
            else if (count == 3)
            {
                // 3 Espadas: El Triángulo Perfecto del Usuario
                if (i == 0) fw.idleOffset = new Vector3(0f, 1.5f, zBack);
                else if (i == 1) fw.idleOffset = new Vector3(0.8f, 0.4f, zBack);
                else fw.idleOffset = new Vector3(-0.8f, 0.4f, zBack);
            }
            else
            {
                // 4 a 15 Espadas: Algoritmo de "Capa de Espadas" (Máximo 3 filas para no tapar al personaje)
                int[] rows;
                switch (count) {
                    case 4: rows = new int[] { 1, 3 }; break;
                    case 5: rows = new int[] { 2, 3 }; break;
                    case 6: rows = new int[] { 3, 3 }; break; // 2 filas anchas
                    case 7: rows = new int[] { 3, 4 }; break;
                    case 8: rows = new int[] { 4, 4 }; break;
                    case 9: rows = new int[] { 2, 3, 4 }; break; // 3 filas compactas
                    case 10: rows = new int[] { 3, 3, 4 }; break;
                    case 11: rows = new int[] { 3, 4, 4 }; break;
                    case 12: rows = new int[] { 3, 4, 5 }; break;
                    case 13: rows = new int[] { 4, 4, 5 }; break;
                    case 14: rows = new int[] { 4, 5, 5 }; break;
                    case 15: rows = new int[] { 4, 5, 6 }; break; // 3 filas súper anchas
                    default: rows = new int[] { count }; break;
                }
                
                // Encontrar en qué fila le toca estar a la espada actual 'i'
                int rowIndex = 0;
                int swordsBefore = 0;
                for (int r = 0; r < rows.Length; r++) {
                    if (i < swordsBefore + rows[r]) {
                        rowIndex = r;
                        break;
                    }
                    swordsBefore += rows[r];
                }
                
                int indexInRow = i - swordsBefore;
                int swordsInRow = rows[rowIndex];
                
                // Ajustes de espaciado
                float dx = 0.9f; // Separación horizontal un poco más amplia
                float dy = 0.6f; // Separación vertical más compacta para que no suban tanto
                
                // Calculamos la altura total para centrar la matriz
                float totalHeight = (rows.Length - 1) * dy;
                float startY = 1.0f + (totalHeight / 2f); // 1.0f es el centro de tu espalda
                
                // Posición XY
                float distanceFromCenter = (indexInRow - (swordsInRow - 1) / 2.0f);
                float x = distanceFromCenter * dx;
                float y = startY - (rowIndex * dy);
                
                // EFECTO CAPA/ALAS (Wing Effect) en Z: 
                // Las espadas del centro están pegadas a la espalda, pero las de las orillas
                // se van haciendo un poco hacia atrás para envolver al personaje como alas.
                float zWingCurve = Mathf.Abs(distanceFromCenter) * 0.4f; 
                float zRowOffset = rowIndex * 0.1f; 
                
                fw.idleOffset = new Vector3(x, y, zBack - zRowOffset - zWingCurve);
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
