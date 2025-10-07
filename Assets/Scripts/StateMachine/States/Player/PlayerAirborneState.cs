using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    private Vector3 _momentum;
    private readonly int AirborneJumpHash = Animator.StringToHash("AirborneJump");
    private readonly int AirborneFlipHash = Animator.StringToHash("AirborneFlip");
    private readonly int AirborneDashHash = Animator.StringToHash("AirborneDash");
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
            stateMachine.Animator.CrossFadeInFixedTime(AirborneJumpHash, 0f);
        }
        else
        {
            stateMachine.Animator.CrossFadeInFixedTime(AirborneFlipHash, .1f);
        }
        //stateMachine.PlayerController.BlockAction += Block;
        //stateMachine.PlayerController.AttackAction += Attack;
        base.Enter();
    }
    public override void Exit()
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        stateMachine.PlayerController.DodgeAction -= Dodge;
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
        if(stateMachine.Velocity.y <=.1f) stateMachine.SwitchState(new PlayerLandingState(this.stateMachine));

        //if (CheckWallRun()) stateMachine.SwitchState(new PlayerWallRunningState(this.stateMachine));
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
    protected override void Dodge()
    {
        if (!stateMachine.CanMove) return;
        float nextDashTime = stateMachine.LastDashTime + stateMachine.DashCooldown;
        if (Time.time > nextDashTime)
        {
            float DashAnimLength = stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length;
            stateMachine.rb.linearVelocity = Vector3.zero;
            stateMachine.DashDirection = stateMachine.transform.forward;
            stateMachine.rb.AddForce(stateMachine.DashDirection * stateMachine.DashSpeed);
            stateMachine.Animator.CrossFadeInFixedTime(AirborneDashHash, .1f);
            stateMachine.LastDashTime = Time.time;
        }
    }
}
