/*****************************************************************************
* Project : Isors Tower Prototype
* File    : IdleState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the idle behavior state of a sheep.
* Keeps the sheep waiting for a configured idle duration while continuously
* checking for threats, nearby players, hunger, and the idle timeout.
* Transitions to alert, eating, or regroup behavior when the corresponding
* condition is met.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep remains idle until danger, hunger, player presence,
/// or the idle timer causes a transition to another state.
/// </summary>
public class IdleState : SheepStateBase
{
    private readonly Timer _timer = new Timer();

    public IdleState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Enters the idle state and resets the internal idle timer.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(IdleState)}");
#endif
        Sheep.Animator.SetBool("PlayIdle", true);
        _timer.Reset();
    }

    /// <summary>
    /// Updates the idle timer and checks for conditions that should transition
    /// the sheep into alert, eating, or regroup behavior.
    /// </summary>
    public override void Tick()
    {
        _timer.Tick(Time.deltaTime);

        if(Sheep.IsAsleep)
        {
            FSM.ChangeState<SleepingState>();
            return;
        }
        if (Sheep.Sense.HasThreat)
        {
            FSM.ChangeState<OnAlertState>();
            return;
        }
        if (Sheep.Sense.HasPlayerInRange)
        {
            FSM.ChangeState<OnAlertState>();
            return;
        }
        if (Sheep.Hunger.IsHungry)
        {
            FSM.ChangeState<EatingState>();
            return;
        }
        if (_timer.IsFinished(Settings.IdleTime))
        {
            FSM.ChangeState<RegroupState>();
            return;
        }
    }

    /// <summary>
    /// Exits the idle state.
    /// </summary>
    public override void Exit()
    {
        Sheep.Animator.SetBool("PlayIdle", false);
    }
}