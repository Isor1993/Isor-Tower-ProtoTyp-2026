/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SheepFSM.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Manages the finite state machine for a sheep entity.
* Stores all registered sheep states, keeps track of the currently active state,
* handles state transitions by calling exit and enter lifecycle methods,
* and updates the active state every frame.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

using System;
using System.Collections.Generic;

/// <summary>
/// Controls the current state of a sheep and manages transitions between sheep behavior states.
/// Stores all available states once and reuses them during runtime.
/// </summary>
public class SheepFSM
{
    private readonly Dictionary<Type, SheepStateBase> _states = new Dictionary<Type, SheepStateBase>();

    private SheepStateBase _currentState;

    /// <summary>
    /// Gets the currently active sheep state.
    /// </summary>
    public SheepStateBase CurrentState => _currentState;

    /// <summary>
    /// Gets the name of the currently active state.
    /// Returns "None" if no state is active.
    /// </summary>
    public string CurrentStateName
    {
        get
        {
            if (_currentState == null)
                return "None";

            return _currentState.GetType().Name;
        }
    }

    /// <summary>
    /// Registers a state instance so it can be reused by the FSM.
    /// </summary>
    /// <param name="state">The state instance to register.</param>
    public void RegisterState(SheepStateBase state)
    {
        if (state == null)
            return;

        Type stateType = state.GetType();

        if (_states.ContainsKey(stateType))
            return;

        _states.Add(stateType, state);
    }

    /// <summary>
    /// Gets a registered state by its type.
    /// </summary>
    /// <typeparam name="T">The state type to retrieve.</typeparam>
    /// <returns>The registered state instance, or null if it was not found.</returns>
    public T GetState<T>() where T : SheepStateBase
    {
        Type stateType = typeof(T);

        if (_states.TryGetValue(stateType, out SheepStateBase state))
        {
            return state as T;
        }

        return null;
    }

    /// <summary>
    /// Checks whether the currently active state is of the given type.
    /// </summary>
    /// <typeparam name="T">The state type to check.</typeparam>
    /// <returns>True if the current state matches the given type; otherwise false.</returns>
    public bool IsCurrentState<T>() where T : SheepStateBase
    {
        return _currentState is T;
    }

    /// <summary>
    /// Changes the active state to a registered state of the given type.
    /// The state must have been registered before this method is called.
    /// <typeparam name="T">The state type to change to.</typeparam>
    public void ChangeState<T>() where T : SheepStateBase
    {
        T nextState = GetState<T>();

        if (nextState == null)
            return;

        ChangeState(nextState);
    }

    /// <summary>
    /// Changes the active state.
    /// Exits the previous state, assigns the new state, and enters it.
    /// </summary>
    /// <param name="newState">The new state that should become active.</param>
    private void ChangeState(SheepStateBase newState)
    {
        if (newState == null)
            return;

        if (_currentState == newState)
            return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    /// <summary>
    /// Updates the currently active state.
    /// </summary>
    public void Tick()
    {
        _currentState?.Tick();
    }
}