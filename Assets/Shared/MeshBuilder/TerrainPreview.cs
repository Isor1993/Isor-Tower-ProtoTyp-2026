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
    [Header("Heightmap")]
    [Tooltip("Vertices per edge of the test heightmap (grid is always square).")]
    [Min(2)]
    [SerializeField] private int _heightmapResolution = 65;

    [Tooltip("Which test pattern fills the heightmap. Flat and Ramp are regression checks for MeshBuilder; Noise runs the real HeightmapGenerator.")]
    [SerializeField] private TestMode _testMode;

    [Header("Noise (used by mode Noise only)")]
    [Tooltip("Zoom into the noise field; larger values = wider, calmer hills. Must not be 0.")]
    [Min(0.01f)]
    [SerializeField] private float _noiseScale = 20f;

    [Tooltip("Number of noise layers stacked per point; more = finer detail, same height.")]
    [Range(1,6)]
    [SerializeField] private int _octaves = 4;

    [Tooltip("Amplitude falloff per octave (0-1); lower = smoother terrain.")]
    [Range(0f, 1f)]
    [SerializeField] private float _persistence = 0.5f;

    [Tooltip("Frequency growth per octave; 2 = each layer twice as fine.")]
    [Min(1f)]
    [SerializeField] private float _lacunarity = 2f;

    [Tooltip("Selects the terrain; the same seed always produces the same terrain.")]
    [SerializeField] private int _seed;

    [Header("Mesh (all modes)")]
    [Tooltip("Edge length of the terrain in world units.")]
    [Min(0.1f)]
    [SerializeField] private float _sizeInMeters = 200f;

    [Tooltip("Scales the 0-1 height values to meters.")]
    [Min(0.1f)]
    [SerializeField] private float _heightMultiplier = 50f;

    private float[,] _heightmap;

    private void Awake()
    {
        _heightmap = new float[_heightmapResolution, _heightmapResolution];
    }

    private void Start()
    {
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

        GetComponent<MeshFilter>().mesh = MeshBuilder.Build(_heightmap, _sizeInMeters, _heightMultiplier);
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
        _heightmap = HeightmapGenerator.Generate(_heightmapResolution, _noiseScale, _octaves, _persistence, _lacunarity, _seed);
    }
}
