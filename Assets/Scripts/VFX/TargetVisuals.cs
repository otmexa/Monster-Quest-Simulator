using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TargetVisuals : MonoBehaviour
{
    private GameObject primaryCircle;
    private GameObject queuedCircle;

    private GameObject CreateCircle(Transform parent, Material decalMat, float size)
    {
        GameObject circleObj = new GameObject("TargetCircle");
        circleObj.transform.SetParent(parent);
        
        Vector3 pos = parent.position;
        pos.y = 0.5f; // Project from slightly above
        circleObj.transform.position = pos;
        circleObj.transform.rotation = Quaternion.Euler(90, 0, 0); // Point downwards

        DecalProjector dp = circleObj.AddComponent<DecalProjector>();
        dp.size = new Vector3(size, size, 5f);
        dp.material = decalMat;
        
        return circleObj;
    }

    public void SelectPrimaryTarget(Transform target)
    {
        ClearPrimaryTarget();
        if (target != null)
        {
            Material greenMat = Resources.Load<Material>("TargetPrimaryDecal");
            primaryCircle = CreateCircle(target, greenMat, 3f);
        }
    }

    public void SelectQueuedTarget(Transform target)
    {
        ClearQueuedTarget();
        if (target != null)
        {
            Material orangeMat = Resources.Load<Material>("TargetQueuedDecal");
            queuedCircle = CreateCircle(target, orangeMat, 2.5f);
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
