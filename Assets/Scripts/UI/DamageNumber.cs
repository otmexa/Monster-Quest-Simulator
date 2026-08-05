using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float fadeSpeed = 1f;
    public float lifetime = 1.5f;
    
    private Text textComponent;
    private Color textColor;
    private float startTime;

    void Awake()
    {
        textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textColor = textComponent.color;
        }
        startTime = Time.time;
        
        // Pop effect
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        // Float up
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        
        // Pop in scale
        float age = Time.time - startTime;
        if (age < 0.2f)
        {
            float scale = Mathf.Lerp(0, 0.02f, age / 0.2f); // Assuming scale 0.01 is normal for world canvas
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0.01f, 0.01f, 0.01f), 10f * Time.deltaTime);
        }

        // Fade out
        if (age > lifetime - 1f)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            if (textComponent != null) textComponent.color = textColor;
        }

        // Destroy
        if (age >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Setup(int damage)
    {
        if (textComponent != null)
        {
            textComponent.text = "-" + damage.ToString();
        }
    }
}
