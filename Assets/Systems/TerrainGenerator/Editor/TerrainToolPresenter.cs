/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : TerrainToolPresenter.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Presenter of the terrain editor tool (MVP pattern): validates the
* config, runs the terrain pipeline in edit mode and owns the generated
* terrain object in the scene. Editor-only - lives in the Editor folder
* and is stripped from builds.
*
* History :
* 18.07.2026 ER Created
* 18.07.2026 ER Terrain material now comes from the config (pipeline default as fallback)
* 19.07.2026 ER Plateau modifier hooked in between generator and mesh build
* 19.07.2026 ER Chunk loop: full rebuild, one child object per chunk
* 20.07.2026 ER Optional water plane added under the terrain root when enabled
* 23.07.2026 ER Plateau now folded into HeightmapGenerator.SampleHeight; separate modifier call removed
******************************************************************************/

using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Presenter of the terrain tool (MVP): validates input, runs the
/// pipeline and owns the generated terrain object in the scene.
/// Knows nothing about how the window draws itself.
/// </summary>
public class TerrainToolPresenter
{
    private const string TerrainObjectName = "Generated Terrain";
    private const string WaterObjectName = "Generated Water Plane";
    // Unity's built-in Plane primitive spans 10x10 units at scale 1.
    private const float PlanePrimitiveSizeInMeters = 10f;

    /// <summary>
    /// Message the view displays below the buttons.
    /// </summary>
    public string StatusMessage { get; private set; } = "";

    /// <summary>
    /// True when generation is allowed. Pure query - the view calls this
    /// every repaint, so it must not touch any state.
    /// </summary>
    /// <param name="config">Terrain settings asset to validate.</param>
    /// <returns>True when a config is assigned.</returns>
    public bool CanGenerate(TerrainConfig config)
    {
        return config != null;
    }

    /// <summary>
    /// Rebuilds the terrain: runs the pipeline once per chunk and parents
    /// the chunk objects under one terrain root.
    /// </summary>
    /// <param name="config">Terrain settings asset driving the pipeline.</param>
    public void Generate(TerrainConfig config)
    {
        if (!CanGenerate(config))
        {
            StatusMessage = "Assign a TerrainConfig asset before generating.";
            return;
        }
        float chunkSizeInMeters = config.ChunkSizeInMeters;

        // Full rebuild so a changed chunk count leaves no stale children.
        GameObject terrain = GameObject.Find(TerrainObjectName);
        if (terrain != null)
        {
            Object.DestroyImmediate(terrain);
        }
        terrain = new GameObject(TerrainObjectName);

        // The built-in default renders magenta under URP; fall back to the pipeline default.
        Material material = config.TerrainMaterial != null
            ? config.TerrainMaterial
            : GraphicsSettings.currentRenderPipeline.defaultMaterial;
        for (int chunkZ = 0; chunkZ < config.ChunksPerEdge; chunkZ++)
        {
            for (int chunkX = 0; chunkX < config.ChunksPerEdge; chunkX++)
            {
                float[,] heightmap = HeightmapGenerator.Generate(config, chunkX, chunkZ);
                Mesh mesh = MeshBuilder.Build(heightmap, chunkSizeInMeters, config.HeightMultiplier);
                GameObject chunk = new GameObject($"Chunk_{chunkX}_{chunkZ}");
                chunk.transform.SetParent(terrain.transform);
                chunk.transform.localPosition = new Vector3(chunkX * chunkSizeInMeters, 0, chunkZ * chunkSizeInMeters);
                // shared* variants - the non-shared ones clone in edit mode.
                chunk.AddComponent<MeshFilter>().sharedMesh = mesh;
                chunk.AddComponent<MeshRenderer>().sharedMaterial = material;

            }
        }
        StatusMessage = $"Terrain generated with seed {config.Seed}.";

        if (config.IsWaterEnabled)
        {
            BuildWaterPlane(config, terrain);
        }
    }

    /// <summary>
    /// Removes the generated terrain from the scene.
    /// </summary>
    public void Clear()
    {
        GameObject terrain = GameObject.Find(TerrainObjectName);
        if (terrain == null)
        {
            StatusMessage = "Nothing to clear - no generated terrain in the scene.";
            return;
        }
        // Edit mode forbids Destroy - DestroyImmediate is the editor variant.
        Object.DestroyImmediate(terrain);
        StatusMessage = "Generated terrain removed.";
    }


    /// <summary>
    /// Spawns the water plane as a child of the terrain root: a single
    /// primitive scaled to the map, centered, and raised to the water level
    /// in meters. Parented so Clear removes it with the terrain.
    /// </summary>
    /// <param name="config">Terrain settings; supplies water level, size and material.</param>
    /// <param name="terrain">Terrain root the plane is parented under.</param>
    private void BuildWaterPlane(TerrainConfig config, GameObject terrain)
    {
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = WaterObjectName;
        water.transform.SetParent(terrain.transform);

        float waterY = config.WaterLevel * config.HeightMultiplier;
        float mapCenter = config.SizeInMeters / 2;
        water.transform.localPosition = new Vector3(mapCenter, waterY, mapCenter);

        float planeScale = config.SizeInMeters / PlanePrimitiveSizeInMeters;
        water.transform.localScale = new Vector3(planeScale, 1f, planeScale);

        Material waterMaterial = config.WaterMaterial != null
            ? config.WaterMaterial
            : GraphicsSettings.currentRenderPipeline.defaultMaterial;
        water.GetComponent<MeshRenderer>().sharedMaterial = waterMaterial;
        Object.DestroyImmediate(water.GetComponent<MeshCollider>());
    }

}