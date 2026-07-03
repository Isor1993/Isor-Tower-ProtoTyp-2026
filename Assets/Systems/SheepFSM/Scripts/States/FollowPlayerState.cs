/*****************************************************************************
* Project : Isors Tower Prototype
* File    : FollowPlayerState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the follow-player behavior state of a sheep.
* Allows a tamed sheep to follow the currently detected player and, if the
* sheep is the commander, informs the herd manager that the herd is currently
* moving as a group.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which a tamed sheep follows the detected player.
/// Commander sheep also notify the herd manager when herd movement starts or stops.
/// </summary>
public class FollowPlayerState : SheepStateBase
{
    private bool _isHerdMovingActivated;
    private float _followDistanceAgent = 0f;
    private float _defaultValue = 2f;

    public FollowPlayerState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Enters the follow-player state and resets the herd movement activation flag.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(FollowPlayerState)}");
#endif
        Sheep.Animator.SetBool("Move", true);
        _isHerdMovingActivated = false;
        Sheep.Move.SetAgentStopDistance(_followDistanceAgent);
    }

    /// <summary>
    /// Updates the follow-player behavior.
    /// If the sheep is no longer tamed or the player reference is missing,
    /// the sheep returns to regroup behavior. Otherwise, it follows behind
    /// the detected player while the player remains within sensing range.
    /// </summary>
    public override void Tick()
    {
        if (Sheep.Move.HasReachedDestination())
        {
            Sheep.Animator.SetBool("Move", false);
            Sheep.Move.RotateTowardsTarget(Sheep.Sense.CurrentPlayer);
        }
        if (!Sheep.IsTamed)
        {
            FSM.ChangeState<RegroupState>();
            return;
        }
        if (Sheep.Sense.CurrentPlayer == null)
        {
            FSM.ChangeState<RegroupState>();
            return;
        }

        if (Sheep.Dodge.ShouldDodge)
        {
            DodgeState dodgeState = FSM.GetState<DodgeState>();

            if (dodgeState == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{Sheep.name}: Cannot enter DodgeState because it is not registered in the FSM.");
#endif
                return;
            }

            dodgeState.SetReturnState(FSM.CurrentState);
            FSM.ChangeState<DodgeState>();
            return;
        }

        if (!Sheep.Sense.HasPlayerInRange)
            return;

        Sheep.Move.FollowBehind(Sheep.Sense.CurrentPlayer);
        Sheep.Animator.SetBool("Move", true);

        if (Sheep.Move.HasReachedDestination() && !_isHerdMovingActivated)
        {
            Sheep.Animator.SetBool("Move", false);
            if (Sheep.IsCommander)
            {
               
                Sheep.HerdManager.SetAllSheepHerdMoving(true);
                _isHerdMovingActivated = true;
            }
        }
    }

    /// <summary>
    /// Exits the follow-player state and clears herd movement if this sheep is the commander.
    /// </summary>
    public override void Exit()
    {
        Sheep.Move.SetAgentStopDistance(_defaultValue);
        if (Sheep.IsCommander)
        {
            Sheep.HerdManager.SetAllSheepHerdMoving(false);
            _isHerdMovingActivated = false;
        }
        Sheep.Animator.SetBool("Move", false);
    }
}