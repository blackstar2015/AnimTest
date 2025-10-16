using UnityEngine;

public class PlayerWalkingState : PlayerBaseState
{
    //private int MovementHash => stateMachine.MovementHash;
    private int MovementHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].MovementHash);

    public PlayerWalkingState(PlayerStateMachine stateMachine, bool shouldFade) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        _shouldFade = shouldFade;
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
        stateMachine.MoveSpeedMultiplier = stateMachine.PlayerWalkSpeedMultiplier;
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
        stateMachine.rb.AddForce(stateMachine.LocalMoveInput * stateMachine.BaseSpeed * Time.deltaTime, ForceMode.Impulse);
        if (stateMachine.IsDashing) stateMachine.SwitchState(new PlayerDodgingState(this.stateMachine, stateMachine.LocalMoveInput, false));
        if (!stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine, false));
        if (stateMachine.Velocity.magnitude * stateMachine.LocalMoveInput == Vector3.zero) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine, true));
        if(stateMachine.IsAttacking) stateMachine.SwitchState(new PlayerAttackState(this.stateMachine, true));
        if(stateMachine.IsBlocking) stateMachine.SwitchState(new PlayerBlockState(this.stateMachine, false));
    }
}
