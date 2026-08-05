using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainProjector : MonoBehaviour
{
    public float radius = 8f;
    public Color color = new Color(1f, 1f, 0f, 0.2f); // Faint yellow default
    
    // Whether this should draw a solid filled disc or just an outline ring
    public bool isOutlineOnly = false;
    public float outlineThickness = 0.1f;

    private Mesh mesh;
    private Vector3[] baseVertices;
    private int segments = 40;

    void Start()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        GetComponent<MeshRenderer>().sharedMaterial = mat;
        
        GenerateMesh();
    }

    void Update()
    {
        UpdateVertices();
    }

    void GenerateMesh()
    {
        if (isOutlineOnly)
        {
            // Just a thick ring
            int rings = 2; // inner and outer
            baseVertices = new Vector3[(segments + 1) * rings];
            int[] triangles = new int[segments * 6];
            
            for (int r = 0; r < rings; r++)
            {
                float ringRadius = (r == 0) ? radius - outlineThickness : radius;
                for (int s = 0; s <= segments; s++)
                {
                    float angle = s * Mathf.PI * 2f / segments;
                    baseVertices[r * (segments + 1) + s] = new Vector3(Mathf.Cos(angle) * ringRadius, 0, Mathf.Sin(angle) * ringRadius);
                }
            }
            
            int t = 0;
            for (int s = 0; s < segments; s++)
            {
                triangles[t++] = s;
                triangles[t++] = (segments + 1) + s;
                triangles[t++] = (segments + 1) + s + 1;
                
                triangles[t++] = s;
                triangles[t++] = (segments + 1) + s + 1;
                triangles[t++] = s + 1;
            }
            
            mesh.vertices = baseVertices;
            mesh.triangles = triangles;
        }
        else
        {
            // Filled disc with concentric rings for better terrain conforming
            int rings = 4;
            baseVertices = new Vector3[(segments + 1) * rings + 1];
            int[] triangles = new int[segments * rings * 6];
            
            baseVertices[0] = Vector3.zero; // Center
            
            int v = 1;
            for (int r = 1; r <= rings; r++)
            {
                float ringRadius = radius * ((float)r / rings);
                for (int s = 0; s <= segments; s++)
                {
                    float angle = s * Mathf.PI * 2f / segments;
                    baseVertices[v++] = new Vector3(Mathf.Cos(angle) * ringRadius, 0, Mathf.Sin(angle) * ringRadius);
                }
            }
            
            int t = 0;
            // Inner ring
            for (int s = 0; s < segments; s++)
            {
                triangles[t++] = 0;
                triangles[t++] = s + 1;
                triangles[t++] = s + 2;
            }
            // Outer rings
            for (int r = 1; r < rings; r++)
            {
                int ringStart = 1 + (r - 1) * (segments + 1);
                int nextRingStart = 1 + r * (segments + 1);
                
                for (int s = 0; s < segments; s++)
                {
                    triangles[t++] = ringStart + s;
                    triangles[t++] = nextRingStart + s;
                    triangles[t++] = nextRingStart + s + 1;
                    
                    triangles[t++] = ringStart + s;
                    triangles[t++] = nextRingStart + s + 1;
                    triangles[t++] = ringStart + s + 1;
                }
            }
            
            mesh.vertices = baseVertices;
            mesh.triangles = triangles;
        }
    }

    void UpdateVertices()
    {
        if (baseVertices == null) return;
        
        Vector3[] dynamicVerts = new Vector3[baseVertices.Length];
        
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(baseVertices[i]);
            Ray ray = new Ray(new Vector3(worldPos.x, transform.position.y + 20f, worldPos.z), Vector3.down);
            
            float hitY = transform.position.y;
            RaycastHit[] hits = Physics.RaycastAll(ray, 40f);
            
            // Sort hits by distance so we hit the highest valid surface
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (var hit in hits)
            {
                // Conforms to floor and bumps
                if (hit.collider.name == "Floor" || hit.collider.name == "TerrainBump")
                {
                    hitY = hit.point.y;
                    break;
                }
            }
            
            // 0.05f above the terrain so it doesn't z-fight
            Vector3 targetWorldPos = new Vector3(worldPos.x, hitY + 0.05f, worldPos.z);
            dynamicVerts[i] = transform.InverseTransformPoint(targetWorldPos);
        }
        
        mesh.vertices = dynamicVerts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
