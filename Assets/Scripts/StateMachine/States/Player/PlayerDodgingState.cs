using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    public PlayerDodgingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        stateMachine.PlayerController.BlockAction += Block;
        stateMachine.PlayerController.AttackAction += Attack;
        base.Enter();
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
    }
}
