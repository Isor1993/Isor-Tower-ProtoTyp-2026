/*****************************************************************************
* Project : Isors Tower Prototype
* File    : DayNightCycle.cs
* Date    : xx.xx.2026
* Author  : Eric Rosenberg
*
* Description :
* Calculates the current phase of the day based on the active IngameTime
* instance. The script exposes the normalized day progress for visual systems
* and raises an event whenever the current day phase changes.
*
* History :
* xx.xx.2026 ER Created
******************************************************************************/
using System;
using UnityEngine;

/// <summary>
/// Represents the main time phases used by the day-night cycle.
/// </summary>
public enum DayPhase
{
    None,
    Morning,
    Afternoon,
    Evening,
    Night
}
/// <summary>
/// Tracks the current day phase and normalized day progress based on the global ingame time.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Phase Settings")]
    [Tooltip("The ingame hour at which the morning phase starts."),Range(1,24)]
    [SerializeField] private int _morningStartHour = 6;
    [Tooltip("The ingame hour at which the afternoon phase starts."), Range(1, 24)]
    [SerializeField] private int _afternoonStartHour = 12;
    [Tooltip("The ingame hour at which the evening phase starts."), Range(1, 24)]
    [SerializeField] private int _eveningStartHour = 18;
    [Tooltip("The ingame hour at which the night phase starts."), Range(1, 24)]
    [SerializeField] private int _nightStartHour = 24;

    [Header("Debug")]
    [Tooltip("The currently active day phase at runtime.")]
    [SerializeField] private DayPhase _currentPhase=DayPhase.None;
    [Tooltip("The current progress through the active day, normalized from 0 to 1.")]
    [SerializeField] private float _dayProgress;

    /// <summary>
    /// Gets the currently active day phase.
    /// </summary>
    public DayPhase CurrentDayPhase => _currentPhase;

    /// <summary>
    /// Gets the current normalized day progress from 0 to 1.
    /// </summary>
    public float DayProgress => _dayProgress;

    /// <summary>
    /// Raised whenever the day phase changes. Provides the previous and the new phase.
    /// </summary>
    public event Action<DayPhase, DayPhase> OnDayPhaseChanged;

    private DayPhase _previousPhase= DayPhase.None;
    private IngameTime _ingameTime;

    /// <summary>
    /// Initializes the day-night cycle by connecting to the global ingame time instance.
    /// </summary>
    private void Awake()
    {
        _ingameTime = IngameTime.Instance;
        if(_ingameTime==null)
        {
            Debug.LogError($"{IngameTime.Instance} is missing!");
            enabled = false;
            return;
        }

        UpdateDayProgress();
        _currentPhase=CalculateCurrentPhase();
        _previousPhase = _currentPhase;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        UpdateDayProgress();
        UpdateCurrentPhase();
    }

    /// <summary>
    /// Calculates the normalized progress through the current day.
    /// </summary>
    private void UpdateDayProgress()
    {
        _dayProgress = (float)_ingameTime.CurrentTotalSeconds / _ingameTime.SecondsPerDay;
    }

    /// <summary>
    /// Calculates the current day phase from the current ingame hour.
    /// </summary>
    /// <returns>The day phase that matches the current ingame hour.</returns>
    private DayPhase CalculateCurrentPhase()
    {
        int currentHour = _ingameTime.Hours;

       

        if (currentHour >= _morningStartHour &&
            currentHour < _afternoonStartHour)
        {
            return DayPhase.Morning;
        }
        if (currentHour >= _afternoonStartHour &&
                 currentHour < _eveningStartHour)
        {
            return DayPhase.Afternoon;
        }
        if (currentHour >= _eveningStartHour &&
                 currentHour < _nightStartHour)
        {
            return DayPhase.Evening;
        }        

        return DayPhase.Night;
    }

    /// <summary>
    /// Checks for day phase changes and notifies listeners when a new phase starts.
    /// </summary>
    private void UpdateCurrentPhase()
    {
        DayPhase newPhase=CalculateCurrentPhase();

        if(_currentPhase==newPhase)
        {
            return;
        }
        _previousPhase = _currentPhase;
        _currentPhase = newPhase;

        OnDayPhaseChanged?.Invoke(_previousPhase, _currentPhase);

#if UNITY_EDITOR
        Debug.Log($"Phase Changed: {_previousPhase} -> {_currentPhase}");
#endif
    }
}