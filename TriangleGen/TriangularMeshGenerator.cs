using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TriangularMeshGenerator : UdonSharpBehaviour
{
    [Header("Mesh Options")]
    public int subdivisions = 300;
    public bool generateUVs = true;
    public string meshName = "TriangularMesh";

    [Header("Optional Completion Callback")]
    public UdonBehaviour onMeshGenerated; // The UdonBehaviour to call
    public string methodName = "OnMeshGenerated"; // Method to call on completion

    void Start()
    {
        GenerateMesh(subdivisions);

        // Trigger callback if assigned
        if (onMeshGenerated != null && !string.IsNullOrEmpty(methodName))
        {
            onMeshGenerated.SendCustomEvent(methodName);
        }

        // Disable this UdonBehaviour to prevent it from running again
        this.enabled = false;
    }

    void GenerateMesh(int n)
    {
        Mesh mesh = new Mesh();
        mesh.name = meshName; // Assign name
        GetComponent<MeshFilter>().mesh = mesh;

        int vertCount = (n + 1) * (n + 2) / 2;
        int triCount = n * n * 2; // Max triangles in full subdivision
        Vector3[] vertices = new Vector3[vertCount];
        int[] triangles = new int[triCount * 3];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        // Precompute constants
        float sqrt3 = Mathf.Sqrt(3f);

        // Generate vertices with the top point at origin (0,0,0)
        int vIndex = 0;
        for (int i = 0; i <= n; i++)
        {
            float rowY = -sqrt3 * i / (2f * n); // move downward from top
            for (int j = 0; j <= i; j++)
            {
                float rowX = (j - i / 2f) / n; // center row horizontally

                vertices[vIndex] = new Vector3(rowX, 0f, rowY);
                normals[vIndex] = Vector3.up;

                if (generateUVs)
                {
                    uvs[vIndex] = new Vector2((float)j / i, (float)i / n); // simple UV
                }

                vIndex++;
            }
        }

        // Generate triangles
        int tIndex = 0;
        for (int i = 0; i < n; i++)
        {
            int rowStart = i * (i + 1) / 2;
            int nextRowStart = (i + 1) * (i + 2) / 2;

            for (int j = 0; j <= i; j++)
            {
                int v0 = rowStart + j;
                int v1 = nextRowStart + j;
                int v2 = nextRowStart + j + 1;

                // Upwards triangle
                triangles[tIndex++] = v0;
                triangles[tIndex++] = v1;
                triangles[tIndex++] = v2;

                if (j < i)
                {
                    int v3 = rowStart + j + 1;
                    // Downwards triangle
                    triangles[tIndex++] = v0;
                    triangles[tIndex++] = v2;
                    triangles[tIndex++] = v3;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;

        if (generateUVs)
        {
            mesh.uv = uvs;
        }
    }
}