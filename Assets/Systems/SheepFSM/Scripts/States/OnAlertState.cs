/*****************************************************************************
* Project : Isors Tower Prototype
* File    : OnAlertState.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Represents the alert behavior state of a sheep.
* Uses an alert timer to keep the sheep alert for a limited duration and a
* reaction timer to periodically evaluate nearby threats and player-related
* conditions. Depending on the situation, the sheep can transition into flee,
* follow-player, or patrol behavior.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using UnityEngine;

/// <summary>
/// State in which the sheep observes its surroundings and reacts to threats,
/// nearby players, or alert timeout conditions.
/// </summary>
public class OnAlertState : SheepStateBase
{
    private readonly Timer _alertTimer = new Timer();
    private readonly Timer _reactionTimer = new Timer();

    public OnAlertState(Sheep sheep, SheepFSM fsm) : base(sheep, fsm)
    {
    }

    /// <summary>
    /// Enters the alert state and resets both the alert timer and reaction timer.
    /// </summary>
    public override void Enter()
    {
#if UNITY_EDITOR
        Debug.Log($"{GetType().Name}:{Sheep.gameObject.name}: Change state => {nameof(OnAlertState)}");
#endif

        Sheep.Animator.SetBool("PlayIdle", true);
        _alertTimer.Reset();
        _reactionTimer.Reset();
    }

    /// <summary>
    /// Updates alert and reaction timers.
    /// When the reaction interval is finished, the sheep checks for threats,
    /// tamed-player follow conditions, and whether the player is too close.
    /// If the alert duration ends without a stronger reaction, the sheep
    /// transitions into patrol behavior.
    /// </summary>
    public override void Tick()
    {
        _alertTimer.Tick(Time.deltaTime);
        _reactionTimer.Tick(Time.deltaTime);

        if (_reactionTimer.IsFinished(Settings.ReactionTime))
        {
            _reactionTimer.Reset();

            if (Sheep.Sense.HasThreat)
            {
                EnterFleeState(Sheep.Sense.CurrentThreat);
                return;
            }

            if (Sheep.Sense.HasPlayerInRange && Sheep.IsTamed)
            {
                FSM.ChangeState<FollowPlayerState>();
                return;
            }

            if (Sheep.Sense.IsPlayerTooClose)
            {
                EnterFleeState(Sheep.Sense.CurrentPlayer);
                return;
            }
        }

        if (!_alertTimer.IsFinished(Settings.AlertTime))
            return;

        FSM.ChangeState<PatrolState>();
    }

    /// <summary>
    /// Exits the alert state.
    /// </summary>
    public override void Exit()
    {
        Sheep.Animator.SetBool("PlayIdle", false);
    }

    /// <summary>
    /// Configures the flee state with the given threat and changes into it.
    /// If no valid threat is available or the flee state is not registered,
    /// the transition is skipped.
    /// </summary>
    /// <param name="threat">The transform the sheep should flee from.</param>
    private void EnterFleeState(Transform threat)
    {
        FleeState fleeState = FSM.GetState<FleeState>();

        if (fleeState == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"{Sheep.name}: Cannot enter FleeState because it is not registered in the FSM.");
#endif
            return;
        }

        fleeState.SetThreat(threat);
        FSM.ChangeState<FleeState>();
    }
}