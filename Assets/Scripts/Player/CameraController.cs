using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distance = 10.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 20.0f;
    public float zoomSpeed = 2.0f;

    public float orbitSpeed = 0.5f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 80f;

    private float currentYaw = 45f; // Horizontal angle
    private float currentPitch = 30f; // Vertical angle

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Handle Orbit (Roblox style: Right click drag OR Left/Right Arrow keys)
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            currentYaw += mouseDelta.x * orbitSpeed;
            currentPitch -= mouseDelta.y * orbitSpeed;
        }
        else if (Keyboard.current != null)
        {
            // Keyboard Arrow keys can also rotate camera (classic MMO)
            if (Keyboard.current.leftArrowKey.isPressed) currentYaw -= 100f * Time.deltaTime;
            if (Keyboard.current.rightArrowKey.isPressed) currentYaw += 100f * Time.deltaTime;
        }
        
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        // Handle Zoom (Scroll wheel)
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // Scroll is usually large (e.g. 120), so we normalize it or multiply by a small number
                distance -= Mathf.Sign(scroll) * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        // Calculate Position and Rotation
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 position = target.position + Vector3.up * 1.5f + rotation * direction; // offset to look at player's head/chest

        transform.position = position;
        transform.rotation = rotation;
    }
}
