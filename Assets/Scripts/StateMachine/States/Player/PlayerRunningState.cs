using UnityEngine;

public class PlayerRunningState : PlayerBaseState
{
    public PlayerRunningState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        stateMachine.PlayerController.BlockAction += Block;
        stateMachine.PlayerController.AttackAction += Attack;
        stateMachine.PlayerController.SprintAction += Sprint;
       base.Enter();
    }

    public override void Exit()
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        stateMachine.PlayerController.DodgeAction -= Dodge;
        stateMachine.PlayerController.BlockAction -= Block;
        stateMachine.PlayerController.AttackAction -= Attack;
        stateMachine.PlayerController.SprintAction -= Sprint;
        base .Exit();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
    }
}
