/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SheepStateBase.cs
* Date    : 20.02.2026
* Author  : Eric Rosenberg
*
* Description :
* Defines the abstract base class for all sheep FSM states.
* Stores shared references to the controlled sheep, the owning finite state
* machine, and the sheep state settings. Provides virtual lifecycle methods
* that derived states can override.
*
* History :
* 20.02.2026 ER Created
******************************************************************************/

/// <summary>
/// Base class for all sheep finite state machine states.
/// Provides shared access to the controlled sheep, the owning FSM, and the state settings.
/// </summary>
public abstract class SheepStateBase
{
    protected readonly Sheep Sheep;
    protected readonly SheepFSM FSM;
    protected readonly SheepStateSettings Settings;

    protected SheepStateBase(Sheep sheep, SheepFSM fsm)
    {
        Sheep = sheep;
        FSM = fsm;
        Settings = sheep.StateSettings;
    }

    /// <summary>
    /// Called when the state becomes active.
    /// </summary>
    public virtual void Enter()
    { }

    /// <summary>
    /// Called every frame while the state is active.
    /// </summary>
    public virtual void Tick()
    { }

    /// <summary>
    /// Called when the state is exited.
    /// </summary>
    public virtual void Exit()
    { }
}