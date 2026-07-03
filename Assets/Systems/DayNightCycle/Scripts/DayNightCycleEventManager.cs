/*****************************************************************************
* Project : Isors Tower Prototype
* File    : DayNightCycleEventManager.cs
* Date    : xx.xx.2026
* Author  : Eric Rosenberg
*
* Description :
* Acts as a central relay between the DayNightCycle and all registered
* IDayNightListener subscribers. The manager listens to phase changes from the
* day-night cycle and forwards them to all active listeners while removing null
* references from the subscriber list.
*
* History :
* xx.xx.2026 ER Created
******************************************************************************/
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages subscriptions for day-night listeners and forwards phase change events to them.
/// </summary>
public class DayNightCycleEventManager : MonoBehaviour
{

    private List<IDayNightListener> _subscribers = new List<IDayNightListener>();
    private DayNightCycle _dayNightCycle;

    private void Awake()
    {
        _dayNightCycle = FindAnyObjectByType<DayNightCycle>();

        if (_dayNightCycle == null)
        {
#if (UNITY_EDITOR)
            Debug.LogError("DayNightCycle instance not found.");
#endif
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        if (_dayNightCycle == null)
        {
            return;
        }

        _dayNightCycle.OnDayPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        if (_dayNightCycle == null)
        {
            return;
        }

        _dayNightCycle.OnDayPhaseChanged -= HandlePhaseChanged;
    }


    /// <summary>
    /// Forwards a day phase change to all registered listeners and removes missing references.
    /// </summary>
    /// <param name="previous">The previous day phase before the change.</param>
    /// <param name="current">The new current day phase after the change.</param>
    private void HandlePhaseChanged(DayPhase previous, DayPhase current)
    {
        for (int i = _subscribers.Count - 1; i >= 0; i--)
        {
            if (_subscribers[i] == null)
            {
                _subscribers.RemoveAt(i);
                continue;
            }

            _subscribers[i].OnDayPhaseChanged(previous, current);

        }
    }

    /// <summary>
    /// Registers a listener for future day phase change notifications.
    /// </summary>
    /// <param name="listener">The listener that should receive day phase change events.</param>
    public void Subscribe(IDayNightListener listener)
    {
        if (listener == null)
            return;

        if (!_subscribers.Contains(listener))
        {
            _subscribers.Add(listener);
        }

        if (_dayNightCycle == null)
            return;

        if (_dayNightCycle.CurrentDayPhase == DayPhase.None)
            return;

        listener.OnDayPhaseChanged(_dayNightCycle.CurrentDayPhase, _dayNightCycle.CurrentDayPhase);
    }

    /// <summary>
    /// Removes a listener from the day phase change notification list.
    /// </summary>
    /// <param name="listener">The listener that should no longer receive day phase change events.</param>
    public void Unsubscribe(IDayNightListener listener)
    {
        if (listener == null || !_subscribers.Contains(listener))
        {
            return;
        }

        _subscribers.Remove(listener);
    }
}