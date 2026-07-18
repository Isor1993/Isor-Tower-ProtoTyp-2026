/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : TerrainPreview.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Scene-side test driver for the terrain pipeline: fills a heightmap
* via the selected test mode (flat, ramp or Perlin noise), runs
* MeshBuilder.Build and assigns the result to the local MeshFilter.
* Not part of the final pipeline - will be replaced by the
* TerrainGenerator orchestrator later.
*
* History :
* 18.07.2026 ER Created
* 18.07.2026 ER Added test mode enum and HeightmapGenerator hookup
* 18.07.2026 ER Replaced parameter fields with a TerrainConfig reference
******************************************************************************/

using UnityEngine;

/// <summary>
/// Selects which test heightmap the preview generates.
/// </summary>
public enum TestMode
{
    Flat,
    Ramp,
    Noise
}

/// <summary>
/// Test MonoBehaviour that feeds MeshBuilder with a heightmap
/// so the mesh generation can be verified in the scene.
/// </summary>
public class TerrainPreview : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Which test pattern fills the heightmap. Flat and Ramp are regression checks for MeshBuilder; Noise runs the real HeightmapGenerator.")]
    [SerializeField] private TestMode _testMode;
    [Tooltip("Terrain settings asset driving all modes; swap assets to switch presets.")]
    [SerializeField] private TerrainConfig _config;

    private float[,] _heightmap;

    private void Awake()
    {
        if (_config == null)
        {
            Debug.LogError("TerrainPreview: no TerrainConfig assigned.", this);
            return;
        }

        _heightmap = new float[_config.HeightmapResolution, _config.HeightmapResolution];
    }

    private void Start()
    {
        if (_config == null) return;

        switch (_testMode)
        {
            case TestMode.Flat:
                FlatTest();
                break;
            case TestMode.Ramp:
                RampTest();
                break;
            case TestMode.Noise:
                NoiseTest();
                break;
        }

        GetComponent<MeshFilter>().mesh = MeshBuilder.Build(_heightmap, _config.SizeInMeters, _config.HeightMultiplier);
    }

    /// <summary>
    /// Fills the heightmap with a linear ramp rising along +X. Verifies
    /// that heights land at the right grid positions and direction.
    /// </summary>
    private void RampTest()
    {
        for (int z = 0; z < _heightmap.GetLength(0); z++)
        {
            for (int x = 0; x < _heightmap.GetLength(1); x++)
            {
                _heightmap[x, z] = x / (_heightmap.GetLength(1) - 1f);
            }
        }
    }

    /// <summary>
    /// Fills the heightmap with zeros. Verifies the plain grid mesh.
    /// </summary>
    private void FlatTest()
    {
        for (int z = 0; z < _heightmap.GetLength(0); z++)
        {
            for (int x = 0; x < _heightmap.GetLength(1); x++)
            {
                _heightmap[x, z] = 0f;
            }
        }
    }

    /// <summary>
    /// Replaces the heightmap with real Perlin terrain from
    /// HeightmapGenerator (the other modes refill the existing array).
    /// </summary>
    private void NoiseTest()
    {
        _heightmap = HeightmapGenerator.Generate(_config);
    }
}
