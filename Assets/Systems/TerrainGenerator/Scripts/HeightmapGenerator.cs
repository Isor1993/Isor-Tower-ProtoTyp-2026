/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : HeightmapGenerator.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Static utility for terrain heights. SampleHeight answers "what is the
* height at this world position?" (layered Perlin noise -> height curve ->
* plateau) and is shared by the chunk loop here and the object placer.
* Generate walks one chunk's grid, calls SampleHeight per vertex and returns
* a square float[,] (0-1, index [x, z]) padded by one ring so MeshBuilder can
* compute seamless border normals. Pure data-in/data-out, no Unity state.
*
* History :
* 18.07.2026 ER Created
* 18.07.2026 ER Switched to TerrainConfig parameter object, added height curve
* 19.07.2026 ER Chunk-based generation: world-position sampling for seamless borders
* 19.07.2026 ER Clamp the curve result to 0-1 (curve overshoot could dip below the world)
* 19.07.2026 ER Smaller octave offsets so float precision does not terrace fine resolutions
* 19.07.2026 ER One-vertex padding ring feeding MeshBuilder's border normals
* 23.07.2026 ER Extracted SampleHeight (noise + curve + plateau at a point), shared with the object placer
******************************************************************************/

using UnityEngine;

/// <summary>
/// Generates terrain heights from octaved Perlin noise. Deterministic:
/// the same config and seed always produce the same terrain.
/// </summary>
public static class HeightmapGenerator
{
    // Large offsets coarsen Perlin's float precision and terrace fine meshes.
    private const int MaxOctaveOffset = 10000;

    // One border ring on each side, read by MeshBuilder for seamless normals; fixed at 1.
    private const int PaddingRing = 1;

    /// <summary>
    /// Generates the heightmap for one chunk: walks its padded grid and
    /// samples the terrain height at each vertex's world position.
    /// </summary>
    /// <param name="config">Terrain settings asset; callers guard against null.</param>
    /// <param name="chunkX">Chunk column, 0 to chunksPerEdge - 1.</param>
    /// <param name="chunkZ">Chunk row, 0 to chunksPerEdge - 1.</param>
    /// <returns>Square float[,] with values 0-1, indexed [x, z], padded by one ring for the mesh's border normals.</returns>
    public static float[,] Generate(TerrainConfig config, int chunkX, int chunkZ)
    {
        int paddedResolution = config.ChunkResolution + 2 * PaddingRing;
        float[,] heightmap = new float[paddedResolution, paddedResolution];

        Vector2[] octaveOffsets = BuildOctaveOffsets(config);

        for (int z = 0; z < paddedResolution; z++)
        {
            for (int x = 0; x < paddedResolution; x++)
            {
                // (ChunkResolution - 1) lets neighbours share a border row; (- PaddingRing) shifts past the ring.
                int globalVertexX = chunkX * (config.ChunkResolution - 1) + (x - PaddingRing);
                int globalVertexZ = chunkZ * (config.ChunkResolution - 1) + (z - PaddingRing);
                float worldX = globalVertexX * config.MetersPerQuad;
                float worldZ = globalVertexZ * config.MetersPerQuad;

                heightmap[x, z] = SampleHeight(config, octaveOffsets, worldX, worldZ);
            }
        }
        return heightmap;
    }

    /// <summary>
    /// Builds the per-octave noise offsets from the seed. They are the same
    /// across the whole terrain, so callers build them once and pass them
    /// into SampleHeight.
    /// </summary>
    /// <param name="config">Terrain settings asset; supplies seed and octave count.</param>
    /// <returns>One offset per octave.</returns>
    public static Vector2[] BuildOctaveOffsets(TerrainConfig config)
    {
        System.Random random = new System.Random(config.Seed);
        Vector2[] octaveOffsets = new Vector2[config.Octaves];

        for (int i = 0; i < octaveOffsets.Length; i++)
        {
            octaveOffsets[i] = new Vector2(random.Next(-MaxOctaveOffset, MaxOctaveOffset), random.Next(-MaxOctaveOffset, MaxOctaveOffset));
        }
        return octaveOffsets;
    }

    /// <summary>
    /// Samples the terrain height at a world position: layered Perlin noise,
    /// reshaped by the height curve, with the plateau composed on top. Shared
    /// by the chunk loop and the object placer.
    /// </summary>
    /// <param name="config">Terrain settings asset; callers guard against null.</param>
    /// <param name="octaveOffsets">Per-octave offsets from BuildOctaveOffsets.</param>
    /// <param name="worldX">World X position in meters.</param>
    /// <param name="worldZ">World Z position in meters.</param>
    /// <returns>Height in 0-1.</returns>
    public static float SampleHeight(TerrainConfig config, Vector2[] octaveOffsets, float worldX, float worldZ)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        float totalAmplitude = 0f;

        for (int o = 0; o < config.Octaves; o++)
        {
            float sampleX = worldX / config.NoiseScale * frequency + octaveOffsets[o].x;
            float sampleZ = worldZ / config.NoiseScale * frequency + octaveOffsets[o].y;
            noiseHeight += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
            totalAmplitude += amplitude;

            amplitude *= config.Persistence;
            frequency *= config.Lacunarity;
        }

        // Divide by the amplitude sum so octaves add detail, not height.
        float height = noiseHeight / totalAmplitude;
        height = Mathf.Clamp01(config.HeightCurve.Evaluate(height));
        height = PlateauModifier.SampleAt(config, height, worldX, worldZ);
        return height;
    }
}
