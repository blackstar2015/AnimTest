using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    private int AirborneJumpHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneJumpHash);
    private int AirborneFlipHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneFlipHash);

    public PlayerAirborneState(PlayerStateMachine stateMachine, bool shouldFade) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        _shouldFade = shouldFade;
    }

    public override void Enter()
    {
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        if (stateMachine.JumpCounter == 1)
        {
            stateMachine.Animator.CrossFade(AirborneJumpHash, 0.1f);
        }
        else
        {
            stateMachine.Animator.CrossFade(AirborneFlipHash, .1f);
        }
        stateMachine.PlayerController.BlockAction += Block;
        base.Enter();
    }
    public override void Exit()
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        base .Exit();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if(stateMachine.IsDashing) stateMachine.SwitchState(new PlayerDodgingState(this.stateMachine, stateMachine.transform.forward, false));
        if (stateMachine.Velocity.y <=.1f) stateMachine.SwitchState(new PlayerLandingState(this.stateMachine, false));
    }

    protected override void Jump()
    {
        if (stateMachine.JumpCounter < stateMachine.MaxJumps)
        {
            float jumpVelocity = Mathf.Sqrt(2f * -stateMachine.Gravity * stateMachine.JumpHeight * Time.deltaTime);         
            stateMachine.Velocity = new Vector3(stateMachine.Velocity.x, jumpVelocity, stateMachine.Velocity.z);
            stateMachine.JumpCounter++;
            stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine, true));
        }
    }
    
}
