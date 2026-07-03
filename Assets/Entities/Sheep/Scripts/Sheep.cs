/*****************************************************************************
* Project : Isors Tower Prototype
* File    : Sheep.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents a sheep entity and acts as the central access point for its
* core components, including sensing, hunger, health, movement, and FSM logic.
* Handles lifecycle events such as damage, death, spawning, starvation,
* day-night phase changes, and initializes the sheep FSM states.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Defines the role of a sheep inside a herd.
/// </summary>
public enum SheepType
{
    Normal,
    Commander
}

/// <summary>
/// Central sheep entity that connects all sheep-related components and controls
/// state transitions based on health, hunger, damage, and day-night events.
/// </summary>
[RequireComponent(typeof(SheepHealth))]
[RequireComponent(typeof(SheepSense))]
[RequireComponent(typeof(SheepHunger))]
[RequireComponent(typeof(SheepMoveBehaviour))]
[RequireComponent(typeof(SheepDodgeBehaviour))]
public class Sheep : MonoBehaviour, IDayNightListener
{
    [Header("Settings")]
    [Tooltip("ScriptableObject that contains the base configuration for this sheep.")]
    [SerializeField] private SheepSettings _settings;

    [Tooltip("ScriptableObject that contains FSM state-related settings for this sheep.")]
    [SerializeField] private SheepStateSettings _stateSettings;

    [Tooltip("Event manager used to subscribe this sheep to day-night phase change events.")]
    [SerializeField] private DayNightCycleEventManager _eventManager;

    [Tooltip("Herd manager that controls herd-level behavior and positioning for this sheep.")]
    [SerializeField] private HerdManager _herdManager;

    [Tooltip("Dodge behaviour used to detect and perform obstacle avoidance.")]
    [SerializeField] private DodgeBehaviourBase _dodgeBehaviour;

    [Tooltip("World position where the sheep is moved after dying.")]
    [SerializeField] private Transform _graveyardPosition;

    [Tooltip("Role of this sheep inside the herd.")]
    [SerializeField] private SheepType _typ;

    [Header("Testing Settings")]
    [Tooltip("Determines whether this sheep is currently tamed by the player.")]
    [SerializeField] private bool _isTamed = false;

    /// <summary>
    /// Indicates whether this sheep is currently moving as part of herd movement.
    /// </summary>
    public bool IsHerdMoving;

    private SkinnedMeshRenderer _visualRoot;
    private NavMeshAgent _agent;

    private bool _isSleeping = false;

    /// <summary>
    /// Gets the herd manager assigned to this sheep.
    /// </summary>
    public HerdManager HerdManager => _herdManager;

    /// <summary>
    /// Gets the movement component used by this sheep.
    /// </summary>
    public SheepMoveBehaviour Move { get; private set; }

    /// <summary>
    /// Gets the health component used by this sheep.
    /// </summary>
    public SheepHealth Health { get; private set; }

    /// <summary>
    /// Gets the hunger component used by this sheep.
    /// </summary>
    public SheepHunger Hunger { get; private set; }

    /// <summary>
    /// Gets the sensing component used by this sheep.
    /// </summary>
    public SheepSense Sense { get; private set; }

    /// <summary>
    /// Gets the finite state machine that controls this sheep's current behavior.
    /// </summary>
    public SheepFSM FSM { get; private set; }

    /// <summary>
    /// Gets the dodge behaviour used by this sheep.
    /// </summary>
    public DodgeBehaviourBase Dodge => _dodgeBehaviour;

    /// <summary>
    /// Gets the base settings assigned to this sheep.
    /// </summary>
    public SheepSettings Settings => _settings;

    /// <summary>
    /// Gets the type of this sheep inside the herd.
    /// </summary>
    public SheepType Typ => _typ;

    /// <summary>
    /// Gets the state settings assigned to this sheep.
    /// </summary>
    public SheepStateSettings StateSettings => _stateSettings;

    /// <summary>
    /// Indicates whether this sheep is the commander of its herd.
    /// </summary>
    public bool IsCommander => _typ == SheepType.Commander;

    /// <summary>
    /// Indicates whether this sheep is currently alive.
    /// </summary>
    public bool IsAlive => Health != null && Health.IsAlive;

    /// <summary>
    /// Indicates whether this sheep is currently asleep.
    /// </summary>
    public bool IsAsleep => _isSleeping;

    /// <summary>
    /// Indicates whether this sheep is currently tamed by the player.
    /// </summary>
    public bool IsTamed => _isTamed;

    /// <summary>
    /// Get animator from sheep.
    /// </summary>
    public Animator Animator { get => _animator; set => _animator = value; }

    private Animator _animator;


    private void Awake()
    {
        Health = GetComponent<SheepHealth>();
        Hunger = GetComponent<SheepHunger>();
        Sense = GetComponent<SheepSense>();
        Move = GetComponent<SheepMoveBehaviour>();
        _agent = GetComponent<NavMeshAgent>();
        _visualRoot = GetComponentInChildren<SkinnedMeshRenderer>();
        Animator = GetComponent<Animator>();

        FSM = new SheepFSM();
        _typ = Settings.Typ;
        RegisterStates();
    }

    private void Start()
    {
        if (_eventManager != null)
            _eventManager.Subscribe(this);

        FSM.ChangeState<IdleState>();
    }

    private void OnEnable()
    {       

        if (Health != null)
        {
            Health.OnDied += TransisionDeadState;
            Health.OnDamaged += HandleDamage;
        }

        if (Hunger != null)
            Hunger.OnStarving += HandleStarving;
    }

    private void OnDisable()
    {
        if (_eventManager != null)
            _eventManager.Unsubscribe(this);

        if (Health != null)
        {
            Health.OnDied -= TransisionDeadState;
            Health.OnDamaged -= HandleDamage;
        }

        if (Hunger != null)
            Hunger.OnStarving -= HandleStarving;
    }

    private void Update()
    {
        FSM.Tick();
        HandleHerdMovementTransition();
    }


    /// <summary>
    /// Reacts to day-night phase changes and toggles the sheep's sleeping state.
    /// </summary>
    /// <param name="previousPhase">The previous day phase before the change.</param>
    /// <param name="currentPhase">The new current day phase after the change.</param>
    public void OnDayPhaseChanged(DayPhase previousPhase, DayPhase currentPhase)
    {
#if UNITY_EDITOR
        Debug.Log($"{name} reacting on changed DayPhase: {previousPhase} -> {currentPhase}");
#endif
        if (currentPhase == DayPhase.Night)
        {
            _isSleeping = true;
#if UNITY_EDITOR
            Debug.Log($"{name} => Sleeping | IsAsleep: {_isSleeping}");
#endif
        }
        else
        {
            _isSleeping = false;
#if UNITY_EDITOR
            Debug.Log($"{name} => Awake | IsAsleep: {_isSleeping}");
#endif
        }
    }

    /// <summary>
    /// Moves normal living sheep into regroup behavior when herd movement is active.
    /// </summary>
    private void HandleHerdMovementTransition()
    {
        if (FSM.IsCurrentState<RegroupState>())
            return;

        if (FSM.IsCurrentState<HerdMovingState>())
            return;

        if (!IsHerdMoving)
            return;

        if (IsAsleep)
            return;

        if (IsCommander)
            return;

        if (!IsAlive)
            return;

        FSM.ChangeState<RegroupState>();
    }

    /// <summary>
    /// Creates and registers all available sheep states once so they can be reused by the FSM.
    /// </summary>
    private void RegisterStates()
    {
        FSM.RegisterState(new IdleState(this, FSM));
        FSM.RegisterState(new PatrolState(this, FSM));
        FSM.RegisterState(new OnAlertState(this, FSM));
        FSM.RegisterState(new FleeState(this, FSM));
        FSM.RegisterState(new EatingState(this, FSM));
        FSM.RegisterState(new SleepingState(this, FSM));
        FSM.RegisterState(new RegroupState(this, FSM));
        FSM.RegisterState(new HerdMovingState(this, FSM));
        FSM.RegisterState(new FollowPlayerState(this, FSM));
        FSM.RegisterState(new DodgeState(this, FSM));
        FSM.RegisterState(new DeadState(this, FSM));
    }

    /// <summary>
    /// Handles sheep death by clearing tame state, disabling active behavior components,
    /// switching to the dead state, and moving the sheep to the graveyard position.
    /// </summary>
    private void TransisionDeadState()
    {
        Move.StopMoving();
        FSM.ChangeState<DeadState>();
    }

    public void HandleDeath()
    {
        _isTamed = false;
        _agent.enabled = false;
        Move.enabled = false;
        Sense.enabled = false;
        Hunger.enabled = false;

        transform.position = _graveyardPosition.position;
        _visualRoot.enabled = true;
    }

    /// <summary>
    /// Handles sheep spawning by re-enabling behavior components,
    /// restoring health, moving the sheep to a valid spawn position,
    /// and switching it back to idle behavior.
    /// </summary>
    public void HandleSpawn()
    {
        _visualRoot.enabled = false;

        transform.position = HerdManager.GetHerdAnchorPosition();

        _agent.enabled = true;
        Move.enabled = true;

        if (!TryWarpToSpawnPosition())
        {
            FSM.ChangeState<DeadState>();
            return;
        }

        Sense.enabled = true;
        Hunger.enabled = true;
        _visualRoot.enabled = true;

        Health.RestoreFullHealth();
        Hunger.RestoreHunger();

        FSM.ChangeState<IdleState>();
    }

    /// <summary>
    /// Tries to find a valid spawn position around the herd and warps the sheep to it.
    /// </summary>
    /// <returns>True if a valid spawn position was found and applied; otherwise false.</returns>
    private bool TryWarpToSpawnPosition()
    {
        const int MAX_TRIES = 100;

        for (int i = 0; i < MAX_TRIES; i++)
        {
            Vector3 spawnPos = HerdManager.GetRandomSpawnPosition();

            if (Move.TryGetValidTargetPosition(spawnPos, out Vector3 validPos))
            {
                _agent.Warp(validPos);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Applies starvation damage to the sheep when the hunger system raises a starvation event.
    /// </summary>
    /// <param name="damageType">The damage type used for starvation damage.</param>
    private void HandleStarving(DamageType damageType)
    {
        Health.TakeDamage(Hunger.StarvationDamage, damageType);
    }

    /// <summary>
    /// Reacts to non-starvation damage by switching the sheep into the flee state
    /// when a current threat is available.
    /// </summary>
    /// <param name="damage">The amount of damage received.</param>
    /// <param name="damageType">The type of damage received.</param>
    private void HandleDamage(int damage, DamageType damageType)
    {
        if (damageType == DamageType.Starvation)
            return;

        FleeState fleeState = FSM.GetState<FleeState>();

        if (fleeState == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name}: Cannot enter FleeState because it is not registered in the FSM.");
#endif
            return;
        }

        if (Sense.CurrentThreat == null)
            return;

        fleeState.SetThreat(Sense.CurrentThreat);
        FSM.ChangeState<FleeState>();
    }
}