using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public GameObject clickParticlePrefab;
    
    private NavMeshAgent agent;
    private CharacterController controller;
    private Camera mainCamera;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        // Ensure NavMeshAgent and CharacterController don't fight
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleKeyboardJoystickMovement();
        HandleClickToMove();
        
        SyncAgentAndController();
    }

    void HandleKeyboardJoystickMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
            // Let CameraController handle A and D for turning, or handle standard strafing here.
            // Wait, if A and D rotate the camera, we only move forward/backward here. 
            // Or if A/D still strafes, we just use them as moveX.
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
        }

        Vector3 inputDir = new Vector3(moveX, 0, moveZ).normalized;

        if (inputDir.magnitude > 0.1f)
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.ResetPath();
            
            // Move relative to camera yaw
            Vector3 camForward = mainCamera.transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            
            Vector3 camRight = mainCamera.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            Vector3 move = camForward * inputDir.z + camRight * inputDir.x;
            
            controller.Move(move * moveSpeed * Time.deltaTime);
            
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    void HandleClickToMove()
    {
        // On Mobile/PC, click on floor to move
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Ignore clicks if they are over UI elements
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<Enemy>() != null)
                {
                    // Target enemy, but don't move towards it
                    if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.ResetPath();
                    return;
                }

                if (agent.isOnNavMesh) agent.SetDestination(hit.point);

                if (clickParticlePrefab != null)
                {
                    Instantiate(clickParticlePrefab, hit.point + Vector3.up * 0.1f, Quaternion.identity);
                }
            }
        }
    }

    void SyncAgentAndController()
    {
        // If the agent is calculating a path (Click to move), follow it
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            Vector3 direction = (agent.steeringTarget - transform.position).normalized;
            controller.Move(direction * agent.speed * Time.deltaTime);
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // Keep agent position synced to our real position so it doesn't rubberband
        agent.nextPosition = transform.position;
    }
}
