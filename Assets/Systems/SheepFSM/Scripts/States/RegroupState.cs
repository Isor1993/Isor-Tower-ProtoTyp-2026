/*****************************************************************************
* Project : Isors Tower Prototype
* File    : RegroupState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the regroup behavior state of a sheep.
* Moves the sheep either to its herd formation position while the herd is
* moving, or to a random valid regroup position around the herd anchor.
* If no valid target can be found, the sheep returns to patrol behavior.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep moves back toward the herd or into its assigned herd formation position.
/// </summary>
public class RegroupState : SheepStateBase, IResumeTargetState
{
    private const int MAX_TRIES = 100;

    private Vector3 _targetPos;
    private Vector3 _resumeTarget;

    private bool _hasValidTarget;
    private bool _hasResumeTarget;
    private bool _isFormationTarget;

    public RegroupState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Enters the regroup state and searches for a valid regroup target.
    /// If the herd is moving, the sheep tries to move to its assigned formation position.
    /// Otherwise, it searches for a random valid regroup position around the herd anchor.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(RegroupState)}");
#endif
        Sheep.Animator.SetBool("Move", true);
        _hasValidTarget = false;
        _isFormationTarget = false;

        if (_hasResumeTarget)
        {
            _targetPos = _resumeTarget;
            _hasValidTarget = true;
            _isFormationTarget = Sheep.IsHerdMoving;
            _hasResumeTarget = false;

            Sheep.Move.MoveTo(_targetPos);
            return;
        }

        bool foundTarget = Sheep.IsHerdMoving
            ? TrySetFormationTarget()
            : TrySetRandomRegroupTarget();

        if (!foundTarget)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{Sheep.name}: Did not find a valid regroup position after {MAX_TRIES} tries!");
#endif
        }
    }

    /// <summary>
    /// Updates the regroup behavior.
    /// If no valid target exists, the sheep returns to patrol behavior.
    /// While the herd is moving, the state waits until the sheep reaches its
    /// formation position before switching to herd movement. Otherwise, the sheep
    /// returns to patrol once it reaches its regroup destination.
    /// </summary>
    public override void Tick()
    {
        if (Sheep.Sense.HasThreat)
        {
            FSM.ChangeState<OnAlertState>();
            return;
        }

        if (!_hasValidTarget)
        {
            FSM.ChangeState<PatrolState>();
            return;
        }

        if (Sheep.IsHerdMoving)
        {
            if (!_isFormationTarget)
            {
                if (!TrySetFormationTarget())
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"{Sheep.name}: Could not switch regroup target to formation position.");
#endif
                    return;
                }

                return;
            }

            if (Sheep.HerdManager.IsSheepInPosition(Sheep))
            {
                FSM.ChangeState<HerdMovingState>();
                return;
            }

            return;
        }

        if (Sheep.Move.HasReachedDestination())
        {
            FSM.ChangeState<PatrolState>();
            return;
        }
    }

    /// <summary>
    /// Exits the regroup state.
    /// </summary>
    public override void Exit()
    {
        Sheep.Animator.SetBool("Move", false);
    }

    /// <summary>
    /// Stores a movement target that should be resumed the next time the regroup state is entered.
    /// </summary>
    /// <param name="target">The movement target to resume.</param>
    public void ResumeTarget(Vector3 target)
    {
        _resumeTarget = target;
        _hasResumeTarget = true;
    }

    /// <summary>
    /// Tries to set the sheep's regroup target to its assigned herd formation position.
    /// </summary>
    /// <returns>True if a valid formation target was found and assigned; otherwise false.</returns>
    private bool TrySetFormationTarget()
    {
        _targetPos = Sheep.HerdManager.GetFormationPositionForSheep(Sheep);

        if (!Sheep.Move.TryGetValidTargetPosition(_targetPos, out Vector3 validPosition))
            return false;

        _targetPos = validPosition;
        _hasValidTarget = true;
        _isFormationTarget = true;

        Sheep.Move.MoveTo(_targetPos);
        return true;
    }

    /// <summary>
    /// Tries to find and assign a random valid regroup target around the herd anchor.
    /// </summary>
    /// <returns>True if a valid regroup target was found and assigned; otherwise false.</returns>
    private bool TrySetRandomRegroupTarget()
    {
        for (int i = 0; i < MAX_TRIES; i++)
        {
            _targetPos = Sheep.HerdManager.GetRandomRegroupPosition();

            if (Sheep.Move.TryGetValidTargetPosition(_targetPos, out Vector3 validPos))
            {
                _targetPos = validPos;
                _hasValidTarget = true;
                _isFormationTarget = false;

                Sheep.Move.MoveTo(_targetPos);
                return true;
            }
        }
        return false;
    }
}