/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : TerrainConfig.cs
* Date    : 18.07.2026
* Author  : Eric Rosenberg
*
* Description :
* ScriptableObject parameter object for the terrain pipeline: bundles the
* chunk layout, noise and mesh settings that every pipeline stage reads.
* Assets are created via Create > Terrain > Terrain Config and swapped in
* the Inspector as terrain presets. Read-only from code by design.
*
* History :
* 18.07.2026 ER Created
* 18.07.2026 ER Added terrain material (null = render pipeline default)
* 19.07.2026 ER Added plateau settings (center, radius, blend, height)
* 19.07.2026 ER Replaced heightmap resolution with chunk layout (chunks per edge, chunk resolution)
* 20.07.2026 ER Added water settings (enabled, level, shore margin, material) and flood-warning validation
******************************************************************************/

using UnityEngine;

/// <summary>
/// Shared settings asset for the terrain pipeline. All consumers reference
/// the same asset (one source of truth); multiple assets act as swappable
/// presets. Code only reads - values are edited exclusively in the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "TerrainConfig", menuName = "Terrain/Terrain Config")]
public class TerrainConfig : ScriptableObject
{
    [Header("Chunks")]
    [Tooltip("Chunks per edge of the square terrain; total chunk count is this value squared.")]
    [Min(1)]
    [SerializeField] private int _chunksPerEdge = 8;
    [Tooltip("Vertices per edge of a single chunk; adjacent chunks share their border row.")]
    [Range(2, 255)]
    [SerializeField] private int _chunkResolution = 129;

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

    [Header("Plateau")]
    [Tooltip("Plateau center along X, normalized: 0 = left edge, 0.5 = middle, 1 = right edge.")]
    [Range(0f, 1f)]
    [SerializeField] private float _plateauCenterX = 0.5f;
    [Tooltip("Plateau center along Z, normalized: 0 = near edge, 0.5 = middle, 1 = far edge.")]
    [Range(0f, 1f)]
    [SerializeField] private float _plateauCenterZ = 0.5f;
    [Tooltip("Radius of the flat area as a fraction of the terrain size; 0 disables the plateau.")]
    [Min(0f)]
    [SerializeField] private float _plateauRadius = 0f;
    [Tooltip("Width of the ring that blends the plateau into the surrounding terrain; must not be 0.")]
    [Min(0.001f)]
    [SerializeField] private float _plateauBlend = 0.05f;
    [Tooltip("Target height of the flat area in the 0-1 heightmap space.")]
    [Range(0f, 1f)]
    [SerializeField] private float _plateauHeight = 0.5f;

    [Header("Mesh")]
    [Tooltip("Edge length of the terrain in world units.")]
    [Min(0.1f)]
    [SerializeField] private float _sizeInMeters = 2048f;
    [Tooltip("Scales the 0-1 height values to meters.")]
    [Min(0.1f)]
    [SerializeField] private float _heightMultiplier = 50f;
    [Tooltip("Material applied to the generated terrain; leave empty to use the render pipeline's default material.")]
    [SerializeField] private Material _terrainMaterial;

    [Header("Water")]
    [Tooltip("Master switch for the water plane; the level below is kept while disabled so it can be toggled without losing the setting.")]
    [SerializeField] private bool _isWaterEnabled = false;
    [Tooltip("Water surface height in the normalized 0-1 heightmap space (compared after the height curve); scales with the map, so the shoreline stays put when the height multiplier changes.")]
    [Range(0f, 1f)]
    [SerializeField] private float _waterLevel = 0.3f;
    [Tooltip("Bare strip above the waterline where nothing is placed, in 0-1 height units; used by the placement stage, not by the water plane itself.")]
    [Range(0f, 1f)]
    [SerializeField] private float _shoreMargin = 0.05f;
    [Tooltip("Material for the water plane; leave empty to use the render pipeline's default material.")]
    [SerializeField] private Material _waterMaterial;

    [Header("Placement")]
    [Tooltip("Object types the placer scatters across the terrain.")]
    [SerializeField] private Placeable[] _placeables;

    /// <summary>
    /// Remaps the normalized 0-1 heights after octave blending.
    /// </summary>
    public AnimationCurve HeightCurve => _heightCurve;

    /// <summary>
    /// Zoom into the noise field; larger = wider hills.
    /// </summary>
    public float NoiseScale => _noiseScale;

    /// <summary>
    /// Number of noise layers stacked per point.
    /// </summary>
    public int Octaves => _octaves;

    /// <summary>
    /// Amplitude falloff per octave (0-1).
    /// </summary>
    public float Persistence => _persistence;

    /// <summary>
    /// Frequency growth per octave.
    /// </summary>
    public float Lacunarity => _lacunarity;

    /// <summary>
    /// Selects the terrain; same seed = same heightmap.
    /// </summary>
    public int Seed => _seed;

    /// <summary>
    /// Edge length of the terrain in world units.
    /// </summary>
    public float SizeInMeters => _sizeInMeters;

    /// <summary>
    /// Scales the 0-1 height values to meters.
    /// </summary>
    public float HeightMultiplier => _heightMultiplier;

    /// <summary>
    /// Material for the generated terrain; null = pipeline default.
    /// </summary>
    public Material TerrainMaterial => _terrainMaterial;

    /// <summary>
    /// Plateau center along X, normalized 0-1.
    /// </summary>
    public float PlateauCenterX => _plateauCenterX;

    /// <summary>
    /// Plateau center along Z, normalized 0-1.
    /// </summary>
    public float PlateauCenterZ => _plateauCenterZ;

    /// <summary>
    /// Width of the blend ring around the plateau.
    /// </summary>
    public float PlateauBlend => _plateauBlend;

    /// <summary>
    /// Target height of the flat area, 0-1.
    /// </summary>
    public float PlateauHeight => _plateauHeight;

    /// <summary>
    /// Radius of the flat area; 0 = plateau disabled.
    /// </summary>
    public float PlateauRadius => _plateauRadius;

    /// <summary>
    /// Chunks per edge of the square terrain.
    /// </summary>
    public int ChunksPerEdge => _chunksPerEdge;

    /// <summary>
    /// Vertices per edge of a single chunk.
    /// </summary>
    public int ChunkResolution => _chunkResolution;

    /// <summary>
    /// World size of one quad in meters, derived from size and chunk layout.
    /// </summary>
    public float MetersPerQuad => _sizeInMeters / (_chunksPerEdge * (_chunkResolution - 1));

    /// <summary>
    /// Edge length of one chunk in world units.
    /// </summary>
    public float ChunkSizeInMeters => _sizeInMeters / _chunksPerEdge;

    /// <summary>
    /// Master switch for the water plane; keeps the level while disabled.
    /// </summary>
    public bool IsWaterEnabled => _isWaterEnabled;

    /// <summary>
    /// Water surface height, normalized 0-1 (compared after the height curve).
    /// </summary>
    public float WaterLevel => _waterLevel;

    /// <summary>
    /// Bare strip above the waterline (0-1) where the placement stage skips objects.
    /// </summary>
    public float ShoreMargin => _shoreMargin;

    /// <summary>
    /// Material for the water plane; null = pipeline default.
    /// </summary>
    public Material WaterMaterial => _waterMaterial;

    /// <summary>
    /// The object types the placer scatters across the terrain.
    /// </summary>
    public Placeable[] Placeables => _placeables;

    private void OnValidate()
    {
        if (_plateauRadius > 0 && _waterLevel >= _plateauHeight)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Plateau height is at or below the water level - the village would be flooded!", this);
#endif
        }
    }
}
