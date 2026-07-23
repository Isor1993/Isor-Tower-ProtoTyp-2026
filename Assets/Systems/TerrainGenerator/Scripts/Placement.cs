/*****************************************************************************
* Project : Isor-Tower-ProtoTyp-2026
* File    : Placement.cs
* Date    : 23.07.2026
* Author  : Eric Rosenberg
*
* Description :
* Immutable value type describing one concrete object placement: which
* prefab, where, how turned and how big. Produced in bulk by the object
* placer and read by the presenter to instantiate the object. A struct so
* thousands of them cost no garbage.
*
* History :
* 23.07.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// One concrete object placement produced by the placer. Immutable:
/// set once through the constructor, never changed afterwards.
/// </summary>
public struct Placement
{
    /// <summary>
    /// The prefab this placement spawns.
    /// </summary>
    public GameObject Prefab { get; }

    /// <summary>
    /// World position in meters.
    /// </summary>
    public Vector3 Position { get; }

    /// <summary>
    /// World rotation; ground-aligned when the placeable requests it.
    /// </summary>
    public Quaternion Rotation { get; }

    /// <summary>
    /// Uniform scale factor.
    /// </summary>
    public float Scale { get; }

    /// <summary>
    /// Creates an immutable placement.
    /// </summary>
    /// <param name="prefab">Prefab to spawn.</param>
    /// <param name="position">World position in meters.</param>
    /// <param name="rotation">World rotation.</param>
    /// <param name="scale">Uniform scale factor.</param>
    public Placement(GameObject prefab, Vector3 position, Quaternion rotation, float scale)
    {
        Prefab = prefab;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}
