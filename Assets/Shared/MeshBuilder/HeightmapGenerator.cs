/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : HeightmapGenerator.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Static utility that generates a square heightmap (float[,] with values
* 0-1, index convention [x, z]) from layered Perlin noise. Pure
* data-in/data-out: no scene objects, no Unity state. Stage 2 of the
* terrain pipeline - MeshBuilder turns the result into a mesh.
*
* History :
* 18.07.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// Generates heightmaps from octaved Perlin noise. Deterministic:
/// the same parameters and seed always produce the same terrain.
/// </summary>
public static class HeightmapGenerator
{
    /// <summary>
    /// Generates a square heightmap from layered Perlin noise.
    /// </summary>
    /// <param name="resolution">Vertices per edge of the square heightmap.</param>
    /// <param name="noiseScale">Zoom into the noise field; larger = wider hills. Must not be 0.</param>
    /// <param name="octaves">Number of noise layers stacked per point.</param>
    /// <param name="persistence">Amplitude falloff per octave (0-1); lower = smoother terrain.</param>
    /// <param name="lacunarity">Frequency growth per octave; 2 = each layer twice as fine.</param>
    /// <param name="seed">Selects the terrain; same seed = same heightmap.</param>
    /// <returns>Square float[,] with values 0-1, indexed [x, z].</returns>
    public static float[,] Generate(int resolution, float noiseScale, int octaves, float persistence, float lacunarity, int seed)
    {
        float[,] heightmap = new float[resolution, resolution];

        // The noise field is infinite and always the same - the seed only
        // decides where we look. Each octave gets its own offset so fine
        // detail does not sit exactly on top of the coarse shape.
        System.Random random = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaveOffsets.Length; i++)
        {
            octaveOffsets[i] = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
        }
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Every pixel restarts at the loudest, coarsest layer -
                // declared inside the loop so nothing carries over.
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;
                float totalAmplitude = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    // Dividing by noiseScale zooms into the noise field;
                    // sampling at whole numbers would return a constant.
                    float sampleX = x / noiseScale * frequency + octaveOffsets[o].x;
                    float sampleZ = z / noiseScale * frequency + octaveOffsets[o].y;
                    noiseHeight += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
                    totalAmplitude += amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                // Normalize by the amplitude sum so more octaves add
                // detail, not height - the result stays in 0-1.
                heightmap[x, z] = noiseHeight / totalAmplitude;
            }
        }
        return heightmap;
    }
}
