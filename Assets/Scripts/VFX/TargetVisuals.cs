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
            currentSelectionCircle = Instantiate(selectionCirclePrefab, target.position, Quaternion.Euler(90, 0, 0), target);
            // Slightly lift it so it doesn't z-fight with the floor
            currentSelectionCircle.transform.localPosition = new Vector3(0, 0.05f, 0);
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
