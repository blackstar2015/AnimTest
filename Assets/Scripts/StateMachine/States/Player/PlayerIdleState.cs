using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    //private int IdleHash => stateMachine.IdleHash;
    private int IdleHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].IdleHash);
    public PlayerIdleState(PlayerStateMachine stateMachine, bool shouldFade) : base(stateMachine)
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
        stateMachine.Animator.CrossFade(IdleHash,_crossfadeDuration);
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
        if(stateMachine.HasMoveInput) stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine, true));
        if(!stateMachine.IsGrounded && !stateMachine.IsDashing) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine, true));
        if(stateMachine.IsAttacking) stateMachine.SwitchState(new PlayerAttackState(this.stateMachine, true));
        if(stateMachine.IsDashing) stateMachine.SwitchState(new PlayerDodgingState(this.stateMachine, -stateMachine.transform.forward, false));
        if(stateMachine.IsBlocking) stateMachine.SwitchState(new PlayerBlockState(this.stateMachine, false));
    }
}
