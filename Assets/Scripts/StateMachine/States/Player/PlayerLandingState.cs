using UnityEngine;

public class PlayerLandingState : PlayerBaseState
{
    private readonly int AirborneFallHash = Animator.StringToHash("AirborneFall");
    private readonly int AirborneLandHash = Animator.StringToHash("AirborneLand");
    private readonly int AirborneDashHash = Animator.StringToHash("AirborneDash");
    public PlayerLandingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        stateMachine.Animator.CrossFadeInFixedTime(AirborneFallHash, .1f);
    }
    public override void Exit() 
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        stateMachine.PlayerController.JumpAction -= Dodge;
        base.Exit(); 
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        stateMachine.rb.AddForce(-stateMachine.transform.up * stateMachine.LandingGravity);
        if (stateMachine.IsGrounded)
        {
            stateMachine.Animator.CrossFadeInFixedTime(AirborneLandHash, .1f);
            if(stateMachine.rb.linearVelocity.magnitude <= .1f)
            {
                stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
            }
            else
            {
                stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine));
            }
        }
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
        float nextDashTime = stateMachine.LastDashTime + stateMachine.DashCooldown - 1;
        if (Time.time > nextDashTime)
        {
            stateMachine.rb.linearVelocity = Vector3.zero;
            stateMachine.DashDirection = stateMachine.transform.forward;
            stateMachine.rb.AddForce(stateMachine.DashDirection * stateMachine.DashSpeed * 10);
            stateMachine.Animator.CrossFadeInFixedTime(AirborneDashHash, .1f);
            stateMachine.LastDashTime = Time.time;
        }
    }
}
