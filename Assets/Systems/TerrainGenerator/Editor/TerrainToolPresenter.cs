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

    /// <summary>Message the view displays below the buttons.</summary>
    public string StatusMessage { get; private set; } = "";

    /// <summary>True when generation is allowed. Pure query - the view
    /// calls this every repaint, so it must not touch any state.</summary>
    public bool CanGenerate(TerrainConfig config)
    {
        return config != null;
    }

    /// <summary>Rebuilds the terrain: runs the pipeline once per chunk
    /// and parents the chunk objects under one terrain root.</summary>
    public void Generate(TerrainConfig config)
    {
        if (!CanGenerate(config))
        {
            StatusMessage = "Assign a TerrainConfig asset before generating.";
            return;
        }
        float chunkSizeInMeters = config.ChunkSizeInMeters;

        // Full rebuild instead of reuse: a changed chunk count would
        // otherwise leave stale chunk children behind.
        GameObject terrain = GameObject.Find(TerrainObjectName);
        if (terrain != null)
        {
            Object.DestroyImmediate(terrain);
        }
        terrain = new GameObject(TerrainObjectName);

        // Resolved once - all chunks share one material. Built-in defaults
        // render magenta under URP, so the fallback asks the active render
        // pipeline for its default material instead.
        Material material = config.TerrainMaterial != null
            ? config.TerrainMaterial
            : GraphicsSettings.currentRenderPipeline.defaultMaterial;
        for (int chunkZ = 0; chunkZ < config.ChunksPerEdge; chunkZ++)
        {
            for (int chunkX = 0; chunkX < config.ChunksPerEdge; chunkX++)
            {
                float[,] heightmap = HeightmapGenerator.Generate(config, chunkX, chunkZ);

                PlateauModifier.Apply(heightmap, config, chunkX, chunkZ);

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
    }

    /// <summary>Removes the generated terrain from the scene.</summary>
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


}