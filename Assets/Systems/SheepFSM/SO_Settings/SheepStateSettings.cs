/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SheepStateSettings.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Stores timing and behaviour configuration values for sheep FSM states.
* Contains idle, alert, patrol, flee, and respawn timing values used by the
* registered sheep state instances during runtime.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// ScriptableObject that stores timing and behaviour values for sheep FSM states.
/// </summary>
[CreateAssetMenu(fileName = "SheepStateSettings", menuName = "Animals/Sheep/Sheep State Settings")]
public class SheepStateSettings : ScriptableObject
{
    [Header("Idle State")]
    [Tooltip("How long the sheep stays idle before leaving the idle state.")]
    [Range(0.1f, 300f)]
    [SerializeField] private float _idleTime = 5f;

    [Header("Alert State")]
    [Tooltip("How long the sheep can stay alert before returning to patrol behaviour.")]
    [Range(0.1f, 300f)]
    [SerializeField] private float _alertTime = 22f;

    [Tooltip("How often the sheep evaluates its surroundings while alert.")]
    [Range(0.05f, 60f)]
    [SerializeField] private float _reactionTime = 1.5f;
   
    [Header("Flee State")]
    [Tooltip("Time interval for recalculating a new flee target after the current flee target has been reached.")]
    [Range(0.05f, 60f)]
    [SerializeField] private float _updateTickNewPosition = 0.5f;

    [Header("Dead / Respawn State")]
    [Tooltip("How long the sheep stays dead before the respawn process starts.")]
    [Range(0.1f, 300f)]
    [SerializeField] private float _spawnTime = 10f;
    [Tooltip("DespawnTime for Animation or fadeout.")]
    [Range(0.1f, 300f)]
    [SerializeField] private float _deSpawnTime = 2f;

    // ===== IDLE =====

    /// <summary>
    /// Gets the duration of the idle state.
    /// </summary>
    public float IdleTime => _idleTime;

    // ===== ALERT =====

    /// <summary>
    /// Gets the maximum alert duration.
    /// </summary>
    public float AlertTime => _alertTime;

    /// <summary>
    /// Gets the reaction interval used while alert.
    /// </summary>
    public float ReactionTime => _reactionTime;    

    // ===== FLEE =====

    /// <summary>
    /// Gets the interval used for updating flee target positions.
    /// </summary>
    public float UpdateTickNewPosition => _updateTickNewPosition;

    // ===== DEAD / RESPAWN =====

    /// <summary>
    /// Gets the time before the sheep respawns.
    /// </summary>
    public float SpawnTime => _spawnTime;

    /// <summary>
    /// Gets the time before the sheep despawns.
    /// </summary>
    public float DeSpawnTime => _deSpawnTime;
}