using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public GameObject hoverCanvas;
    public Image healthBarFill;
    public GameObject damageNumberPrefab;
    
    [HideInInspector]
    public bool isTargeted = false;

    private Material enemyMaterial;
    private bool isHovered = false;
    private bool isInRange = false;
    private int maxHealth = 100;
    private int currentHealth;

    public bool IsDead() { return currentHealth <= 0; }

    public Text healthText;
    public GameObject respawnParticlesPrefab;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        currentHealth = maxHealth;
        UpdateHealthBar();

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            enemyMaterial = rend.material; 
            enemyMaterial.EnableKeyword("_EMISSION");
        }

        if (hoverCanvas != null)
        {
            hoverCanvas.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Glow (Emission)
        if (enemyMaterial != null)
        {
            float targetGlow = (isInRange && currentHealth > 0) ? 0.05f : 0.0f;
            Color emissionColor = Color.white * targetGlow;
            enemyMaterial.SetColor("_EmissionColor", emissionColor);
        }

        // 2. Hover Icon
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

        StartCoroutine(HitEffectRoutine());

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
        }
    }

    System.Collections.IEnumerator HitEffectRoutine()
    {
        if (enemyMaterial != null)
        {
            enemyMaterial.SetColor("_EmissionColor", Color.white * 2f);
        }

        Vector3 originalScale = new Vector3(1, 1, 1);
        Vector3 targetScale = new Vector3(1.3f, 0.7f, 1.3f);

        float t = 0;
        while (t < 0.1f)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / 0.1f);
            t += Time.deltaTime;
            yield return null;
        }
        
        t = 0;
        while (t < 0.2f)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t / 0.2f);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
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

        if (healthText != null)
        {
            healthText.text = currentHealth + "/" + maxHealth;
        }
    }

    System.Collections.IEnumerator DieRoutine()
    {
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

        // Wait 7 seconds before magic starts
        yield return new WaitForSeconds(7f);

        // Flash respawn magic 3 times
        for (int i = 0; i < 3; i++)
        {
            if (respawnParticlesPrefab != null)
            {
                Instantiate(respawnParticlesPrefab, startPosition, Quaternion.Euler(-90, 0, 0));
            }
            yield return new WaitForSeconds(1f);
        }

        // Respawn!
        transform.position = startPosition;
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (rend != null) rend.enabled = true;
        if (col != null) col.enabled = true;
        if (healthBarFill != null && healthBarFill.transform.parent != null)
        {
            healthBarFill.transform.parent.gameObject.SetActive(true);
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
