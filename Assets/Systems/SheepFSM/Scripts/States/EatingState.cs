/*****************************************************************************
* Project : Isors Tower Prototype
* File    : EatingState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the eating behavior state of a sheep.
* Marks the sheep as eating through the hunger system and keeps it in this
* state until it is full or a threat is detected. The sheep can transition
* into alert behavior when danger appears or back to patrol behavior after
* eating is complete.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep eats until it is full or interrupted by a detected threat.
/// </summary>
public class EatingState : SheepStateBase
{
    public EatingState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Enters the eating state and marks the sheep as currently eating.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(EatingState)}");
#endif
        Sheep.Animator.SetBool("Eat", true);
        Sheep.Hunger.IsEating = true;
    }

    /// <summary>
    /// Updates the eating behavior.
    /// If a threat is detected, the sheep switches to alert behavior.
    /// If the sheep becomes full, it returns to patrol behavior.
    /// </summary>
    public override void Tick()
    {
        if (Sheep.Sense.HasThreat)
        {
            FSM.ChangeState<OnAlertState>();
            return;
        }
        if(Sheep.IsAsleep)
        {
            FSM.ChangeState<SleepingState>();
            return;
        }
        if (Sheep.Hunger.IsFull)
        {
            FSM.ChangeState<PatrolState>();
            return;
        }
    }

    /// <summary>
    /// Exits the eating state and clears the eating flag on the hunger system.
    /// </summary>
    public override void Exit()
    {

        Sheep.Hunger.IsEating = false;
        Sheep.Animator.SetBool("Eat", false);
    }
}