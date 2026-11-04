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
        
        // Colocar el proyector alto para que pinte todo hacia abajo
        Vector3 pos = parent.position;
        pos.y = 5f; 
        circleObj.transform.position = pos;
        circleObj.transform.rotation = Quaternion.Euler(90, 0, 0); // Apuntar hacia abajo

        DecalProjector dp = circleObj.AddComponent<DecalProjector>();
        // Hacer la caja de proyección gigante (10 de profundidad) para garantizar que toque el piso
        dp.size = new Vector3(size, size, 10f); 
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
