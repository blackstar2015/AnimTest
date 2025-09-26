using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        Debug.Log("Entering Idle State");
        stateMachine.Controller.JumpEvent += Jump;
    }

    public override void Exit()
    {
        Debug.Log("Exiting Idle State");
        stateMachine.Controller.JumpEvent -= Jump;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if (stateMachine.HasMoveInput) stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine));

        if(!stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine));
    }
}
