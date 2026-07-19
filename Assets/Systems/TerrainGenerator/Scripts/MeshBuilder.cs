/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : MeshBuilder.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Static utility that translates a heightmap (square float[,] with values
* 0-1, index convention [x, z]) into a Unity mesh. Pure data-in/data-out:
* no scene objects, no design decisions - size and shape come entirely
* from the caller. Stage 5 of the terrain pipeline.
*
* History :
* 18.07.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// Builds terrain meshes from heightmap data. The only class in the
/// pipeline that touches the Unity mesh API.
/// </summary>
public static class MeshBuilder
{
    /// <summary>
    /// Creates a grid mesh from a square heightmap.
    /// </summary>
    /// <param name="heightmap">Square grid of height values 0-1, indexed [x, z].</param>
    /// <param name="sizeInMeters">Edge length of the terrain in world units.</param>
    /// <param name="heightMultiplier">Scales the 0-1 height values to meters.</param>
    /// <returns>Mesh with vertices, triangles and recalculated normals.</returns>
    public static Mesh Build(float[,] heightmap, float sizeInMeters, float heightMultiplier)
    {
        int resolution = heightmap.GetLength(0);

        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triangleIndex = 0;
        float spacing = sizeInMeters / (resolution - 1);

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                vertices[z * resolution + x] = new Vector3(x * spacing, heightmap[x, z] * heightMultiplier, z * spacing);
            }
        }

        // Loops run one short of resolution: cells sit between vertices,
        // so a grid of n vertices per edge only has n-1 cells per edge.
        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int i = z * resolution + x;

                // 6 entries per cell (2 triangles x 3 corners), each one an
                // index into the vertex array. Corner order is clockwise
                // seen from above so the faces point up (backface culling).
                triangles[triangleIndex + 0] = i;
                triangles[triangleIndex + 1] = i + resolution;
                triangles[triangleIndex + 2] = i + 1;

                triangles[triangleIndex + 3] = i + 1;
                triangles[triangleIndex + 4] = i + resolution;
                triangles[triangleIndex + 5] = i + resolution + 1;

                triangleIndex += 6;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}
