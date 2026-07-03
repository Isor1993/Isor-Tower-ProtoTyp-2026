/*****************************************************************************
* Project : Isors Tower Prototype
* File    : IDayNightListener.cs
* Date    : xx.xx.2026
* Author  : Eric Rosenberg
*
* Description :
* Defines the contract for objects that need to react to day phase changes.
* Classes implementing this interface can subscribe to the day-night event
* manager and receive the previous and current phase whenever the world time
* enters a new phase.
*
* History :
* xx.xx.2026 ER Created
******************************************************************************/
using UnityEngine;

/// <summary>
///Provides a callback for objects that react to changes in the current day phase.
/// </summary>
public interface IDayNightListener
{
    /// <summary>
    /// Called when the day-night cycle changes from one phase to another.
    /// </summary>
    /// <param name="previousPhase">The phase that was active before the change.</param>
    /// <param name="currentPhase">The phase that is active after the change.</param>
    public void OnDayPhaseChanged(DayPhase previousPhase, DayPhase currentPhase);
}