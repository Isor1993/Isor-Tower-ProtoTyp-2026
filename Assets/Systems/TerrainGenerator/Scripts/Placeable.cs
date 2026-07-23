/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : Placeable.cs
* Date    : 23.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Serializable recipe for one kind of placeable object (tree, grass, ...):
* its prefab plus the rules the placer uses to scatter it - height band,
* max slope, spacing, random scale and ground alignment. An array of these
* lives in TerrainConfig and is edited in the Inspector.
*
* History :
* 23.07.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// Recipe for one placeable object type: the prefab and the rules that
/// decide where and how the placer scatters it.
/// </summary>
[System.Serializable]
public class Placeable
{
    [Tooltip("Prefab spawned for this object type.")]
    [SerializeField] private GameObject _prefab;
    [Tooltip("Lower bound of the height band (0-1) where this object may spawn.")]
    [Range(0f, 1f)]
    [SerializeField] private float _minHeight;
    [Tooltip("Upper bound of the height band (0-1) where this object may spawn.")]
    [Range(0f, 1f)]
    [SerializeField] private float _maxHeight;
    [Tooltip("Maximum ground slope in degrees this object tolerates.")]
    [Range(0f, 90f)]
    [SerializeField] private float _maxSlope;
    [Tooltip("Minimum distance in meters between two of these objects (Poisson radius).")]
    [Min(0.1f)]
    [SerializeField] private float _minSpacing;
    [Tooltip("Smallest random uniform scale applied when spawning.")]
    [Min(0f)]
    [SerializeField] private float _scaleMin;
    [Tooltip("Largest random uniform scale applied when spawning.")]
    [Min(0f)]
    [SerializeField] private float _scaleMax;
    [Tooltip("Rotate the object to match the ground slope instead of standing upright.")]
    [SerializeField] private bool _alignToGround;

    /// <summary>
    /// Prefab spawned for this object type.
    /// </summary>
    public GameObject Prefab => _prefab;

    /// <summary>
    /// Lower edge of the height band, normalized 0-1.
    /// </summary>
    public float MinHeight => _minHeight;

    /// <summary>
    /// Upper edge of the height band, normalized 0-1.
    /// </summary>
    public float MaxHeight => _maxHeight;

    /// <summary>
    /// Maximum ground slope this object tolerates, in degrees.
    /// </summary>
    public float MaxSlope => _maxSlope;

    /// <summary>
    /// Minimum distance between two instances in meters (Poisson radius).
    /// </summary>
    public float MinSpacing => _minSpacing;

    /// <summary>
    /// Smallest random uniform scale applied when spawning.
    /// </summary>
    public float ScaleMin => _scaleMin;

    /// <summary>
    /// Largest random uniform scale applied when spawning.
    /// </summary>
    public float ScaleMax => _scaleMax;

    /// <summary>
    /// Whether to rotate the object to match the ground slope.
    /// </summary>
    public bool AlignToGround => _alignToGround;
}
