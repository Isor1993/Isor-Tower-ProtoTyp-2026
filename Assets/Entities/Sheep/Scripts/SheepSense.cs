/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SheepSense.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Detects nearby objects relevant to a sheep entity, including threats,
* other sheep, the player, and commander sheep. Uses configurable detection
* radii and layer masks from SheepSettings, with optional local overrides
* for debugging and tuning. Also provides editor gizmos to visualize detection
* ranges and closest detected targets.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using System;
using UnityEngine;

/// <summary>
/// Handles perception for a sheep by detecting nearby threats, sheep, the player,
/// and commander sheep within configurable detection ranges.
/// </summary>
public class SheepSense : MonoBehaviour
{
    [Tooltip("ScriptableObject that contains the base sensing configuration for this sheep.")]
    [SerializeField] private SheepSettings _settings;

    [Header("Debug Settings")]
    [Tooltip("If enabled, this component uses the local inspector values instead of the values from SheepSettings.")]
    [SerializeField] private bool _useLocalOverrides = false;

    [Header("On selected Gizmos")]
    [Tooltip("Draws lines to the closest detected targets when this GameObject is selected.")]
    [SerializeField] private bool _showClosestDetectionLine = false;

    [Tooltip("Draws detection range wire spheres when this GameObject is selected.")]
    [SerializeField] private bool _showDetectionRangeWires = false;

    [Header("Permanent on Gizmos")]
    [Tooltip("Always draws lines to the closest detected targets in the Scene view.")]
    [SerializeField] private bool _showClosestDetectionLinePermanent = false;

    [Tooltip("Always draws detection range wire spheres in the Scene view.")]
    [SerializeField] private bool _showDetectionRangeWiresPermanent = false;

    [Tooltip("Distance at which the player is considered too close and may scare the sheep.")]
    [SerializeField] private float _fearDistanceForPlayer;

    [Tooltip("Distance at which the player is close enough to tame the sheep.")]
    [SerializeField] private float _distanceForTamingSheep;

    [Tooltip("Gizmo color used for threat detection visualization.")]
    [SerializeField] private Color _colorThreat;

    [Tooltip("Gizmo color used for sheep detection visualization.")]
    [SerializeField] private Color _colorSheep;

    [Tooltip("Gizmo color used for player detection visualization.")]
    [SerializeField] private Color _colorPlayer;

    [Tooltip("Gizmo color used for commander detection visualization.")]
    [SerializeField] private Color _colorCommander;

    [Header("Local Overrides (only if enabled)")]
    [Tooltip("Local detection radius for threats. Used only when local overrides are enabled."), Range(1, 200)]
    [SerializeField] private float _threatRadius;

    [Tooltip("Local detection radius for sheep. Used only when local overrides are enabled."), Range(1, 200)]
    [SerializeField] private float _sheepRadius;

    [Tooltip("Local detection radius for player. Used only when local overrides are enabled."), Range(1, 200)]
    [SerializeField] private float _playerRadius;

    [Tooltip("Local detection radius for commander. Used only when local overrides are enabled."), Range(1, 200)]
    [SerializeField] private float _commanderRadius;

    [Tooltip("Local layer mask used to detect threats. Used only when local overrides are enabled.")]
    [SerializeField] private LayerMask _threatLayer;

    [Tooltip("Local layer mask used to detect sheep. Used only when local overrides are enabled.")]
    [SerializeField] private LayerMask _sheepLayer;

    [Tooltip("Local layer mask used to detect the player. Used only when local overrides are enabled.")]
    [SerializeField] private LayerMask _playerLayer;

    private bool _isPlayerTooClose;
    private bool _isPlayerInTameRange;

    /// <summary>
    /// Gets the active threat detection radius, either from local overrides or from SheepSettings.
    /// </summary>
    private float ThreatRadius => _useLocalOverrides ? _threatRadius : _settings.ThreatRadius;

    /// <summary>
    /// Gets the active sheep detection radius, either from local overrides or from SheepSettings.
    /// </summary>
    private float SheepRadius => _useLocalOverrides ? _sheepRadius : _settings.SheepRadius;

    /// <summary>
    /// Gets the active player detection radius, either from local overrides or from SheepSettings.
    /// </summary>
    private float PlayerRadius => _useLocalOverrides ? _playerRadius : _settings.PlayerRadius;

    /// <summary>
    /// Gets the active commander detection radius, either from local overrides or from SheepSettings.
    /// </summary>
    private float CommanderRadius => _useLocalOverrides ? _commanderRadius : _settings.CommanderRadius;

    /// <summary>
    /// Gets the active threat layer mask, either from local overrides or from SheepSettings.
    /// </summary>
    private LayerMask ThreatLayer => _useLocalOverrides ? _threatLayer : _settings.ThreatLayer;

    /// <summary>
    /// Gets the active sheep layer mask, either from local overrides or from SheepSettings.
    /// </summary>
    private LayerMask SheepLayer => _useLocalOverrides ? _sheepLayer : _settings.SheepLayer;

    /// <summary>
    /// Gets the active player layer mask, either from local overrides or from SheepSettings.
    /// </summary>
    private LayerMask PlayerLayer => _useLocalOverrides ? _playerLayer : _settings.PlayerLayer;

    /// <summary>
    /// Gets the closest currently detected threat.
    /// </summary>
    public Transform CurrentThreat { get; private set; }

    /// <summary>
    /// Gets the closest currently detected sheep.
    /// </summary>
    public Transform CurrentSheep { get; private set; }

    /// <summary>
    /// Gets the closest currently detected player.
    /// </summary>
    public Transform CurrentPlayer { get; private set; }

    /// <summary>
    /// Gets the closest currently detected commander sheep.
    /// </summary>
    public Transform CurrentCommander { get; private set; }

    /// <summary>
    /// Indicates whether at least one threat is currently detected.
    /// </summary>
    public bool HasThreat { get; private set; }

    /// <summary>
    /// Indicates whether at least one other sheep is currently detected.
    /// </summary>
    public bool HasSheepInRange { get; private set; }

    /// <summary>
    /// Indicates whether the player is currently detected.
    /// </summary>
    public bool HasPlayerInRange { get; private set; }

    /// <summary>
    /// Indicates whether a commander sheep is currently detected.
    /// </summary>
    public bool HasCommanderInRange { get; private set; }

    /// <summary>
    /// Gets all colliders detected within the threat detection range.
    /// </summary>
    public Collider[] ThreatHits { get; private set; }

    /// <summary>
    /// Gets all colliders detected within the sheep detection range.
    /// </summary>
    public Collider[] SheepHits { get; private set; }

    /// <summary>
    /// Gets all colliders detected within the player detection range.
    /// </summary>
    public Collider[] PlayerHits { get; private set; }

    /// <summary>
    /// Gets all colliders detected within the commander detection range.
    /// </summary>
    public Collider[] CommanderHits { get; private set; }

    /// <summary>
    /// Indicates whether the player is close enough to scare the sheep.
    /// </summary>
    public bool IsPlayerTooClose => _isPlayerTooClose;

    /// <summary>
    /// Indicates whether the player is close enough to tame the sheep.
    /// </summary>
    public bool IsPlayerInTameRange => _isPlayerInTameRange;

    private void Awake()
    {
        SetBaseValues();
    }

    /// <summary>
    /// Copies the base sensing configuration and gizmo colors from SheepSettings into local fields.
    /// </summary>
    private void SetBaseValues()
    {
        if (_settings == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name}: No SheepSettings assigned.");
#endif
            return;
        }

        _threatRadius = _settings.ThreatRadius;
        _sheepRadius = _settings.SheepRadius;
        _playerRadius = _settings.PlayerRadius;
        _commanderRadius = _settings.CommanderRadius;
        _threatLayer = _settings.ThreatLayer;
        _sheepLayer = _settings.SheepLayer;
        _playerLayer = _settings.PlayerLayer;
        _colorThreat = _settings.ColorThreat;
        _colorSheep = _settings.ColorSheep;
        _colorPlayer = _settings.ColorPlayer;
        _colorCommander = _settings.ColorCommander;
        _fearDistanceForPlayer = _settings.FearRadiusForPlayer;
        _distanceForTamingSheep = _settings.DistanceForTaming;
    }

    private void Update()
    {
        CheckThreat();
        CheckSheepInRange();
        CheckPlayerInRange();
        CheckCommanderInRange();
    }

    /// <summary>
    /// Checks for nearby threats and stores the closest detected threat.
    /// </summary>
    private void CheckThreat()
    {
        ThreatHits = Physics.OverlapSphere(transform.position, ThreatRadius, ThreatLayer);

        CurrentThreat = TryGetClosest(ThreatHits);
        HasThreat = CurrentThreat != null;
    }

    /// <summary>
    /// Checks for nearby sheep and stores the closest detected sheep.
    /// </summary>
    private void CheckSheepInRange()
    {
        SheepHits = Physics.OverlapSphere(transform.position, SheepRadius, SheepLayer);

        CurrentSheep = TryGetClosest(SheepHits);
        HasSheepInRange = CurrentSheep != null;
    }

    /// <summary>
    /// Checks for the nearby player and stores the closest detected player transform.
    /// </summary>
    private void CheckPlayerInRange()
    {
        PlayerHits = Physics.OverlapSphere(transform.position, PlayerRadius, PlayerLayer);

        CurrentPlayer = TryGetClosest(PlayerHits);

        HasPlayerInRange = CurrentPlayer != null;

        _isPlayerTooClose = false;
        _isPlayerInTameRange = false;

        if (CurrentPlayer == null)
            return;

        float distance = Vector3.Distance(transform.position, CurrentPlayer.position);
        _isPlayerTooClose = distance < _fearDistanceForPlayer;
        _isPlayerInTameRange = distance < _distanceForTamingSheep;
    }

    /// <summary>
    /// Checks for nearby commander sheep and stores the closest detected commander.
    /// </summary>
    private void CheckCommanderInRange()
    {
        CommanderHits = Physics.OverlapSphere(transform.position, CommanderRadius, SheepLayer);

        CurrentCommander = TryGetClosestCommander(CommanderHits);
        HasCommanderInRange = CurrentCommander != null;
    }

    /// <summary>
    /// Finds the closest valid transform from a given collider array while ignoring this sheep itself.
    /// </summary>
    /// <param name="hits">The collider array returned by a physics overlap check.</param>
    /// <returns>The closest detected transform, or null if no valid target was found.</returns>
    private Transform TryGetClosest(Collider[] hits)
    {
        Transform closest = null;

        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hit.transform;
            }
        }
        return closest;
    }

    /// <summary>
    /// Finds the closest detected sheep that is marked as a commander.
    /// </summary>
    /// <param name="hits">The collider array returned by the commander detection overlap check.</param>
    /// <returns>The closest commander sheep transform, or null if no commander was found.</returns>
    private Transform TryGetClosestCommander(Collider[] hits)
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (!hit.TryGetComponent(out Sheep sheep))
                continue;

            if (!sheep.IsCommander)
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = sheep.transform;
            }
        }
        return closest;
    }

#if (UNITY_EDITOR)

    private void OnDrawGizmosSelected()
    {
        if (_settings == null)
            return;

        var currentThreatPosition = GetTargetPosition(CurrentThreat);
        var currentSheepPosition = GetTargetPosition(CurrentSheep);
        var currentPlayerPosition = GetTargetPosition(CurrentPlayer);
        var currentCommanderPosition = GetTargetPosition(CurrentCommander);

        if (_showDetectionRangeWires)
        {
            ShowDetectionAreaOnGizmos();
        }
        if (_showClosestDetectionLine)
        {
            ShowClosestDetectionOnGizmos(currentThreatPosition, currentSheepPosition, currentPlayerPosition, currentCommanderPosition);
        }
    }

    private void OnDrawGizmos()
    {
        if (_settings == null)
            return;

        var currentThreatPosition = GetTargetPosition(CurrentThreat);
        var currentSheepPosition = GetTargetPosition(CurrentSheep);
        var currentPlayerPosition = GetTargetPosition(CurrentPlayer);
        var currentCommanderPosition = GetTargetPosition(CurrentCommander);

        if (_showDetectionRangeWiresPermanent)
        {
            ShowDetectionAreaOnGizmos();
        }
        if (_showClosestDetectionLinePermanent)
        {
            ShowClosestDetectionOnGizmos(currentThreatPosition, currentSheepPosition, currentPlayerPosition, currentCommanderPosition);
        }
    }

    /// <summary>
    /// Draws colored lines from this sheep to the currently detected closest targets.
    /// </summary>
    /// <param name="currentThreatPosition">The position of the closest detected threat.</param>
    /// <param name="currentSheepPosition">The position of the closest detected sheep.</param>
    /// <param name="currentPlayerPosition">The position of the closest detected player.</param>
    /// <param name="currentCommanderPosition">The position of the closest detected commander sheep.</param>
    private void ShowClosestDetectionOnGizmos(Vector3 currentThreatPosition, Vector3 currentSheepPosition, Vector3 currentPlayerPosition, Vector3 currentCommanderPosition)
    {
        Gizmos.color = _colorThreat;
        Gizmos.DrawLine(transform.position, currentThreatPosition);

        Gizmos.color = _colorSheep;
        Gizmos.DrawLine(transform.position, currentSheepPosition);

        Gizmos.color = _colorPlayer;
        Gizmos.DrawLine(transform.position, currentPlayerPosition);

        Gizmos.color = _colorCommander;
        Gizmos.DrawLine(transform.position, currentCommanderPosition);
    }

    /// <summary>
    /// Draws colored wire spheres for all configured detection ranges.
    /// </summary>
    private void ShowDetectionAreaOnGizmos()
    {
        Gizmos.color = _colorThreat;
        Gizmos.DrawWireSphere(transform.position, ThreatRadius);

        Gizmos.color = _colorSheep;
        Gizmos.DrawWireSphere(transform.position, SheepRadius);

        Gizmos.color = _colorPlayer;
        Gizmos.DrawWireSphere(transform.position, PlayerRadius);

        Gizmos.color = _colorCommander;
        Gizmos.DrawWireSphere(transform.position, CommanderRadius);
    }

    /// <summary>
    /// Returns the position of a target transform or this sheep's position if the target is missing.
    /// </summary>
    /// <param name="target">The target transform to read the position from.</param>
    /// <returns>The target position, or this sheep's position if the target is null.</returns>
    private Vector3 GetTargetPosition(Transform target)
    {
        return target != null ? target.position : transform.position;
    }

#endif
}