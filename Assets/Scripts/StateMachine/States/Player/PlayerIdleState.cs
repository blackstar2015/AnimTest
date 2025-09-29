using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        stateMachine.PlayerController.BlockAction += Block;
        stateMachine.PlayerController.AttackAction += Attack;
    }

    public override void Exit()
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        stateMachine.PlayerController.DodgeAction -= Dodge;
        stateMachine.PlayerController.BlockAction -= Block;
        stateMachine.PlayerController.AttackAction -= Attack;
        base .Exit();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        
        if(stateMachine.HasMoveInput) stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine));
        //if(!stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine));
    }


}
