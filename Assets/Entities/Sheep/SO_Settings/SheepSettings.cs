/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SheepSettings.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Stores shared configuration values for sheep entities.
* Contains general sheep type data, health and hunger values, movement and flee
* settings, sensing ranges, sensing layer masks, and debug visualization colors.
* These values are used by multiple sheep behaviour components at runtime.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// ScriptableObject that stores shared configuration values for sheep entities.
/// </summary>
[CreateAssetMenu(fileName = "SheepSettings", menuName = "Animals/Sheep/Sheep Settings")]
public class SheepSettings : ScriptableObject
{
    [Header("General")]
    [Tooltip("Defines the role of this sheep inside the herd.")]
    [SerializeField] private SheepType _type = SheepType.Normal;

    [Header("Health")]
    [Tooltip("Maximum health value of the sheep.")]
    [Range(1, 1000)]
    [SerializeField] private int _maxHealth = 100;

    [Header("Hunger")]
    [Tooltip("Time interval in seconds between two hunger ticks.")]
    [Range(0.1f, 300f)]
    [SerializeField] private float _hungerTickInterval = 10f;

    [Tooltip("Amount of hunger added per hunger tick.")]
    [Range(1, 100)]
    [SerializeField] private int _hungerTickRate = 4;

    [Tooltip("Amount of hunger removed per eating tick.")]
    [Range(1, 100)]
    [SerializeField] private int _eatTickRate = 4;

    [Tooltip("Hunger value above which the sheep is considered hungry.")]
    [Range(0, 100)]
    [SerializeField] private int _hungerThreshold = 50;

    [Tooltip("Maximum hunger value before starvation begins.")]
    [Range(1, 100)]
    [SerializeField] private int _maxHunger = 100;

    [Tooltip("Damage applied when the sheep is starving.")]
    [Range(1, 100)]
    [SerializeField] private int _starvationDamage = 1;

    [Header("Walk Movement")]
    [Tooltip("Movement speed used during normal walking behaviour.")]
    [Range(0.1f, 20f)]
    [SerializeField] private float _walkSpeed = 1f;

    [Tooltip("Acceleration used during normal walking behaviour.")]
    [Range(0.1f, 100f)]
    [SerializeField] private float _walkAcceleration = 2f;

    [Tooltip("Angular speed used during normal walking behaviour.")]
    [Range(1f, 720f)]
    [SerializeField] private float _walkAngularSpeed = 120f;

    [Header("Flee Movement")]
    [Tooltip("Movement speed used while fleeing.")]
    [Range(0.1f, 30f)]
    [SerializeField] private float _fleeSpeed = 3f;

    [Tooltip("Acceleration used while fleeing.")]
    [Range(0.1f, 100f)]
    [SerializeField] private float _fleeAcceleration = 6f;

    [Tooltip("Angular speed used while fleeing.")]
    [Range(1f, 720f)]
    [SerializeField] private float _fleeAngularSpeed = 360f;

    [Tooltip("Minimum distance used when searching for flee target positions.")]
    [Range(1f, 50f)]
    [SerializeField] private float _minFleeDistance = 8f;

    [Tooltip("Maximum distance used when searching for flee target positions.")]
    [Range(1f, 100f)]
    [SerializeField] private float _maxFleeDistance = 10f;

    [Tooltip("Maximum random sideways offset applied to flee target positions.")]
    [Range(0f, 50f)]
    [SerializeField] private float _fleeDistanceSideOffset = 10f;

    [Header("Sense Radius")]
    [Tooltip("Radius used to detect threats.")]
    [Range(0.1f, 200f)]
    [SerializeField] private float _threatRadius = 10f;

    [Tooltip("Radius used to detect nearby sheep.")]
    [Range(0.1f, 200f)]
    [SerializeField] private float _sheepRadius = 8f;

    [Tooltip("Radius used to detect the player.")]
    [Range(0.1f, 200f)]
    [SerializeField] private float _playerRadius = 12f;

    [Tooltip("Radius used to detect commander sheep.")]
    [Range(0.1f, 200f)]
    [SerializeField] private float _commanderRadius = 15f;

    [Tooltip("Distance at which the player is considered too close and may scare the sheep.")]
    [Range(0.1f, 50f)]
    [SerializeField] private float _fearRadiusForPlayer = 2f;

    [Tooltip("Distance at which the player is close enough to tame the sheep.")]
    [Range(0.1f, 50f)]
    [SerializeField] private float _distanceForTaming = 4f;

    [Header("Sense Layers")]
    [Tooltip("Layer mask used to detect threats.")]
    [SerializeField] private LayerMask _threatLayer;

    [Tooltip("Layer mask used to detect other sheep.")]
    [SerializeField] private LayerMask _sheepLayer;

    [Tooltip("Layer mask used to detect the player.")]
    [SerializeField] private LayerMask _playerLayer;

    [Header("Debug Colors")]
    [Tooltip("Gizmo color used for threat detection.")]
    [SerializeField] private Color _colorThreat = Color.red;

    [Tooltip("Gizmo color used for sheep detection.")]
    [SerializeField] private Color _colorSheep = Color.green;

    [Tooltip("Gizmo color used for player detection.")]
    [SerializeField] private Color _colorPlayer = Color.blue;

    [Tooltip("Gizmo color used for commander detection.")]
    [SerializeField] private Color _colorCommander = Color.yellow;

    // ===== GENERAL =====

    /// <summary>
    /// Gets the role of this sheep inside the herd.
    /// </summary>
    public SheepType Typ => _type;

    /// <summary>
    /// Gets the role of this sheep inside the herd.
    /// </summary>
    public SheepType Type => _type;

    // ===== HEALTH =====

    /// <summary>
    /// Gets the maximum health value of the sheep.
    /// </summary>
    public int MaxHealth => _maxHealth;

    // ===== HUNGER =====

    /// <summary>
    /// Gets the time interval between two hunger ticks.
    /// </summary>
    public float HungerTickInterval => _hungerTickInterval;

    /// <summary>
    /// Gets the amount of hunger added per hunger tick.
    /// </summary>
    public int HungerTick => _hungerTickRate;

    /// <summary>
    /// Gets the amount of hunger removed per eating tick.
    /// </summary>
    public int EatTickRate => _eatTickRate;   

    /// <summary>
    /// Gets the hunger value above which the sheep is considered hungry.
    /// </summary>
    public int HungerThreshold => _hungerThreshold;

    /// <summary>
    /// Gets the maximum hunger value.
    /// </summary>
    public int MaxHunger => _maxHunger;

    /// <summary>
    /// Gets the damage applied while starving.
    /// </summary>
    public int StarvationDamage => _starvationDamage;

    // ===== WALK MOVEMENT =====

    /// <summary>
    /// Gets the normal walking speed.
    /// </summary>
    public float WalkSpeed => _walkSpeed;

    /// <summary>
    /// Gets the normal walking acceleration.
    /// </summary>
    public float WalkAcceleration => _walkAcceleration;

    /// <summary>
    /// Gets the normal walking angular speed.
    /// </summary>
    public float WalkAngularSpeed => _walkAngularSpeed;

    // ===== FLEE MOVEMENT =====

    /// <summary>
    /// Gets the flee movement speed.
    /// </summary>
    public float FleeSpeed => _fleeSpeed;

    /// <summary>
    /// Gets the flee movement acceleration.
    /// </summary>
    public float FleeAcceleration => _fleeAcceleration;

    /// <summary>
    /// Gets the flee movement angular speed.
    /// </summary>
    public float FleeAngularSpeed => _fleeAngularSpeed;

    /// <summary>
    /// Gets the minimum flee target distance.
    /// </summary>
    public float MinFleeDistance => _minFleeDistance;

    /// <summary>
    /// Gets the maximum flee target distance.
    /// </summary>
    public float MaxFleeDistance => _maxFleeDistance;

    /// <summary>
    /// Gets the random sideways flee offset range.
    /// </summary>
    public float FleeDistanceSideOffset => _fleeDistanceSideOffset;

    // ===== SENSE RADII =====

    /// <summary>
    /// Gets the threat detection radius.
    /// </summary>
    public float ThreatRadius => _threatRadius;

    /// <summary>
    /// Gets the sheep detection radius.
    /// </summary>
    public float SheepRadius => _sheepRadius;

    /// <summary>
    /// Gets the player detection radius.
    /// </summary>
    public float PlayerRadius => _playerRadius;

    /// <summary>
    /// Gets the commander detection radius.
    /// </summary>
    public float CommanderRadius => _commanderRadius;    

    /// <summary>
    /// Gets the distance at which the player is considered too close.
    /// </summary>
    public float FearRadiusForPlayer => _fearRadiusForPlayer;

    /// <summary>
    /// Gets the distance at which the player can tame the sheep.
    /// </summary>
    public float DistanceForTaming => _distanceForTaming;

    // ===== SENSE LAYERS =====

    /// <summary>
    /// Gets the threat detection layer mask.
    /// </summary>
    public LayerMask ThreatLayer => _threatLayer;

    /// <summary>
    /// Gets the sheep detection layer mask.
    /// </summary>
    public LayerMask SheepLayer => _sheepLayer;

    /// <summary>
    /// Gets the player detection layer mask.
    /// </summary>
    public LayerMask PlayerLayer => _playerLayer;

    // ===== DEBUG COLORS =====

    /// <summary>
    /// Gets the threat gizmo color.
    /// </summary>
    public Color ColorThreat => _colorThreat;

    /// <summary>
    /// Gets the sheep gizmo color.
    /// </summary>
    public Color ColorSheep => _colorSheep;

    /// <summary>
    /// Gets the player gizmo color.
    /// </summary>
    public Color ColorPlayer => _colorPlayer;

    /// <summary>
    /// Gets the commander gizmo color.
    /// </summary>
    public Color ColorCommander => _colorCommander;
    
}