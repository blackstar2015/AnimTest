using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    //private int AirborneJumpHash => stateMachine.AirborneJumpHash;
    //private int AirborneFlipHash => stateMachine.AirborneFlipHash;
    private int AirborneJumpHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneJumpHash);
    private int AirborneFlipHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneFlipHash);

    public PlayerAirborneState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
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
        //stateMachine.PlayerController.AttackAction += Attack;
        base.Enter();
    }
    public override void Exit()
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        //stateMachine.PlayerController.DodgeAction -= Dodge;
        //stateMachine.PlayerController.BlockAction -= Block;
        //stateMachine.PlayerController.AttackAction -= Attack;
        base .Exit();
    }
    //public bool CheckWallRun()
    //{
    //    if (!stateMachine.IsGrounded) return false;

    //    bool hit = Physics.SphereCast(stateMachine.transform.position, stateMachine.WallRunCheckRadius, stateMachine.LookDirection, out RaycastHit hitInfo, stateMachine.WallRunCheckDistance, stateMachine.WallRunLayer);

    //    return hit;
    //}

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if(stateMachine.IsDashing) stateMachine.SwitchState(new PlayerDodgingState(this.stateMachine, stateMachine.transform.forward));
        if (stateMachine.Velocity.y <=.1f) stateMachine.SwitchState(new PlayerLandingState(this.stateMachine));
    }

    protected override void Jump()
    {
        if (stateMachine.JumpCounter < stateMachine.MaxJumps)
        {
            // calculate jump velocity from jump height and gravity
            float jumpVelocity = Mathf.Sqrt(2f * -stateMachine.Gravity * stateMachine.JumpHeight);
            // override current y velocity but maintain x/z velocity
            stateMachine.Velocity = new Vector3(stateMachine.Velocity.x, jumpVelocity, stateMachine.Velocity.z);
            stateMachine.JumpCounter++;
            stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine));
        }
    }
    
}
