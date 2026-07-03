/*****************************************************************************
* Project : Isors Tower Prototype
* File    : DodgeState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the dodge behavior state of a sheep.
* Temporarily interrupts the current movement state to dodge an obstacle,
* stores the previous movement target, and returns the sheep to its previous
* state once the dodge movement is finished.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep temporarily dodges an obstacle and then returns
/// to the state it came from.
/// </summary>
public class DodgeState : SheepStateBase
{
    private Vector3 _previousTarget;
    private SheepStateBase _returnState;
    private bool _hasReturned;

    public DodgeState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Sets the state that should be resumed after the dodge movement is finished.
    /// Must be called before entering the dodge state.
    /// </summary>
    /// <param name="returnState">The state to return to after dodging.</param>
    public void SetReturnState(SheepStateBase returnState)
    {
        _returnState = returnState;
    }

    /// <summary>
    /// Enters the dodge state and tries to start the dodge movement.
    /// If no return state exists or dodging cannot be started, the sheep falls
    /// back to a safe state.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(DodgeState)}");
#endif
        Sheep.Animator.SetBool("Move", true);
        _hasReturned = false;

        if (_returnState == null)
        {
            FSM.ChangeState<IdleState>();
            return;
        }

        if (!Sheep.Dodge.TryStartDodge(out _previousTarget))
        {
#if UNITY_EDITOR
            Debug.Log($"{Sheep.name}: Dodge not possible. Returning to {_returnState.GetType().Name}.");
#endif
            ReturnToPreviousState();
        }
    }

    /// <summary>
    /// Updates the dodge state.
    /// The sheep stays in this state while the dodge behavior is active and
    /// returns to its previous state once dodging is complete.
    /// </summary>
    public override void Tick()
    {
        if (Sheep.Dodge.IsDodging)
            return;

        ReturnToPreviousState();
    }

    /// <summary>
    /// Exits the dodge state and clears temporary dodge data so the pooled state
    /// can be reused safely later.
    /// </summary>
    public override void Exit()
    {
        _returnState = null;
        _previousTarget = Vector3.zero;
        _hasReturned = false;
        Sheep.Animator.SetBool("Move", false);
    }

    /// <summary>
    /// Returns the sheep to its previous state.
    /// If the previous state supports target resuming, the last movement target
    /// is passed back before the state transition happens.
    /// </summary>
    private void ReturnToPreviousState()
    {
        if (_hasReturned)
            return;

        _hasReturned = true;

        if (_returnState == null)
        {
            FSM.ChangeState<IdleState>();
            return;
        }

        if (_returnState is IResumeTargetState resumeState)
        {
            resumeState.ResumeTarget(_previousTarget);
        }

        ChangeBackToReturnState();
    }

    /// <summary>
    /// Changes back to the stored return state.
    /// Falls back to idle behavior if the stored state type is not explicitly supported.
    /// </summary>
    private void ChangeBackToReturnState()
    {
        if (_returnState is PatrolState)
        {
            FSM.ChangeState<PatrolState>();
            return;
        }

        if (_returnState is RegroupState)
        {
            FSM.ChangeState<RegroupState>();
            return;
        }

        if (_returnState is FollowPlayerState)
        {
            FSM.ChangeState<FollowPlayerState>();
            return;
        }

        FSM.ChangeState<IdleState>();
    }
}