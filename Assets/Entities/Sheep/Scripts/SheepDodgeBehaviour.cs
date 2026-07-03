/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SheepDodgeBehaviour.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Handles obstacle dodge behavior for a sheep entity.
* Uses configurable raycasts to detect nearby obstacles, calculates a valid
* side dodge target on the NavMesh, starts the dodge movement, and reports
* whether the sheep is currently dodging.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles dodge detection and dodge movement for a sheep using configurable raycasts
/// and NavMesh validation.
/// </summary>
public class SheepDodgeBehaviour : DodgeBehaviourBase
{
    /// <summary>
    /// Defines one local raycast used to detect obstacles around the sheep.
    /// </summary>
    [Serializable]
    private struct DodgeRaycastSetting
    {
        [Tooltip("Display name of this raycast setting.")]
        public string Name;

        [Tooltip("Local direction of the raycast relative to the sheep.")]
        public Vector3 LocalDirection;

        [Tooltip("Maximum distance of the raycast.")]
        public float Distance;

        [Tooltip("Layer mask used to detect dodge obstacles.")]
        public LayerMask SearchLayer;
    }

    [Header("Raycast Settings")]
    [Tooltip("Raycast configurations used to detect nearby dodge obstacles.")]
    [SerializeField] private List<DodgeRaycastSetting> _raycastSettings = new List<DodgeRaycastSetting>();

    [Header("Dodge Movement Settings")]
    [Tooltip("Sideways distance applied when calculating the dodge target position.")]
    [SerializeField] private float _sideDodgeDistance = 3f;

    [Tooltip("Forward distance applied when calculating the dodge target position.")]
    [SerializeField] private float _forwardDodgeDistance = 3f;

    [Tooltip("Radius used when sampling the calculated dodge position on the NavMesh.")]
    [SerializeField] private float _sampleRadius = 1f;

    [Header("Gizmo Settings")]
    [Tooltip("If enabled, draws dodge raycasts and the selected dodge target position.")]
    [SerializeField] private bool _debugGizmosOn = false;

    [Tooltip("Color used for dodge detection ray gizmos.")]
    [SerializeField] private Color _rayColor = Color.red;

    [Tooltip("Color used for the selected dodge target point.")]
    [SerializeField] private Color _dodgePointColor = Color.yellow;

    [Tooltip("Size of the debug point drawn at the selected dodge target position.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float _drawPointSize = 0.15f;

    private NavMeshAgent _agent;
    private NavMeshPath _navPath;

    private RaycastHit _obstacleHit;
    private Vector3 _drawPoint;

    private bool _obstacleDetected;
    private bool _isDodging;
    private bool _hasDrawPoint;

    /// <summary>
    /// Indicates whether an obstacle was detected and the sheep should dodge.
    /// </summary>
    public override bool ShouldDodge => _obstacleDetected;

    /// <summary>
    /// Indicates whether the sheep is currently performing a dodge movement.
    /// </summary>
    public override bool IsDodging => _isDodging;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _navPath = new NavMeshPath();
    }

    private void OnDisable()
    {
        _obstacleDetected = false;
        _isDodging = false;
        _hasDrawPoint = false;
        _obstacleHit = default;
        _drawPoint = Vector3.zero;
    }

    private void Update()
    {
        _obstacleDetected = DetectObstacle();

        if (_isDodging && HasReachedDestination())
        {
            _isDodging = false;
            _hasDrawPoint = false;
        }
    }

    /// <summary>
    /// Tries to start a dodge movement and returns the movement target that was active before dodging.
    /// </summary>
    /// <param name="previousTarget">The movement target that should be resumed after dodging.</param>
    /// <returns>True if the dodge movement was started successfully; otherwise false.</returns>
    public override bool TryStartDodge(out Vector3 previousTarget)
    {
        previousTarget = GetPreviousTarget();

        if (_agent == null || !_agent.enabled)
            return false;

        if (_isDodging)
            return false;

        if (!TryGetDodgeDirection(out Vector3 dodgeDirection))
            return false;

        if (!TryGetValidDodgePosition(dodgeDirection, out Vector3 validPosition))
            return false;

        StartDodgeMovement(validPosition);
        return true;
    }

    /// <summary>
    /// Gets the current NavMeshAgent destination so it can be resumed after dodging.
    /// </summary>
    /// <returns>The current agent destination, or the current position if no path exists.</returns>
    private Vector3 GetPreviousTarget()
    {
        if (_agent == null || !_agent.enabled)
            return transform.position;

        return _agent.hasPath ? _agent.destination : transform.position;
    }

    /// <summary>
    /// Calculates the side direction the sheep should dodge toward based on the detected obstacle position.
    /// </summary>
    /// <param name="dodgeDirection">The resulting world-space dodge direction.</param>
    /// <returns>True if a valid dodge direction was calculated; otherwise false.</returns>
    private bool TryGetDodgeDirection(out Vector3 dodgeDirection)
    {
        dodgeDirection = Vector3.zero;

        if (_obstacleHit.transform == null)
            return false;

        Vector3 obstacleDirection = (_obstacleHit.transform.position - transform.position).normalized;
        obstacleDirection.y = 0f;

        float sideValue = Vector3.Dot(transform.right, obstacleDirection);

        dodgeDirection = sideValue > 0f
            ? -transform.right
            : transform.right;

        return true;
    }

    /// <summary>
    /// Calculates and validates a dodge target position on the NavMesh.
    /// </summary>
    /// <param name="dodgeDirection">The side direction used for the dodge movement.</param>
    /// <param name="validPosition">The resulting valid NavMesh position.</param>
    /// <returns>True if a valid dodge target was found; otherwise false.</returns>
    private bool TryGetValidDodgePosition(Vector3 dodgeDirection, out Vector3 validPosition)
    {
        validPosition = transform.position;

        Vector3 offset = dodgeDirection * _sideDodgeDistance + transform.forward * _forwardDodgeDistance;
        Vector3 wantedPosition = transform.position + offset;

        if (!NavMesh.SamplePosition(wantedPosition, out NavMeshHit hit, _sampleRadius, _agent.areaMask))
        {
#if UNITY_EDITOR
            Debug.Log($"{name}: No valid NavMesh position found for dodge.");
#endif
            return false;
        }

        if (!_agent.CalculatePath(hit.position, _navPath))
        {
#if UNITY_EDITOR
            Debug.Log($"{name}: Could not calculate path to dodge position.");
#endif
            return false;
        }

        if (_navPath.status != NavMeshPathStatus.PathComplete)
        {
#if UNITY_EDITOR
            Debug.Log($"{name}: Dodge path is not complete.");
#endif
            return false;
        }

        validPosition = hit.position;
        return true;
    }

    /// <summary>
    /// Starts the dodge movement toward the given valid NavMesh position.
    /// </summary>
    /// <param name="validPosition">The valid dodge target position.</param>
    private void StartDodgeMovement(Vector3 validPosition)
    {
        _drawPoint = validPosition;
        _hasDrawPoint = true;

        _agent.SetDestination(validPosition);
        _isDodging = true;
    }

    /// <summary>
    /// Checks whether the dodge movement has reached its current NavMesh destination.
    /// </summary>
    /// <returns>True if the agent has reached its destination or cannot move; otherwise false.</returns>
    private bool HasReachedDestination()
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
    /// Detects nearby dodge obstacles using the configured raycasts.
    /// </summary>
    /// <returns>True if an obstacle was detected; otherwise false.</returns>
    private bool DetectObstacle()
    {
        if (_raycastSettings == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name}: Dodge raycast settings are missing.");
#endif
            return false;
        }

        foreach (DodgeRaycastSetting setting in _raycastSettings)
        {
            Vector3 direction = transform.TransformDirection(setting.LocalDirection.normalized);

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, setting.Distance, setting.SearchLayer))
            {
                _obstacleHit = hit;
                return true;
            }
        }

        _obstacleHit = default;
        return false;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!_debugGizmosOn)
            return;

        DrawRaycastGizmos();
        DrawDodgeTargetGizmo();
    }

    /// <summary>
    /// Draws all configured dodge raycasts in the Scene view.
    /// </summary>
    private void DrawRaycastGizmos()
    {
        if (_raycastSettings == null)
            return;

        Gizmos.color = _rayColor;

        foreach (DodgeRaycastSetting setting in _raycastSettings)
        {
            Vector3 direction = transform.TransformDirection(setting.LocalDirection.normalized);
            Vector3 endPoint = transform.position + direction * setting.Distance;

            Gizmos.DrawLine(transform.position, endPoint);
        }
    }

    /// <summary>
    /// Draws the currently selected dodge target position as a small point.
    /// </summary>
    private void DrawDodgeTargetGizmo()
    {
        if (!_hasDrawPoint)
            return;

        Gizmos.color = _dodgePointColor;
        Gizmos.DrawSphere(_drawPoint, _drawPointSize);
    }

#endif
}