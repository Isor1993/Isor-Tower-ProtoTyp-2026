/*****************************************************************************
* Project : Isors Tower Prototype
* File    : FleeState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the flee behavior state of a sheep.
* Moves the sheep away from a detected threat, periodically recalculates a new
* flee target position, and returns the sheep to regroup behavior when the
* threat is gone or the flee destination has been reached.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep flees from a configured threat transform and updates
/// its flee destination at configured intervals.
/// </summary>
public class FleeState : SheepStateBase
{
    private readonly Timer _updateTimer = new Timer();

    private Transform _threat;

    public FleeState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Sets the threat transform this sheep should flee from.
    /// Must be called before entering the flee state.
    /// </summary>
    /// <param name="threat">The threat transform the sheep should flee from.</param>
    public void SetThreat(Transform threat)
    {
        _threat = threat;
    }

    /// <summary>
    /// Enters the flee state, switches the sheep to flee movement settings,
    /// and starts moving away from the configured threat position.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(FleeState)}");
#endif
        Sheep.Animator.SetBool("Run", true);
        _updateTimer.Reset();

        Sheep.Move.SetFleeMovement();

        if (_threat == null)
        {
            FSM.ChangeState<RegroupState>();
            return;
        }

        Sheep.Move.FleeFrom(_threat.position);
      
    }

    /// <summary>
    /// Updates the flee behavior.
    /// If the threat is missing or no longer detected after reaching the destination,
    /// the sheep returns to regroup behavior. Otherwise, the flee target is recalculated
    /// at the configured update interval after reaching its current flee destination.
    /// </summary>
    public override void Tick()
    {
        if (_threat == null)
        {
            FSM.ChangeState<RegroupState>();
            return;
        }

        if (Sheep.Move.HasReachedDestination() && !Sheep.Sense.HasThreat)
        {
            FSM.ChangeState<RegroupState>();
            return;
        }

        _updateTimer.Tick(Time.deltaTime);

        if (_updateTimer.IsFinished(Settings.UpdateTickNewPosition) &&
            Sheep.Move.HasReachedDestination())
        {
            _updateTimer.Reset();
            Sheep.Move.FleeFrom(_threat.position);
        }
    }

    /// <summary>
    /// Exits the flee state and restores the sheep's normal walking movement settings.
    /// </summary>
    public override void Exit()
    {
        Sheep.Move.SetWalkMovement();
        _threat = null;
        Sheep.Animator.SetBool("Run", false);
    }
}