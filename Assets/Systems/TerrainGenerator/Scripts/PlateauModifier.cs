/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : PlateauModifier.cs
* Date    : 19.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Computes the village plateau contribution: flattens a circular area to a
* fixed height and blends it into the surrounding terrain. Works per world
* point and returns the resulting height - HeightmapGenerator.SampleHeight
* composes it on top of the noise. Pure data-in/data-out, no state.
*
* History :
* 19.07.2026 ER Created
* 19.07.2026 ER Works per chunk: normalization now spans the whole terrain
* 19.07.2026 ER Padding-aware index mapping; smoothstep blend for a kink-free ring
* 23.07.2026 ER Per-point SampleAt replaces the whole-array Apply stage
******************************************************************************/

using UnityEngine;

/// <summary>
/// Computes the plateau height contribution at a single world point.
/// Stateless - the same inputs always produce the same result.
/// </summary>
public static class PlateauModifier
{
    /// <summary>
    /// Returns the height at one world point after the plateau: full plateau
    /// height inside the radius, blended in the ring around it, unchanged
    /// beyond that. Radius 0 disables the plateau.
    /// </summary>
    /// <param name="config">Terrain settings asset; callers guard against null.</param>
    /// <param name="baseHeight">Terrain height at this point before the plateau (0-1).</param>
    /// <param name="worldX">World X position in meters.</param>
    /// <param name="worldZ">World Z position in meters.</param>
    /// <returns>Height in 0-1 after applying the plateau.</returns>
    public static float SampleAt(TerrainConfig config, float baseHeight, float worldX, float worldZ)
    {
        if (config.PlateauRadius <= 0)
        {
            return baseHeight;
        }

        // Normalize to 0-1 across the terrain so center and radius are size-independent.
        float normalX = worldX / config.SizeInMeters;
        float normalZ = worldZ / config.SizeInMeters;

        float distanceToCenter = Vector2.Distance(
            new Vector2(normalX, normalZ),
            new Vector2(config.PlateauCenterX, config.PlateauCenterZ));

        if (distanceToCenter < config.PlateauRadius)
        {
            return config.PlateauHeight;
        }
        if (distanceToCenter < config.PlateauRadius + config.PlateauBlend)
        {
            // Smoothstep so the ring meets plateau and terrain without a kink.
            float t = (distanceToCenter - config.PlateauRadius) / config.PlateauBlend;
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(config.PlateauHeight, baseHeight, t);
        }
        return baseHeight;
    }
}
