/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : HeightmapGenerator.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Static utility that generates one chunk of the terrain heightmap
* (square float[,] with values 0-1, index convention [x, z]) from
* layered Perlin noise, driven by a TerrainConfig asset. Samples by
* world position, so adjacent chunks line up seamlessly. Pure
* data-in/data-out: no scene objects, no Unity state. Stage 2 of the
* terrain pipeline - MeshBuilder turns the result into a mesh.
*
* History :
* 18.07.2026 ER Created
* 18.07.2026 ER Switched to TerrainConfig parameter object, added height curve
* 19.07.2026 ER Chunk-based generation: world-position sampling for seamless borders
******************************************************************************/

using UnityEngine;

/// <summary>
/// Generates heightmaps from octaved Perlin noise. Deterministic:
/// the same parameters and seed always produce the same terrain.
/// </summary>
public static class HeightmapGenerator
{
    /// <summary>
    /// Generates the heightmap chunk at the given chunk coordinates from
    /// layered Perlin noise and reshapes it through the config's height
    /// curve. Samples world positions, so chunk borders match up.
    /// </summary>
    /// <param name="config">Terrain settings asset; callers guard against null.</param>
    /// <param name="chunkX">Chunk column, 0 to chunksPerEdge - 1.</param>
    /// <param name="chunkZ">Chunk row, 0 to chunksPerEdge - 1.</param>
    /// <returns>Square float[,] with values 0-1, indexed [x, z].</returns>
    public static float[,] Generate(TerrainConfig config, int chunkX, int chunkZ)
    {
        float[,] heightmap = new float[config.ChunkResolution, config.ChunkResolution];

        // The noise field is infinite and always the same - the seed only
        // decides where we look. Each octave gets its own offset so fine
        // detail does not sit exactly on top of the coarse shape.
        System.Random random = new System.Random(config.Seed);
        Vector2[] octaveOffsets = new Vector2[config.Octaves];

        for (int i = 0; i < octaveOffsets.Length; i++)
        {
            octaveOffsets[i] = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
        }
        for (int z = 0; z < config.ChunkResolution; z++)
        {
            for (int x = 0; x < config.ChunkResolution; x++)
            {
                // Local index -> global vertex index -> world position.
                // ChunkResolution - 1 because neighbors share their border
                // row: the same world position samples the same height,
                // which is what keeps the chunk borders seamless.
                int globalX = chunkX * (config.ChunkResolution - 1) + x;
                int globalZ = chunkZ * (config.ChunkResolution - 1) + z;
                float worldX = globalX * config.MetersPerQuad;
                float worldZ = globalZ * config.MetersPerQuad;
                // Every pixel restarts at the loudest, coarsest layer -
                // declared inside the loop so nothing carries over.
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;
                float totalAmplitude = 0f;

                for (int o = 0; o < config.Octaves; o++)
                {
                    // Dividing by noiseScale zooms into the noise field;
                    // sampling at whole numbers would return a constant.
                    float sampleX = worldX / config.NoiseScale * frequency + octaveOffsets[o].x;
                    float sampleZ = worldZ / config.NoiseScale * frequency + octaveOffsets[o].y;
                    noiseHeight += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
                    totalAmplitude += amplitude;

                    amplitude *= config.Persistence;
                    frequency *= config.Lacunarity;
                }
                // Normalize by the amplitude sum so more octaves add
                // detail, not height - the result stays in 0-1.
                heightmap[x, z] = noiseHeight / totalAmplitude;
                // The curve reshapes the 0-1 profile (e.g. flatten
                // valleys); meter scaling happens later in MeshBuilder.
                heightmap[x, z] = config.HeightCurve.Evaluate(heightmap[x, z]);
            }
        }
        return heightmap;
    }
}
