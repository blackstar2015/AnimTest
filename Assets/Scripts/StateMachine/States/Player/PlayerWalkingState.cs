using UnityEngine;

public class PlayerWalkingState : PlayerBaseState
{
    private readonly int MovementHash = Animator.StringToHash("Movement");

    public PlayerWalkingState(PlayerStateMachine stateMachine) : base(stateMachine)
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
        stateMachine.Animator.CrossFadeInFixedTime(MovementHash, .1f);

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
        stateMachine.rb.AddForce(stateMachine.LocalMoveInput * stateMachine.Speed * Time.deltaTime, ForceMode.Impulse);
        if (!stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine));
        if (stateMachine.Velocity.magnitude * stateMachine.LocalMoveInput == Vector3.zero) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        if(stateMachine.IsAttacking) stateMachine.SwitchState(new PlayerAttackState(this.stateMachine));
    }
}
