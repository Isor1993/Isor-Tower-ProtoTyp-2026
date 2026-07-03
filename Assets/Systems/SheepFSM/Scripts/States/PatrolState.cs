/*****************************************************************************
* Project : Isors Tower Prototype
* File    : PatrolState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the patrol behavior state of a sheep.
* Moves the sheep to a random valid patrol position around the herd anchor.
* The state reacts to threats, dodge situations, player proximity, sleep,
* hunger, and herd movement by transitioning into the corresponding behavior.
* Once the patrol destination is reached, the sheep returns to idle behavior.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep moves toward a random patrol position around the herd area.
/// </summary>
public class PatrolState : SheepStateBase, IResumeTargetState
{
    private bool _hasResumeTarget;
    private Vector3 _resumeTarget;

    public PatrolState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Enters the patrol state and assigns either a resumed movement target
    /// or a new random patrol target.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(PatrolState)}");
#endif
        Sheep.Animator.SetBool("Move", true);

        if (_hasResumeTarget)
        {
            Sheep.Move.MoveTo(_resumeTarget);
            _hasResumeTarget = false;
            return;
        }
        SetNewPatrolTarget();
    }

    /// <summary>
    /// Updates the patrol behavior and checks for conditions that should interrupt
    /// patrol movement, such as threats, dodge situations, player proximity,
    /// sleep, hunger, or herd movement. Returns to idle once the destination is reached.
    /// </summary>
    public override void Tick()
    {
        if (Sheep.Sense.HasThreat)
        {
            FSM.ChangeState<OnAlertState>();
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

        if (Sheep.Sense.IsPlayerInTameRange && Sheep.IsTamed)
        {
            FSM.ChangeState<FollowPlayerState>();
            return;
        }

        if (Sheep.IsAsleep && Sheep.Move.HasReachedDestination())
        {
            FSM.ChangeState<SleepingState>();
            return;
        }

        if (Sheep.Hunger.IsHungry && Sheep.Move.HasReachedDestination())
        {
            FSM.ChangeState<EatingState>();
            return;
        }

        if (Sheep.IsHerdMoving)
        {
            FSM.ChangeState<RegroupState>();
            return;
        }

        if (Sheep.Sense.IsPlayerTooClose)
        {
            FleeState fleeState = FSM.GetState<FleeState>();
            if (fleeState == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{Sheep.name}: Cannot enter FleeState because it is not registered in the FSM.");
#endif
                return;
            }
            fleeState.SetThreat(Sheep.Sense.CurrentPlayer);
            FSM.ChangeState<FleeState>();
            return;
        }

        if (Sheep.Move.HasReachedDestination())
        {           
            FSM.ChangeState<IdleState>();
            return;
        }
    }

    /// <summary>
    /// Exits the patrol state.
    /// </summary>
    public override void Exit()
    {
        Sheep.Animator.SetBool("Move", false);
    }

    /// <summary>
    /// Requests a random patrol position from the herd manager, validates it on the NavMesh,
    /// and moves the sheep toward the resulting valid target position.
    /// </summary>
    private void SetNewPatrolTarget()
    {
        Vector3 newPos = Sheep.HerdManager.GetRandomPatrolPosition();

        if (!Sheep.Move.TryGetValidTargetPosition(newPos, out Vector3 validPos))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{Sheep.name}: Could not find valid patrol target.");
#endif
            return;
        }

        Sheep.Move.MoveTo(validPos);

    }

    /// <summary>
    /// Stores a movement target that should be resumed the next time the patrol state is entered.
    /// </summary>
    /// <param name="target">The movement target to resume.</param>
    public void ResumeTarget(Vector3 target)
    {
        _resumeTarget = target;
        _hasResumeTarget = true;


    }
}