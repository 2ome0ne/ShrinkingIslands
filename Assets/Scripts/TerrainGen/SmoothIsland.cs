using Unity.Netcode;
using UnityEngine;

public class SmoothIsland : NetworkBehaviour {
    Mesh mesh;
    Vector3[] vertices;
    public float scale = 5f;
    public float power = 2f;

    public void generateMesh() {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices; // Get all "corners" of the grid

        for (int i = 0; i < vertices.Length; i++) {
            // Convert vertex local position to World position for noise
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            
            // Sample noise based on X and Z
            float y = Mathf.PerlinNoise(worldPos.x / scale, worldPos.z / scale);
            
            // Apply the height to the vertex
            vertices[i].y = y * power;
        }

        // Update the mesh with new positions
        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // This makes the lighting look smooth!
        mesh.RecalculateBounds();
    }
}