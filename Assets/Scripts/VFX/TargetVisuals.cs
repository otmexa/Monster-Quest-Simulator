using UnityEngine;

public class TargetVisuals : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject selectionCirclePrefab; // Spawned under enemies
    
    private GameObject currentSelectionCircle;

    public void SelectTarget(Transform target)
    {
        ClearTarget();
        if (target != null && selectionCirclePrefab != null)
        {
            currentSelectionCircle = Instantiate(selectionCirclePrefab, target);
            // Snap to floor globally
            Vector3 pos = target.position;
            pos.y = 0.05f;
            currentSelectionCircle.transform.position = pos;
            currentSelectionCircle.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    public void ClearTarget()
    {
        if (currentSelectionCircle != null)
        {
            Destroy(currentSelectionCircle);
        }
    }
}
