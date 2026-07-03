/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SheepHunger.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Manages the hunger system for a sheep entity.
* Loads hunger-related values from SheepSettings, increases hunger over time,
* allows hunger to be reduced through eating, and raises a starvation event
* when the sheep reaches the maximum hunger value.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using System;
using UnityEngine;

/// <summary>
/// Controls the hunger values of a sheep and notifies other systems when the sheep starts starving.
/// </summary>
public class SheepHunger : MonoBehaviour
{
    [Tooltip("ScriptableObject that contains the base hunger configuration for this sheep.")]
    [SerializeField] private SheepSettings _settings;

    [Tooltip("Maximum hunger value the sheep can reach before it is considered starving.")]
    [Range(1, 100)]
    [SerializeField] private int _maxHunger;

    [Tooltip("Current hunger value of the sheep. Higher values mean the sheep is more hungry.")]
    [Range(0, 100)]
    [SerializeField] private int _currentHunger;

    [Tooltip("Time interval in seconds between two hunger ticks.")]
    [Range(1f, 100f)]
    [SerializeField] private float _hungerTickInterval;

    [Tooltip("Amount of hunger added each time a hunger tick is applied.")]
    [Range(1, 10)]
    [SerializeField] private int _hungerTick;

    [Tooltip("Amount of hunger removed each time the sheep eats.")]
    [Range(1, 20)]
    [SerializeField] private int _eatTick;

    [Tooltip("Hunger value at which the sheep is considered hungry.")]
    [Range(1, 100)]
    [SerializeField] private int _hungerThreshold;

    [Tooltip("Damage amount applied when the sheep is starving.")]
    [Range(1, 100)]
    [SerializeField] private int _starvationDamage;

    private float _hungerTimer;
    private bool _isEating = false;
    private bool _isHungerActive = true;

    /// <summary>
    /// Raised when the sheep reaches starvation and should receive starvation damage.
    /// Provides the damage type that caused the starvation event.
    /// </summary>
    public event Action<DamageType> OnStarving;

    /// <summary>
    /// Indicates whether the sheep is currently hungry.
    /// Returns true when the current hunger value is greater than the hunger threshold.
    /// </summary>
    public bool IsHungry => _currentHunger > _hungerThreshold;

    /// <summary>
    /// Indicates whether the sheep is currently starving.
    /// Returns true when the current hunger value has reached or exceeded the maximum hunger value.
    /// </summary>
    public bool IsStarving => _currentHunger >= _maxHunger;

    /// <summary>
    /// Indicates whether the sheep is currently full.
    /// Returns true when the current hunger value is zero or below.
    /// </summary>
    public bool IsFull => _currentHunger <= 0;

    /// <summary>
    /// Gets the amount of damage caused by starvation.
    /// </summary>
    public int StarvationDamage => _starvationDamage;

    /// <summary>
    /// Indicates whether the sheep is currently reducing hunger through eating.
    /// </summary>
    public bool IsEating { get => _isEating; set => _isEating = value; }

    /// <summary>
    /// Indicates whether hunger increases over time.
    /// Can be disabled while the sheep is sleeping or inactive.
    /// </summary>
    public bool IsHungerActive { get => _isHungerActive; set => _isHungerActive = value; }

    private void Awake()
    {
        SetBaseValues();
    }

    private void OnEnable()
    {
        RestoreHunger();
    }

    /// <summary>
    /// Loads the initial hunger configuration from the SheepSettings ScriptableObject.
    /// Sets maximum hunger, current hunger, tick interval, hunger tick amount,
    /// eat tick amount, hunger threshold, and starvation damage.
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
        _maxHunger = _settings.MaxHunger;
        _currentHunger = Mathf.Clamp(_currentHunger, 0, _maxHunger);
        _hungerTickInterval = _settings.HungerTickInterval;
        _hungerTick = _settings.HungerTick;
        _eatTick = _settings.EatTickRate;
        _hungerThreshold = _settings.HungerThreshold;
        _starvationDamage = _settings.StarvationDamage;
    }

    private void Update()
    {
        _hungerTimer += Time.deltaTime;

        if (_hungerTimer < _hungerTickInterval)
            return;

        _hungerTimer = 0f;

        if (IsEating)
        {
            Eat();
            return;
        }

        if (!_isHungerActive)
            return;

        ApplyHungerTick();

        if (IsStarving)
        {
            OnStarving?.Invoke(DamageType.Starvation);
        }
    }

    /// <summary>
    /// Reduces the current hunger value by the configured eat amount.
    /// </summary>
    private void Eat()
    {
        _currentHunger -= _eatTick;
        _currentHunger = Mathf.Clamp(_currentHunger, 0, _maxHunger);
    }

    /// <summary>
    /// Increases the current hunger value by the configured hunger tick amount.
    /// </summary>
    private void ApplyHungerTick()
    {
        _currentHunger += _hungerTick;
        _currentHunger = Mathf.Clamp(_currentHunger, 0, _maxHunger);
    }

    /// <summary>
    /// Resets the current hunger value and hunger timer, making the sheep fully fed.
    /// </summary>
    public void RestoreHunger()
    {
        _currentHunger = 0;
        _hungerTimer = 0f;
    }
}