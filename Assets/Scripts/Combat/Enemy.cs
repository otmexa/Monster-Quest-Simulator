using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public GameObject rangeCircle;
    public GameObject hoverCanvas;
    public Image healthBarFill;
    public GameObject damageNumberPrefab;
    
    [HideInInspector]
    public bool isTargeted = false;

    private Material enemyMaterial;
    private Material rangeCircleMaterial;
    private bool isHovered = false;
    private bool isInRange = false;

    private int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            enemyMaterial = rend.material; 
            enemyMaterial.EnableKeyword("_EMISSION");
        }

        if (rangeCircle != null)
        {
            Renderer circleRend = rangeCircle.GetComponent<Renderer>();
            if (circleRend != null)
            {
                rangeCircleMaterial = circleRend.material;
            }
            rangeCircle.SetActive(false);
        }

        if (hoverCanvas != null)
        {
            hoverCanvas.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Range Circle Opacity (Pulse)
        if (rangeCircle != null)
        {
            if (isInRange && !isTargeted && currentHealth > 0)
            {
                rangeCircle.SetActive(true);
                if (rangeCircleMaterial != null)
                {
                    float pulse = (Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f) * 0.3f + 0.05f;
                    Color c = rangeCircleMaterial.color;
                    c.a = pulse;
                    rangeCircleMaterial.color = c;
                }
            }
            else
            {
                rangeCircle.SetActive(false);
            }
        }

        // 2. Glow (Emission)
        if (enemyMaterial != null)
        {
            float targetGlow = (isInRange && currentHealth > 0) ? 0.05f : 0.0f;
            Color emissionColor = Color.white * targetGlow;
            enemyMaterial.SetColor("_EmissionColor", emissionColor);
        }

        // 3. Hover Icon
        if (hoverCanvas != null)
        {
            hoverCanvas.SetActive(isHovered && !isTargeted && currentHealth > 0);
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        
        // Spawn Damage Number
        if (damageNumberPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1.5f + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0, 0.5f), 0);
            GameObject dmgNum = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);
            DamageNumber dn = dmgNum.GetComponent<DamageNumber>();
            if (dn != null) dn.Setup(amount);
        }

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float percent = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = percent;

            if (percent > 0.5f)
                healthBarFill.color = Color.green;
            else if (percent > 0.25f)
                healthBarFill.color = Color.yellow;
            else if (percent > 0.10f)
                healthBarFill.color = Color.red;
            else
                healthBarFill.color = new Color(0.2f, 0f, 0f); // Dark Red
        }
    }

    void Die()
    {
        // Simple death sequence
        if (rangeCircle != null) rangeCircle.SetActive(false);
        if (hoverCanvas != null) hoverCanvas.SetActive(false);
        isTargeted = false;
        
        // Hide model
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (healthBarFill != null && healthBarFill.transform.parent != null)
        {
            healthBarFill.transform.parent.gameObject.SetActive(false);
        }
    }

    public void SetInRange(bool inRange)
    {
        isInRange = inRange;
    }

    void OnMouseEnter()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;
        isHovered = true;
    }

    void OnMouseExit()
    {
        isHovered = false;
    }
}
