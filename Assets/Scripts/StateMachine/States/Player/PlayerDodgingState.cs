using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    private readonly int DodgeHash = Animator.StringToHash("Dodge");
    private readonly int AirborneDashHash = Animator.StringToHash("AirborneDash");
    public PlayerDodgingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {        
        base.Enter();
        stateMachine.Animator.applyRootMotion = true;
        stateMachine.LookInCameraDirection = false;

    }
    public override void Exit()
    {
        stateMachine.Animator.applyRootMotion = false;
        stateMachine.LookInCameraDirection = true;
        base .Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if(stateMachine.IsGrounded)
        {
            Dash();
        }
        else
        {
            AirDash();
        }
    }
    private void Dash()
    {
        if (!stateMachine.CanMove) return;
        float nextDashTime = stateMachine.LastDashTime + stateMachine.DashCooldown;
        if (Time.time > nextDashTime)
        {
            float DashAnimLength = stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length;
            if(stateMachine.LocalMoveInput == Vector3.zero)
            {
                stateMachine.DashDirection = -stateMachine.transform.forward;
            }
            else
            {
                stateMachine.DashDirection = stateMachine.LocalMoveInput;
            }
            stateMachine.Dodge(DashAnimLength, stateMachine.DashDirection, DodgeHash);

            stateMachine.LastDashTime = Time.time;
            stateMachine.SwitchToMovement();
        }
    }

    private void AirDash()
    {
        if (!stateMachine.CanMove) return;
        float nextDashTime = stateMachine.LastDashTime + stateMachine.DashCooldown;
        if (Time.time > nextDashTime)
        {
            float DashAnimLength = stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length;
            stateMachine.rb.linearVelocity = Vector3.zero;
            stateMachine.DashDirection = stateMachine.transform.forward;
            stateMachine.rb.AddForce(stateMachine.DashDirection * stateMachine.DashSpeed * stateMachine.AirDashMultiplier);
            stateMachine.Animator.CrossFadeInFixedTime(AirborneDashHash, .1f);
            stateMachine.LastDashTime = Time.time;
        }
    }
}
