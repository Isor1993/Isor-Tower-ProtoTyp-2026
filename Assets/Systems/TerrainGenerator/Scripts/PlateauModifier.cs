/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : PlateauModifier.cs
* Date    : 19.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Static heightmap modifier that flattens a circular area to a fixed
* height (the village plateau) and blends it smoothly into the
* surrounding terrain. Runs between HeightmapGenerator and MeshBuilder;
* writes into the heightmap in place. Pure data-in/data-out.
*
* History :
* 19.07.2026 ER Created
* 19.07.2026 ER Works per chunk: normalization now spans the whole terrain
******************************************************************************/

using UnityEngine;

/// <summary>
/// Flattens a circular plateau into a heightmap. Stateless - the same
/// heightmap and config always produce the same result.
/// </summary>
public static class PlateauModifier
{
    /// <summary>
    /// Flattens the configured plateau area in place: full plateau
    /// height inside the radius, blended in the ring around it,
    /// untouched beyond that.
    /// </summary>
    /// <param name="heightmap">Square 0-1 chunk heightmap, indexed [x, z]; modified in place.</param>
    /// <param name="config">Terrain settings asset; callers guard against null.</param>
    /// <param name="chunkX">Chunk column, 0 to chunksPerEdge - 1.</param>
    /// <param name="chunkZ">Chunk row, 0 to chunksPerEdge - 1.</param>
    public static void Apply(float[,] heightmap, TerrainConfig config, int chunkX, int chunkZ)
    {
        float plateauRadius = config.PlateauRadius;
        float plateauBlend = config.PlateauBlend;

        // Radius 0 is the intended off switch, not an error - no log.
        if (plateauRadius <= 0)
        {
            return;
        }
        int resolution = heightmap.GetLength(0);

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int globalX = chunkX * (config.ChunkResolution - 1) + x;
                int globalZ = chunkZ * (config.ChunkResolution - 1) + z;
                float worldX = globalX * config.MetersPerQuad;
                float worldZ = globalZ * config.MetersPerQuad;
                // World position over world size: 0-1 across the WHOLE
                // terrain, so center and radius mean the same thing in
                // every chunk.
                float normalZ = worldZ / config.SizeInMeters;
                float normalX = worldX / config.SizeInMeters;

                float distanceToCenter = Vector2.Distance(
                    new Vector2(normalX, normalZ),
                    new Vector2(config.PlateauCenterX, config.PlateauCenterZ));

                if (distanceToCenter < plateauRadius)
                {
                    heightmap[x, z] = config.PlateauHeight;
                    continue;
                }

                // Blend ring: t runs 0 (plateau edge) to 1 (outer rim),
                // fading from plateau height back to the original terrain.
                if (distanceToCenter < plateauRadius + plateauBlend)
                {
                    float t = (distanceToCenter - plateauRadius) / plateauBlend;
                    heightmap[x, z] = Mathf.Lerp(config.PlateauHeight, heightmap[x, z], t);
                }
            }
        }
    }
}
