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
        // The heightmap carries a one-vertex ring for the border normals;
        // the mesh covers only the inner grid, so subtract the two ring rows
        // to get the real vertex count per edge.
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
                // Inner vertex (x,z) sits at padded index (px,pz): the +1
                // skips the ring, so even edge vertices have all four
                // neighbours available in the heightmap.
                int px = x + 1;
                int pz = z + 1;

                vertices[z * resolution + x] = new Vector3(x * spacing, heightmap[px, pz] * heightMultiplier, z * spacing);

                // Neighbour heights in meters: left, right, down, up.
                float heightLeft = heightmap[px - 1, pz] * heightMultiplier;
                float heightRight = heightmap[px + 1, pz] * heightMultiplier;
                float heightDown = heightmap[px, pz - 1] * heightMultiplier;
                float heightUp = heightmap[px, pz + 1] * heightMultiplier;

                // Normal from the slope to those neighbours (central
                // difference): flat ground points up, a slope tilts it
                // downhill. The ring gives both chunks identical neighbour
                // heights at a shared border, so the border normal matches
                // on both sides - no lighting seam.
                normals[z * resolution + x] = new Vector3(heightLeft - heightRight, 2f * spacing, heightDown - heightUp).normalized;
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
        // Our own normals instead of RecalculateNormals: that sees only one
        // chunk's triangles and would leave a lighting seam at the borders.
        mesh.normals = normals;
        return mesh;
    }
}
