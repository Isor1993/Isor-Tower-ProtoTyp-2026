/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : MeshBuilder.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Static utility that translates a heightmap into a Unity mesh. Expects a
* heightmap padded by one ring (see HeightmapGenerator): the ring is not
* meshed, it only supplies the neighbour heights so that edge normals match
* the adjacent chunk. Normals are computed analytically from height
* differences (no RecalculateNormals), which keeps chunk borders seam-free.
* Pure data-in/data-out. Stage 3 of the terrain pipeline.
*
* History :
* 18.07.2026 ER Created
* 19.07.2026 ER Analytic normals from a padded heightmap - seamless chunk borders
******************************************************************************/

using UnityEngine;

/// <summary>
/// Builds terrain meshes from heightmap data. The only class in the
/// pipeline that touches the Unity mesh API.
/// </summary>
public static class MeshBuilder
{
    /// <summary>
    /// Creates a grid mesh from a padded heightmap. The one-vertex ring
    /// around the edge feeds the border normals only and is not meshed.
    /// </summary>
    /// <param name="heightmap">Square 0-1 heightmap padded by one ring, indexed [x, z]; the mesh covers the inner grid.</param>
    /// <param name="sizeInMeters">Edge length of the terrain in world units.</param>
    /// <param name="heightMultiplier">Scales the 0-1 height values to meters.</param>
    /// <returns>Mesh with vertices, triangles and analytic normals from height differences.</returns>
    public static Mesh Build(float[,] heightmap, float sizeInMeters, float heightMultiplier)
    {
        // Drop the two ring rows; only the inner grid is meshed.
        int paddedResolution = heightmap.GetLength(0);
        int resolution = paddedResolution - 2;
        Vector3[] normals = new Vector3[resolution * resolution];

        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triangleIndex = 0;
        float spacing = sizeInMeters / (resolution - 1);

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int px = x + 1;
                int pz = z + 1;

                vertices[z * resolution + x] = new Vector3(x * spacing, heightmap[px, pz] * heightMultiplier, z * spacing);

                float heightLeft = heightmap[px - 1, pz] * heightMultiplier;
                float heightRight = heightmap[px + 1, pz] * heightMultiplier;
                float heightDown = heightmap[px, pz - 1] * heightMultiplier;
                float heightUp = heightmap[px, pz + 1] * heightMultiplier;

                // Central-difference normal; shared ring heights keep borders seam-free.
                normals[z * resolution + x] = new Vector3(heightLeft - heightRight, 2f * spacing, heightDown - heightUp).normalized;
            }
        }

        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int i = z * resolution + x;

                // Clockwise from above so the faces point up (backface culling).
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
        // Analytic normals, not RecalculateNormals, to avoid a border seam.
        mesh.normals = normals;
        return mesh;
    }
}
