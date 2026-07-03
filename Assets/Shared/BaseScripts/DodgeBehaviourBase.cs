/*****************************************************************************
* Project : Isors Tower Prototype
* File    : DodgeBehaviourBase.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Defines the abstract base class for dodge behaviours.
* Provides a shared interface for checking whether an entity should dodge,
* whether it is currently dodging, and for starting a dodge while returning
* the previous movement target.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// Abstract base class for dodge behaviour components.
/// Defines the required dodge state information and dodge start logic.
/// </summary>
public abstract class DodgeBehaviourBase : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the entity currently detects a situation that requires dodging.
    /// </summary>
    public abstract bool ShouldDodge { get; }

    /// <summary>
    /// Indicates whether the entity is currently performing a dodge movement.
    /// </summary>
    public abstract bool IsDodging { get; }

    /// <summary>
    /// Tries to start a dodge movement and returns the movement target that was active before dodging.
    /// </summary>
    /// <param name="previousTarget">The movement target that should be resumed after dodging.</param>
    /// <returns>True if the dodge movement was started successfully; otherwise false.</returns>
    public abstract bool TryStartDodge(out Vector3 previousTarget);
}