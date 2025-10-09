using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    private int IdleHash => stateMachine.IdleHash;
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
        stateMachine.PlayerController.SprintAction += Sprint;
        stateMachine.Animator.CrossFadeInFixedTime(IdleHash,0f);
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
        if(stateMachine.HasMoveInput) stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine));
        if(!stateMachine.IsGrounded && !stateMachine.IsDashing) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine));
        if(stateMachine.IsAttacking) stateMachine.SwitchState(new PlayerAttackState(this.stateMachine, stateMachine.CurrentActionIndex));
        if(stateMachine.IsDashing) stateMachine.SwitchState(new PlayerDodgingState(this.stateMachine, -stateMachine.transform.forward));
        if(stateMachine.IsBlocking) stateMachine.SwitchState(new PlayerBlockState(this.stateMachine));
    }
}
