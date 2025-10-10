using UnityEngine;

public class PlayerWalkingState : PlayerBaseState
{
    //private int MovementHash => stateMachine.MovementHash;
    private int MovementHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].MovementHash);

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
        stateMachine.PlayerController.WeaponSwitchAction += WeaponSwitch;
        stateMachine.Animator.CrossFade(MovementHash, 0.1f);
    }

    public override void Exit()
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        stateMachine.PlayerController.DodgeAction -= Dodge;
        stateMachine.PlayerController.BlockAction -= Block;
        stateMachine.PlayerController.AttackAction -= Attack;
        stateMachine.PlayerController.SprintAction -= Sprint;
        stateMachine.PlayerController.WeaponSwitchAction -= WeaponSwitch;
        base .Exit();
    }


    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        stateMachine.rb.AddForce(stateMachine.LocalMoveInput * stateMachine.Speed * Time.deltaTime, ForceMode.Impulse);
        if (stateMachine.IsDashing) stateMachine.SwitchState(new PlayerDodgingState(this.stateMachine, stateMachine.LocalMoveInput));
        if (!stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine));
        if (stateMachine.Velocity.magnitude * stateMachine.LocalMoveInput == Vector3.zero) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        if(stateMachine.IsAttacking) stateMachine.SwitchState(new PlayerAttackState(this.stateMachine));
        if(stateMachine.IsBlocking) stateMachine.SwitchState(new PlayerBlockState(this.stateMachine));
    }
}
