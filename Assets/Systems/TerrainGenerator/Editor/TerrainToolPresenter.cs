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

    /// <summary>Runs the pipeline and assigns the mesh to the terrain object.</summary>
    public void Generate(TerrainConfig config)
    {
        if (!CanGenerate(config))
        {
            StatusMessage = "Assign a TerrainConfig asset before generating.";
            return;
        }

        float[,] heightmap = HeightmapGenerator.Generate(config);
        PlateauModifier.Apply(heightmap, config);
        Mesh mesh = MeshBuilder.Build(heightmap, config.SizeInMeters, config.HeightMultiplier);

        GameObject terrain = FindOrCreateTerrainObject();
        // sharedMesh, not mesh - .mesh would clone the mesh in edit mode.
        terrain.GetComponent<MeshFilter>().sharedMesh = mesh;

        // Applied on every generate (not only on create) so a material change
        // in the config shows up without clearing first. Built-in materials
        // render magenta under URP, so the fallback asks the active render
        // pipeline for its default material instead.
        Material material = config.TerrainMaterial != null
            ? config.TerrainMaterial
            : GraphicsSettings.currentRenderPipeline.defaultMaterial;
        terrain.GetComponent<MeshRenderer>().sharedMaterial = material;

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

    /// <summary>Reuses the existing terrain object or creates a fresh one.</summary>
    private GameObject FindOrCreateTerrainObject()
    {
        GameObject terrain = GameObject.Find(TerrainObjectName);
        if (terrain == null)
        {
            terrain = new GameObject(TerrainObjectName);
            terrain.AddComponent<MeshFilter>();
            terrain.AddComponent<MeshRenderer>();
        }
        return terrain;
    }
}