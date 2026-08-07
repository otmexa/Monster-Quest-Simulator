using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TargetVisuals : MonoBehaviour
{
    private GameObject primaryCircle;
    private GameObject queuedCircle;

    [SerializeField] private Material primaryDecalMaterial;
    [SerializeField] private Material queuedDecalMaterial;

    private GameObject CreateCircle(Transform parent, Material decalMat, float size)
    {
        if (decalMat == null) return null;

        GameObject circleObj = new GameObject("TargetCircle");
        circleObj.transform.SetParent(parent);
        
        Vector3 pos = parent.position;
        pos.y = 0.1f; // Project from very close to the ground
        circleObj.transform.position = pos;
        circleObj.transform.rotation = Quaternion.Euler(90, 0, 0); // Point downwards

        DecalProjector dp = circleObj.AddComponent<DecalProjector>();
        dp.size = new Vector3(size, size, 0.5f); // Shallow depth to not paint floating items
        dp.material = decalMat;
        
        return circleObj;
    }

    public void SelectPrimaryTarget(Transform target)
    {
        ClearPrimaryTarget();
        if (target != null)
        {
            primaryCircle = CreateCircle(target, primaryDecalMaterial, 3f);
        }
    }

    public void SelectQueuedTarget(Transform target)
    {
        ClearQueuedTarget();
        if (target != null)
        {
            queuedCircle = CreateCircle(target, queuedDecalMaterial, 2.5f);
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
