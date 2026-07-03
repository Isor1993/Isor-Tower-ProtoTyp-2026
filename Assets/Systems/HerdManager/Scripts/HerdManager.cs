/*****************************************************************************
* Project : Isors Tower Prototype
* File    : HerdManager.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Manages a sheep herd by storing all herd members, tracking the commander,
* calculating herd anchor positions, and providing formation, patrol, regroup,
* and respawn positions. Also validates the herd setup and offers editor gizmos
* for visualizing patrol, regroup, and spawn radius.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages herd-level data and positioning logic for a group of sheep.
/// </summary>
public class HerdManager : MonoBehaviour
{
    [Header("Herd Members")]
    [Tooltip("The sheep that leads this herd.")]
    [SerializeField] private Sheep _commander;

    [Tooltip("All sheep that belong to this herd.")]
    [SerializeField] private List<Sheep> _herdPool = new List<Sheep>();

    [Tooltip("Formation offsets used to position normal sheep around the herd anchor.")]
    [SerializeField] private Vector3[] _slotOffsets;

    [Tooltip("Radius around the herd anchor used for random regroup positions."), Range(1f, 100f)]
    [SerializeField] private float _regroupRadius;

    [Tooltip("Radius around the herd anchor used for random patrol positions."), Range(1f, 100f)]
    [SerializeField] private float _patrolRadius;

    [Tooltip("Radius around the herd anchor used for random spawn or respawn positions."), Range(1f, 100f)]
    [SerializeField] private float _spawnRadius;

    [Header("Gizmo Settings")]
    [Tooltip("If enabled, draws herd radius gizmos in the Scene view.")]
    [SerializeField] private bool _showGizmo = false;

    [Tooltip("Gizmo color used for the patrol radius visualization.")]
    [SerializeField] private Color _patrolRadiusColor;

    [Tooltip("Gizmo color used for the regroup radius visualization.")]
    [SerializeField] private Color _regroupRadiusColor;

    [Tooltip("Gizmo color used for the spawn radius visualization.")]
    [SerializeField] private Color _spawnRadiusColor;

    /// <summary>
    /// Indicates whether this herd has an assigned commander and whether that commander is alive.
    /// </summary>
    public bool IsCommanderAlive => _commander != null && _commander.IsAlive;

    /// <summary>
    /// Indicates whether this herd currently has an assigned commander.
    /// </summary>
    public bool HasCommander => _commander != null;

    private Vector3 _lastHerdAnchorPosition;

    private void Start()
    {
        ValidateHerd();
    }

    /// <summary>
    /// Gets the current anchor position of the herd.
    /// Uses the commander position when the commander is alive and tamed, otherwise uses the herd center
    /// of all living sheep. If no sheep are alive, the last known anchor position is returned.
    /// </summary>
    /// <returns>The current or last known herd anchor position.</returns>
    public Vector3 GetHerdAnchorPosition()
    {
        if (_commander != null && _commander.IsAlive && _commander.IsTamed)
        {
            return _lastHerdAnchorPosition = _commander.transform.position;
        }
        if (HasAliveSheep())
        {
            return _lastHerdAnchorPosition = CalculateHerdCenter();
        }
        return _lastHerdAnchorPosition;
    }

    /// <summary>
    /// Gets the formation target position for a specific sheep.
    /// The commander uses the herd anchor directly, while normal sheep receive an offset position.
    /// </summary>
    /// <param name="sheep">The sheep that needs a formation position.</param>
    /// <returns>The calculated formation position for the given sheep.</returns>
    public Vector3 GetFormationPositionForSheep(Sheep sheep)
    {
        Vector3 anchor = GetHerdAnchorPosition();

        if (!IsHerdMember(sheep))
            return anchor;

        if (sheep == _commander)
            return anchor;

        if (!sheep.IsAlive)
            return sheep.transform.position;

        int aliveIndex = GetAliveNormalSheepIndex(sheep);

        if (aliveIndex < 0)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name}: Sheep is not an alive member of this herd.");
#endif
            return anchor;
        }

        if (_slotOffsets == null || _slotOffsets.Length == 0)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name}: No slot offsets assigned.");
#endif
            return anchor;
        }

        int slotIndex = aliveIndex;

        if (slotIndex >= _slotOffsets.Length)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name}: Slot index is outside of the slot offset array.");
#endif
            return anchor;
        }
        Vector3 offset = _slotOffsets[slotIndex];
        if (_commander != null)
        {
            offset = _commander.transform.rotation * offset;
        }

        Vector3 targetPosition = anchor + offset;
        return targetPosition;
    }

    /// <summary>
    /// Gets a random regroup position around the current herd anchor.
    /// </summary>
    /// <returns>A random world position within the regroup radius.</returns>
    public Vector3 GetRandomRegroupPosition()
    {
        return GetRandomPositionInRadius(_regroupRadius);
    }

    /// <summary>
    /// Gets a random patrol position around the current herd anchor.
    /// </summary>
    /// <returns>A random world position within the patrol radius.</returns>
    public Vector3 GetRandomPatrolPosition()
    {
        return GetRandomPositionInRadius(_patrolRadius);
    }

    /// <summary>
    /// Gets a random spawn position around the current herd anchor.
    /// </summary>
    /// <returns>A random world position within the spawn radius.</returns>
    public Vector3 GetRandomSpawnPosition()
    {
        return GetRandomPositionInRadius(_spawnRadius);
    }

    /// <summary>
    /// Checks whether the given sheep is a herd member and has reached its current movement destination.
    /// </summary>
    /// <param name="sheep">The sheep to check.</param>
    /// <returns>True if the sheep belongs to this herd and has reached its destination; otherwise false.</returns>
    public bool IsSheepInPosition(Sheep sheep)
    {
        if (!IsHerdMember(sheep))
        {
#if UNITY_EDITOR
            Debug.LogError($"{name}: Sheep {sheep?.name} is not part of this herd.");
#endif
            return false;
        }

        if (sheep.Move == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name}: Sheep {sheep.name} has no Move behaviour.");
#endif
            return false;
        }

        return sheep.Move.HasReachedDestination();
    }

    /// <summary>
    /// Checks whether all living sheep in the herd have reached their current movement destination.
    /// </summary>
    /// <returns>True if all valid living sheep have reached their destination; otherwise false.</returns>
    public bool AreAllSheepInPosition()
    {
        for (int i = 0; i < _herdPool.Count; i++)
        {
            Sheep sheep = _herdPool[i];

            if (sheep == null)
                continue;
            if (!sheep.IsAlive)
                continue;
            if (sheep.Move == null)
                continue;

            if (!sheep.Move.HasReachedDestination())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a suitable respawn position for a sheep.
    /// Herd members respawn inside the spawn radius, while non-members keep their current position.
    /// </summary>
    /// <param name="sheep">The sheep that needs a respawn position.</param>
    /// <returns>The calculated respawn position for the given sheep.</returns>
    public Vector3 GetRespawnPositionForSheep(Sheep sheep)
    {
        if (sheep == null)
            return GetHerdAnchorPosition();
        if (!IsHerdMember(sheep))
            return sheep.transform.position;
        return GetRandomSpawnPosition();
    }

    /// <summary>
    /// Checks whether at least one sheep in the herd pool is alive.
    /// </summary>
    /// <returns>True if at least one valid sheep is alive; otherwise false.</returns>
    private bool HasAliveSheep()
    {
        foreach (Sheep sheep in _herdPool)
        {
            if (sheep == null)
                continue;
            if (sheep.IsAlive)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Calculates the center position of all living sheep in the herd.
    /// </summary>
    /// <returns>The average position of all living sheep, or the last known anchor position if none are alive.</returns>
    private Vector3 CalculateHerdCenter()
    {
        Vector3 sum = Vector3.zero;
        int sheepCount = 0;
        foreach (Sheep sheep in _herdPool)
        {
            if (sheep == null)
                continue;
            if (!sheep.IsAlive)
                continue;
            sheepCount++;
            sum += sheep.transform.position;
        }
        if (sheepCount <= 0)
        {
            return _lastHerdAnchorPosition;
        }
        Vector3 herdCenter = sum / sheepCount;
        return herdCenter;
    }

    /// <summary>
    /// Validates the configured commander and herd pool setup and logs warnings for missing or invalid entries.
    /// </summary>
    private void ValidateHerd()
    {
        if (_commander == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name}: No commander assigned.");
#endif
        }
        else
        {
            if (!_commander.IsCommander)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{name}: Assigned commander {_commander.name} is not marked as commander.");
#endif
            }

            if (!_herdPool.Contains(_commander))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{name}: Commander {_commander.name} is not part of the herd pool.");
#endif
            }
        }

        for (int i = 0; i < _herdPool.Count; i++)
        {
            if (_herdPool[i] == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{name}: Herd pool contains an empty slot at index {i}.");
#endif
            }
        }
    }

    /// <summary>
    /// Creates a random world position inside a radius around the current herd anchor.
    /// </summary>
    /// <param name="radius">The radius around the herd anchor used for random position generation.</param>
    /// <returns>A random position inside the given radius on the XZ plane.</returns>
    private Vector3 GetRandomPositionInRadius(float radius)
    {
        Vector3 anchor = GetHerdAnchorPosition();

        Vector2 randomCircle = Random.insideUnitCircle * radius;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);

        return anchor + randomOffset;
    }

    /// <summary>
    /// Gets the formation index of a living non-commander sheep.
    /// Dead sheep, null entries, and the commander are ignored.
    /// </summary>
    /// <param name="targetSheep">The sheep whose alive [normal] sheep index should be found.</param>
    /// <returns>The index of the sheep among living normal herd members, or -1 if it was not found.</returns>
    private int GetAliveNormalSheepIndex(Sheep targetSheep)
    {
        int aliveIndex = 0;

        foreach (Sheep sheep in _herdPool)
        {
            if (sheep == null)
                continue;

            if (!sheep.IsAlive)
                continue;
            if (sheep == _commander)
                continue;

            if (sheep == targetSheep)
                return aliveIndex;

            aliveIndex++;
        }

        return -1;
    }

    /// <summary>
    /// Checks whether the given sheep belongs to this herd.
    /// </summary>
    /// <param name="sheep">The sheep to check.</param>
    /// <returns>True if the sheep is not null and is part of the herd pool; otherwise false.</returns>
    private bool IsHerdMember(Sheep sheep)
    {
        return sheep != null && _herdPool.Contains(sheep);
    }

    /// <summary>
    /// Sets the herd movement flag for all non-commander sheep in the herd.
    /// </summary>
    /// <param name="isHerdMoving">Whether normal sheep should move as part of herd movement.</param>
    public void SetAllSheepHerdMoving(bool isHerdMoving)
    {
        foreach (Sheep sheep in _herdPool)
        {
            if (sheep == null)
                continue;
            if (sheep.IsCommander)
                continue;
            sheep.IsHerdMoving = isHerdMoving;
        }
    }

#if(UNITY_EDITOR)

    /// <summary>
    /// Draws herd-related radius gizmos in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (_showGizmo)
        {
            Vector3 anchor = Application.isPlaying ? GetHerdAnchorPosition() : transform.position;

            Gizmos.color = _patrolRadiusColor;
            Gizmos.DrawWireSphere(anchor, _patrolRadius);

            Gizmos.color = _spawnRadiusColor;
            Gizmos.DrawWireSphere(anchor, _spawnRadius);

            Gizmos.color = _regroupRadiusColor;
            Gizmos.DrawWireSphere(anchor, _regroupRadius);
        }
    }

#endif
}