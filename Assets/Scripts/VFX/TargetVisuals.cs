using UnityEngine;

public class TargetVisuals : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject selectionCirclePrefab;
    
    private GameObject primaryCircle;
    private GameObject queuedCircle;

    public void SelectPrimaryTarget(Transform target)
    {
        ClearPrimaryTarget();
        if (target != null && selectionCirclePrefab != null)
        {
            primaryCircle = Instantiate(selectionCirclePrefab, target);
            Vector3 pos = target.position;
            pos.y = 0.05f;
            primaryCircle.transform.position = pos;
            primaryCircle.transform.rotation = Quaternion.identity;
            
            LineRenderer lr = primaryCircle.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.material.color = Color.green;
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
            }
        }
    }

    public void SelectQueuedTarget(Transform target)
    {
        ClearQueuedTarget();
        if (target != null && selectionCirclePrefab != null)
        {
            queuedCircle = Instantiate(selectionCirclePrefab, target);
            Vector3 pos = target.position;
            pos.y = 0.05f;
            queuedCircle.transform.position = pos;
            queuedCircle.transform.rotation = Quaternion.identity;
            
            LineRenderer lr = queuedCircle.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.material.color = new Color(1.0f, 0.5f, 0f); // Orange
                lr.startWidth = 0.08f; // Slightly thinner for secondary
                lr.endWidth = 0.08f;
            }
        }
    }

    public void ClearPrimaryTarget()
    {
        if (primaryCircle != null) Destroy(primaryCircle);
    }

    public void ClearQueuedTarget()
    {
        if (queuedCircle != null) Destroy(queuedCircle);
    }

    public void ClearAll()
    {
        ClearPrimaryTarget();
        ClearQueuedTarget();
    }
}
