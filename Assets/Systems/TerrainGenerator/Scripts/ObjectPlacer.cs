/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : ObjectPlacer.cs
* Date    : 23.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Static placement stage, sibling to MeshBuilder: reads the terrain and the
* placeable recipes and returns where objects go - it never touches the
* scene (the presenter instantiates). One global Poisson-disc pass per
* placeable type guarantees the minimum spacing; a cheap-to-expensive rule
* filter (water, height band, slope) decides which candidates survive.
* Pure data-in/data-out, deterministic via the config's placement seed.
*
* History :
* 23.07.2026 ER Created
******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scatters placeable objects across the terrain. For each placeable type it
/// runs one global Poisson-disc pass (its own spacing) and filters the
/// candidates by water, height band and slope. Stateless and deterministic.
/// </summary>
public static class ObjectPlacer
{
    // Bridson's rejection limit: attempts per active point before it is retired.
    private const int MaxSampleAttempts = 30;

    /// <summary>
    /// Produces the placements for every placeable type in the config, in
    /// priority order (array order). Reads the terrain only.
    /// </summary>
    /// <param name="config">Terrain settings asset; caller guards against null.</param>
    /// <returns>All surviving placements across all types.</returns>
    public static List<Placement> Place(TerrainConfig config)
    {
        List<Placement> placements = new List<Placement>();
        if (config.Placeables == null)
        {
            return placements;
        }

        Vector2[] octaveOffsets = HeightmapGenerator.BuildOctaveOffsets(config);

        for (int i = 0; i < config.Placeables.Length; i++)
        {
            Placeable placeable = config.Placeables[i];
            // Own seed per type so trees and grass scatter independently, still deterministic.
            System.Random random = new System.Random(config.PlacementSeed + i);
            PlaceType(config, placeable, octaveOffsets, random, placements);
        }
        return placements;
    }

    /// <summary>
    /// Bridson Poisson-disc sampling over a square region: scatters points that
    /// stay at least one radius apart while still filling the area densely. A
    /// background grid (cell = radius / sqrt 2, so at most one point per cell)
    /// keeps the neighbour test local, which makes it run in linear time.
    /// </summary>
    /// <param name="regionSize">Edge length of the square sample area in meters.</param>
    /// <param name="radius">Minimum distance between two points in meters.</param>
    /// <param name="random">Seeded generator; the same seed yields the same scatter.</param>
    /// <returns>The scattered points, each within [0, regionSize) on both axes.</returns>
    private static List<Vector2> SamplePoissonDisc(float regionSize, float radius, System.Random random)
    {
        float cellSize = radius / Mathf.Sqrt(2f);
        int gridSize = Mathf.CeilToInt(regionSize / cellSize);
        // Cell -> sample index + 1; 0 stays "empty" (the array's default).
        int[,] grid = new int[gridSize, gridSize];

        List<Vector2> samples = new List<Vector2>();
        List<Vector2> active = new List<Vector2>();

        Vector2 first = new Vector2(
            (float)random.NextDouble() * regionSize,
            (float)random.NextDouble() * regionSize);
        samples.Add(first);
        active.Add(first);
        grid[(int)(first.x / cellSize), (int)(first.y / cellSize)] = samples.Count;

        while (active.Count > 0)
        {
            int randomIndex = random.Next(active.Count);
            Vector2 point = active[randomIndex];
            bool found = false;

            for (int attempt = 0; attempt < MaxSampleAttempts; attempt++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float distance = radius + (float)random.NextDouble() * radius;

                Vector2 candidateDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidatePosition = point + candidateDirection * distance;

                if (IsValid(candidatePosition, regionSize, cellSize, radius, grid, samples))
                {
                    samples.Add(candidatePosition);
                    active.Add(candidatePosition);
                    grid[(int)(candidatePosition.x / cellSize), (int)(candidatePosition.y / cellSize)] = samples.Count;

                    found = true;
                    break;
                }
            }

            // No candidate found around this point - it is surrounded, retire it.
            if (!found)
            {
                active.RemoveAt(randomIndex);
            }
        }
        return samples;
    }

    /// <summary>
    /// Tests a Poisson candidate: it must lie inside the region and keep at
    /// least one radius from every existing sample. Only the 5x5 grid block
    /// around the candidate's cell is searched - a conflict can sit at most two
    /// cells away, so that block is enough.
    /// </summary>
    /// <param name="candidatePosition">Candidate point in meters.</param>
    /// <param name="regionSize">Edge length of the sample area in meters.</param>
    /// <param name="cellSize">Grid cell size (radius / sqrt 2).</param>
    /// <param name="radius">Minimum allowed distance to any existing sample.</param>
    /// <param name="grid">Cell to sample index + 1 (0 = empty).</param>
    /// <param name="samples">Points placed so far.</param>
    /// <returns>True if the candidate may be placed.</returns>
    private static bool IsValid(Vector2 candidatePosition, float regionSize, float cellSize, float radius, int[,] grid, List<Vector2> samples)
    {
        if (candidatePosition.x < 0
            || candidatePosition.x >= regionSize
            || candidatePosition.y < 0
            || candidatePosition.y >= regionSize)
        {
            return false;
        }

        int cellX = (int)(candidatePosition.x / cellSize);
        int cellZ = (int)(candidatePosition.y / cellSize);
        // A conflicting sample can sit at most two cells away (cell = radius / sqrt 2).
        int cellOffset = 2;
        int gridSize = grid.GetLength(0);

        int startX = Mathf.Max(cellX - cellOffset, 0);
        int endX = Mathf.Min(cellX + cellOffset, gridSize - 1);
        int startZ = Mathf.Max(cellZ - cellOffset, 0);
        int endZ = Mathf.Min(cellZ + cellOffset, gridSize - 1);

        for (int neighborX = startX; neighborX <= endX; neighborX++)
        {
            for (int neighborZ = startZ; neighborZ <= endZ; neighborZ++)
            {
                int stored = grid[neighborX, neighborZ];
                if (stored == 0)
                {
                    continue;
                }

                Vector2 other = samples[stored - 1];
                if (Vector2.Distance(candidatePosition, other) < radius)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Approximates the ground normal at a world position from four neighbouring
    /// height samples (central difference, like the mesh). Serves both the slope
    /// filter and aligning objects to the ground.
    /// </summary>
    /// <param name="config">Terrain settings asset; caller guards against null.</param>
    /// <param name="octaveOffsets">Per-octave offsets from BuildOctaveOffsets.</param>
    /// <param name="worldX">World X position in meters.</param>
    /// <param name="worldZ">World Z position in meters.</param>
    /// <returns>The unit-length ground normal.</returns>
    private static Vector3 SampleNormal(TerrainConfig config, Vector2[] octaveOffsets, float worldX, float worldZ)
    {
        float distanceToNeighborSamples = config.MetersPerQuad;

        // Heights must be in meters here so they share the horizontal unit and the angle comes out right.
        float HeightAt(float x, float z) => HeightmapGenerator.SampleHeight(config, octaveOffsets, x, z) * config.HeightMultiplier;

        float heightLeft = HeightAt(worldX - distanceToNeighborSamples, worldZ);
        float heightRight = HeightAt(worldX + distanceToNeighborSamples, worldZ);
        float heightDown = HeightAt(worldX, worldZ - distanceToNeighborSamples);
        float heightUp = HeightAt(worldX, worldZ + distanceToNeighborSamples);

        Vector3 normal = new Vector3(heightLeft - heightRight, 2 * distanceToNeighborSamples, heightDown - heightUp).normalized;
        return normal;
    }

    /// <summary>
    /// Scatters one placeable type: a Poisson pass at its spacing, then a
    /// cheap-to-expensive filter (water, height band, slope). Every surviving
    /// point becomes a placement appended to the shared list.
    /// </summary>
    /// <param name="config">Terrain settings asset; caller guards against null.</param>
    /// <param name="placeable">Recipe defining the prefab, spacing and placement rules.</param>
    /// <param name="octaveOffsets">Per-octave offsets shared with the terrain.</param>
    /// <param name="random">Seeded generator for this type's scatter, scale and rotation.</param>
    /// <param name="placements">Target list; this type's placements are added here.</param>
    private static void PlaceType(TerrainConfig config, Placeable placeable, Vector2[] octaveOffsets, System.Random random, List<Placement> placements)
    {
        List<Vector2> points = SamplePoissonDisc(config.SizeInMeters, placeable.MinSpacing, random);

        foreach (Vector2 point in points)
        {
            float height = HeightmapGenerator.SampleHeight(config, octaveOffsets, point.x, point.y);
            if (config.IsWaterEnabled && height < config.WaterLevel + config.ShoreMargin)
            {
                continue;
            }
            if (height < placeable.MinHeight || height > placeable.MaxHeight)
            {
                continue;
            }

            // Slope is the expensive check (four more height samples), so it runs last.
            Vector3 normal = SampleNormal(config, octaveOffsets, point.x, point.y);
            float slope = Vector3.Angle(normal, Vector3.up);
            if (slope > placeable.MaxSlope)
            {
                continue;
            }

            Vector3 position = new Vector3(point.x, height * config.HeightMultiplier, point.y);
            float yaw = (float)random.NextDouble() * 360f;

            Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
            if (placeable.AlignToGround)
            {
                // Tilt onto the ground first, then the yaw spins it around its own axis.
                rotation = Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, yaw, 0f);
            }

            float scale = Mathf.Lerp(placeable.ScaleMin, placeable.ScaleMax, (float)random.NextDouble());
            placements.Add(new Placement(placeable.Prefab, position, rotation, scale));
        }
    }
}
