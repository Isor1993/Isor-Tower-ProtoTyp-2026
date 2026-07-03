/*****************************************************************************
* Project : Isors Tower Prototype
* File    : HerdMovingState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the herd movement behavior state of a sheep.
* Allows the commander sheep to follow the player while normal sheep move
* into their assigned herd formation positions. The state keeps the herd
* moving as a group until herd movement ends or required conditions are lost.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep participates in active herd movement.
/// The commander follows the player, while normal sheep follow their assigned formation positions.
/// </summary>
public class HerdMovingState : SheepStateBase
{
    public HerdMovingState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Enters the herd movement state.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(HerdMovingState)}");
#endif
        Sheep.Animator.SetBool("Move", true);
        if (!Sheep.IsCommander)
        {
            Sheep.Move.SetHerdMovement();
        }
    }

    /// <summary>
    /// Updates herd movement behavior.
    /// The commander continues following the player while it is tamed and has a valid player target.
    /// Normal sheep continue moving toward their assigned formation positions while herd movement is active.
    /// If required conditions are lost, the sheep returns to patrol behavior.
    /// </summary>
    public override void Tick()
    {
        if (!Sheep.IsTamed && Sheep.IsCommander)
        {
            FSM.ChangeState<PatrolState>();
            return;
        }
        if (!Sheep.IsHerdMoving && !Sheep.IsCommander)
        {
            FSM.ChangeState<PatrolState>();
            return;
        }
        if (Sheep.IsCommander)
        {
            if (Sheep.Sense.CurrentPlayer == null)
            {
                FSM.ChangeState<PatrolState>();
                return;
            }

            Sheep.Move.FollowBehind(Sheep.Sense.CurrentPlayer);
            return;
        }
        Vector3 newPos = Sheep.HerdManager.GetFormationPositionForSheep(Sheep);

        if (!Sheep.Move.TryGetValidTargetPosition(newPos, out Vector3 validPos))
            return;

        Sheep.Move.MoveTo(validPos);
    }

    /// <summary>
    /// Exits the herd movement state and restores normal walking movement settings
    /// for non-commander sheep.
    /// </summary>
    public override void Exit()
    {
        if (!Sheep.IsCommander)
            Sheep.Move.SetWalkMovement();
        Sheep.Animator.SetBool("Move", false);
    }
}