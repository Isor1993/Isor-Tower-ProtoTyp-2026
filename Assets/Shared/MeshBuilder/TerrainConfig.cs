/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : TerrainConfig.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* ScriptableObject parameter object for the terrain pipeline: bundles the
* heightmap, noise and mesh settings that every pipeline stage reads.
* Assets are created via Create > Terrain > Terrain Config and swapped in
* the Inspector as terrain presets. Read-only from code by design.
*
* History :
* 18.07.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// Shared settings asset for the terrain pipeline. All consumers
/// reference the same asset (one source of truth); multiple assets
/// act as swappable presets. Code only reads - values are edited
/// exclusively in the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "TerrainConfig", menuName = "Terrain/Terrain Config")]
public class TerrainConfig : ScriptableObject
{
    [Header("Heightmap")]
    [Tooltip("Vertices per edge of the square heightmap.")]
    [Min(2)]
    [SerializeField] private int _heightmapResolution = 65;

    [Header("Noise")]
    [Tooltip("Remaps the normalized 0-1 heights; bend the curve down to flatten valleys and emphasize peaks. Linear = no change.")]
    [SerializeField] private AnimationCurve _heightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("Zoom into the noise field; larger values = wider, calmer hills. Must not be 0.")]
    [Min(0.01f)]
    [SerializeField] private float _noiseScale = 20f;
    [Tooltip("Number of noise layers stacked per point; more = finer detail, same height.")]
    [Range(1, 6)]
    [SerializeField] private int _octaves = 4;
    [Tooltip("Amplitude falloff per octave (0-1); lower = smoother terrain.")]
    [Range(0f, 1f)]
    [SerializeField] private float _persistence = 0.5f;
    [Tooltip("Frequency growth per octave; 2 = each layer twice as fine.")]
    [Min(1f)]
    [SerializeField] private float _lacunarity = 2f;
    [Tooltip("Selects the terrain; the same seed always produces the same terrain.")]
    [SerializeField] private int _seed;

    [Header("Mesh")]
    [Tooltip("Edge length of the terrain in world units.")]
    [Min(0.1f)]
    [SerializeField] private float _sizeInMeters = 200f;
    [Tooltip("Scales the 0-1 height values to meters.")]
    [Min(0.1f)]
    [SerializeField] private float _heightMultiplier = 50f;

    /// <summary>Vertices per edge of the square heightmap.</summary>
    public int HeightmapResolution => _heightmapResolution;

    /// <summary>Remaps the normalized 0-1 heights after octave blending.</summary>
    public AnimationCurve HeightCurve => _heightCurve;

    /// <summary>Zoom into the noise field; larger = wider hills.</summary>
    public float NoiseScale => _noiseScale;

    /// <summary>Number of noise layers stacked per point.</summary>
    public int Octaves => _octaves;

    /// <summary>Amplitude falloff per octave (0-1).</summary>
    public float Persistence => _persistence;

    /// <summary>Frequency growth per octave.</summary>
    public float Lacunarity => _lacunarity;

    /// <summary>Selects the terrain; same seed = same heightmap.</summary>
    public int Seed => _seed;

    /// <summary>Edge length of the terrain in world units.</summary>
    public float SizeInMeters => _sizeInMeters;

    /// <summary>Scales the 0-1 height values to meters.</summary>
    public float HeightMultiplier => _heightMultiplier;
}
