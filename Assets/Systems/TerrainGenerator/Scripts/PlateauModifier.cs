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
    /// <param name="heightmap">Square 0-1 heightmap, indexed [x, z]; modified in place.</param>
    /// <param name="config">Terrain settings asset; callers guard against null.</param>
    public static void Apply(float[,] heightmap, TerrainConfig config)
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
                // Normalize to 0-1 so distances compare against the
                // normalized center/radius regardless of resolution.
                float normalZ = z / (float)(resolution - 1);
                float normalX = x / (float)(resolution - 1);

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
