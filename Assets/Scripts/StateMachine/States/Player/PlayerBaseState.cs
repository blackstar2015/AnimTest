using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    public PlayerStateMachine stateMachine;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {

    }
    public override void Exit()
    {

    }
    public override void Tick(float deltaTime)
    {

    }

    public virtual void Jump() { }

    public virtual void Dash() { }

    
}
