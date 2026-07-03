/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SheepMoveBehaviour.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Handles NavMesh-based movement for a sheep entity.
* Provides basic movement, stopping, following, walking, and fleeing behavior.
* Calculates valid NavMesh target positions and searches for safe flee targets
* away from threats using configurable movement and pathfinding settings.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls NavMeshAgent-based movement for a sheep, including walking,
/// following targets, stopping, validating destinations, and fleeing from threats.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SheepMoveBehaviour : MonoBehaviour
{
    [Tooltip("ScriptableObject that contains the base movement and flee configuration for this sheep.")]
    [SerializeField] private SheepSettings _settings;

    [Header("Flee Position Finding Settings")]
    [Tooltip("Radius used when sampling nearby NavMesh positions for flee target validation.")]
    [Range(1, 4)]
    [SerializeField] private float _sampleRadius = 2f;

    [Tooltip("Minimum distance the sheep should move away when choosing a flee target.")]
    [Range(4, 20)]
    [SerializeField] private float _minFleeDistance = 6f;

    [Tooltip("Maximum distance the sheep may move away when choosing a flee target.")]
    [Range(4, 40)]
    [SerializeField] private float _maxFleeDistance = 10f;

    [Tooltip("Maximum sideways offset applied to flee target candidates to avoid fleeing in a perfectly straight line.")]
    [Range(1, 20)]
    [SerializeField] private float _fleeDistanceSideOffset = 10f;

    [Tooltip("Maximum number of random flee target candidates tested before giving up.")]
    [Range(1, 1000)]
    [SerializeField] private int _maxSearchTries = 100;

    [Header("Movement Settings")]
    [Tooltip("Movement speed used for normal walking behavior.")]
    [Range(1, 1000)]
    [SerializeField] private float _walkSpeed = 2f;

    [Tooltip("Acceleration used for normal walking behavior.")]
    [Range(1, 1000)]
    [SerializeField] private float _walkAcceleration = 4f;

    [Tooltip("Angular speed used for normal walking behavior.")]
    [Range(1, 1000)]
    [SerializeField] private float _walkAngularSpeed = 120f;

    [Header("Flee Movement Settings")]
    [Tooltip("Movement speed used while fleeing from a threat.")]
    [Range(1, 1000)]
    [SerializeField] private float _fleeSpeed = 6f;

    [Tooltip("Acceleration used while fleeing from a threat.")]
    [Range(1, 1000)]
    [SerializeField] private float _fleeAcceleration = 14f;

    [Tooltip("Angular speed used while fleeing from a threat.")]
    [Range(1, 1000)]
    [SerializeField] private float _fleeAngularSpeed = 360f;

    [Header("Rotation Settings")]
    [Tooltip("Rotation speed used when the sheep turns toward a target.")]
    [Range(1f, 720f)]
    [SerializeField] private float _lookRotationSpeed = 180f;

    [Header("Follow Settings")]
    [Tooltip("Distance behind the followed target used to calculate the follow position.")]
    [Range(0f, 20f)]
    [SerializeField] private float _behindOffset = 4f;

    private NavMeshAgent _agent;
    private NavMeshPath _fleePath;
    private NavMeshPath _validPath;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _fleePath = new NavMeshPath();
        _validPath = new NavMeshPath();
        SetBaseValues();
    }

    private void OnValidate()
    {
        if (_maxFleeDistance < _minFleeDistance)
        {
            _maxFleeDistance = _minFleeDistance + 1;
        }
    }

    /// <summary>
    /// Moves the sheep to the given world position using the NavMeshAgent.
    /// </summary>
    /// <param name="targetPosition">The world position the sheep should move to.</param>
    public void MoveTo(Vector3 targetPosition)
    {
        if (_agent == null || !_agent.enabled)
            return;

        _agent.isStopped = false;
        _agent.SetDestination(targetPosition);
    }

    /// <summary>
    /// Stops the sheep immediately and clears its current NavMesh path.
    /// </summary>
    public void StopMoving()
    {
        if (_agent == null || !_agent.enabled)
            return;
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    /// <summary>
    /// Moves the sheep toward a position behind the given target transform.
    /// </summary>
    /// <param name="target">The transform the sheep should follow behind.</param>
    public void FollowBehind(Transform target)
    {
        if (target == null)
            return;
        Vector3 followPosition = target.position - target.forward * _behindOffset;
        MoveTo(followPosition);
    }

    /// <summary>
    /// Smoothly rotates the sheep toward the given target transform.
    /// </summary>
    /// <param name="target">The transform the sheep should look at.</param>
    public void RotateTowardsTarget(Transform target)
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            _lookRotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Checks whether the sheep has reached its current NavMesh destination.
    /// </summary>
    /// <returns>True if the agent has reached its destination or cannot move; otherwise false.</returns>
    public bool HasReachedDestination()
    {
        if (_agent == null || !_agent.enabled)
            return true;
        if (_agent.pathPending)
            return false;
        if (_agent.remainingDistance > _agent.stoppingDistance)
            return false;

        return !_agent.hasPath || _agent.velocity.sqrMagnitude <= 0.01f;
    }

    /// <summary>
    /// Finds a valid flee target away from the given threat position and moves the sheep toward it.
    /// </summary>
    /// <param name="threatPosition">The world position of the threat the sheep should flee from.</param>
    public void FleeFrom(Vector3 threatPosition)
    {
        Vector3 targetPosition = TryGetBestFleeTarget(threatPosition);
        if (targetPosition == transform.position)
            return;

        MoveTo(targetPosition);
    }

    /// <summary>
    /// Applies normal walking movement values to the NavMeshAgent.
    /// </summary>
    public void SetWalkMovement()
    {
        if (_agent == null || !_agent.enabled)
            return;
        _agent.speed = _walkSpeed;
        _agent.acceleration = _walkAcceleration;
        _agent.angularSpeed = _walkAngularSpeed;
        _agent.stoppingDistance = 2f;
    }

    /// <summary>
    /// Applies movement values used while normal sheep move in herd formation.
    /// </summary>
    public void SetHerdMovement()
    {
        if (_agent == null || !_agent.enabled)
            return;
        _agent.speed = _walkSpeed;
        _agent.acceleration = _walkAcceleration;
        _agent.angularSpeed = _walkAngularSpeed;
        _agent.stoppingDistance = 0.2f;
    }

    /// <summary>
    /// Applies fleeing movement values to the NavMeshAgent.
    /// </summary>
    public void SetFleeMovement()
    {
        if (_agent == null || !_agent.enabled)
            return;
        _agent.speed = _fleeSpeed;
        _agent.acceleration = _fleeAcceleration;
        _agent.angularSpeed = _fleeAngularSpeed;
        _agent.stoppingDistance = 2f;
    }

    /// <summary>
    /// Searches for the best valid flee target on the NavMesh that increases distance from the threat.
    /// </summary>
    /// <param name="threat">The world position of the threat.</param>
    /// <returns>A valid NavMesh flee target, or the current position if no valid target was found.</returns>
    public Vector3 TryGetBestFleeTarget(Vector3 threat)
    {
        if (_agent == null || !_agent.enabled)
            return transform.position;

        float currentDistanceToThreat = Vector3.Distance(transform.position, threat);
        for (int i = 0; i < _maxSearchTries; i++)
        {
            Vector3 randomPosition = GetRandomFleePosition(threat);

            if (!NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, _sampleRadius, _agent.areaMask))
            {
                continue;
            }

            float distanceToSheep = Vector3.Distance(transform.position, hit.position);

            if (distanceToSheep < _minFleeDistance)
                continue;

            float targetDistanceToThreat = Vector3.Distance(threat, hit.position);

            if (targetDistanceToThreat <= currentDistanceToThreat)
                continue;

            if (!_agent.CalculatePath(hit.position, _fleePath))
                continue;

            if (_fleePath.status != NavMeshPathStatus.PathComplete)
                continue;

            return hit.position;
        }
        return transform.position;
    }

    /// <summary>
    /// Tries to convert a wanted world position into a reachable NavMesh position.
    /// </summary>
    /// <param name="wantedPosition">The desired world position.</param>
    /// <param name="validPosition">The resulting valid NavMesh position if one was found.</param>
    /// <returns>True if a complete path to a valid NavMesh position was found; otherwise false.</returns>
    public bool TryGetValidTargetPosition(Vector3 wantedPosition, out Vector3 validPosition)
    {
        validPosition = transform.position;

        if (_agent == null || !_agent.enabled)
            return false;

        if (!NavMesh.SamplePosition(wantedPosition, out NavMeshHit hit, _sampleRadius, _agent.areaMask))
            return false;

        if (!_agent.CalculatePath(hit.position, _validPath))
            return false;

        if (_validPath.status != NavMeshPathStatus.PathComplete)
            return false;

        validPosition = hit.position;
        return true;
    }

    /// <summary>
    /// Loads movement and flee configuration values from the assigned SheepSettings ScriptableObject.
    /// </summary>
    private void SetBaseValues()
    {
        if (_settings == null)
        {
#if (UNITY_EDITOR)
            Debug.LogError($"{name}: No SheepSettings assigned.");
#endif
            return;
        }
        _minFleeDistance = _settings.MinFleeDistance;
        _maxFleeDistance = _settings.MaxFleeDistance;
        _fleeDistanceSideOffset = _settings.FleeDistanceSideOffset;
        _walkSpeed = _settings.WalkSpeed;
        _walkAcceleration = _settings.WalkAcceleration;
        _walkAngularSpeed = _settings.WalkAngularSpeed;
        _fleeSpeed = _settings.FleeSpeed;
        _fleeAcceleration = _settings.FleeAcceleration;
        _fleeAngularSpeed = _settings.FleeAngularSpeed;
    }

    /// <summary>
    /// Calculates the normalized direction pointing away from a threat position.
    /// </summary>
    /// <param name="threatPosition">The world position of the threat.</param>
    /// <returns>A normalized direction vector pointing away from the threat.</returns>
    private Vector3 GetFleeDirection(Vector3 threatPosition)
    {
        Vector3 fleeDirection = (transform.position - threatPosition).normalized;
        return fleeDirection;
    }

    /// <summary>
    /// Creates a random flee candidate position away from the threat with an additional sideways offset.
    /// </summary>
    /// <param name="threat">The world position of the threat.</param>
    /// <returns>A random flee candidate position before NavMesh validation.</returns>
    private Vector3 GetRandomFleePosition(Vector3 threat)
    {
        Vector3 fleeDirection = GetFleeDirection(threat);

        float distanceOffset = Random.Range(_minFleeDistance, _maxFleeDistance);

        Vector3 newFleePosition = transform.position + fleeDirection * distanceOffset;

        Vector3 sideDirection = Vector3.Cross(Vector3.up, fleeDirection).normalized;

        float sideOffset = Random.Range(-_fleeDistanceSideOffset, _fleeDistanceSideOffset);

        newFleePosition += sideDirection * sideOffset;

        newFleePosition.y = transform.position.y;

        return newFleePosition;
    }

    /// <summary>
    /// Sets the NavMeshAgent stopping distance.
    /// </summary>
    /// <param name="value">The stopping distance value to apply.</param>
    public void SetAgentStopDistance(float value)
    {
        _agent.stoppingDistance = value;
    }
}