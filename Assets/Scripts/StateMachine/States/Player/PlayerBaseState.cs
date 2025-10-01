using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    public PlayerStateMachine stateMachine;
    private Vector3 _dashDirection;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        if(stateMachine.debugStateTransitions) Debug.Log("Entering " + stateMachine.CurrentState);
    }
    public override void Exit()
    {
        if (stateMachine.debugStateTransitions) Debug.Log("Exiting " + stateMachine.CurrentState);
    }
    public override void Tick(float deltaTime)
    {
        if (stateMachine.debugStateTransitions) Debug.Log("Current State " + stateMachine.CurrentState);
    }

    protected override void Jump()
    {
        // calculate jump velocity from jump height and gravity
        float jumpVelocity = Mathf.Sqrt(2f * -stateMachine.Gravity * stateMachine.JumpHeight);
        // override current y velocity but maintain x/z velocity
        stateMachine.Velocity = new Vector3(stateMachine.Velocity.x, jumpVelocity, stateMachine.Velocity.z);
    }

    protected override void Dodge()
    {
    }
    
    protected override void Attack()
    {

    }

    protected override void Block()
    {
        
    }
}
