/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SleepingState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the sleeping behavior state of a sheep.
* Stops the sheep's movement when entering the state and keeps it inactive
* while the sheep is marked as asleep. Periodically checks whether the sheep
* should wake up and returns to patrol behavior once the sleeping condition ends.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep stops moving and remains asleep until the sleep condition ends.
/// </summary>
public class SleepingState : SheepStateBase
{
    private readonly Timer _updateTimer = new Timer();
    private const float UPDATE_TIME = 1f;

    public SleepingState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Enters the sleeping state, resets the update timer, and stops the sheep's movement.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(SleepingState)}");
#endif
        Sheep.Animator.SetBool("Sleep", true);
        _updateTimer.Reset();
        Sheep.Move.StopMoving();
        Sheep.Hunger.IsHungerActive = false;
    }

    /// <summary>
    /// Periodically checks whether the sheep is still asleep.
    /// If the sheep is no longer asleep, it transitions back to patrol behavior.
    /// </summary>
    public override void Tick()
    {
        _updateTimer.Tick(Time.deltaTime);

        if (!_updateTimer.IsFinished(UPDATE_TIME))
            return;
        _updateTimer.Reset();

        if (!Sheep.IsAsleep)
        {
            FSM.ChangeState<PatrolState>();
            return;
        }
    }

    /// <summary>
    /// Exits the sleeping state.
    /// </summary>
    public override void Exit()
    {
        Sheep.Hunger.IsHungerActive = true;
        Sheep.Animator.SetBool("Sleep", false);
    }
}