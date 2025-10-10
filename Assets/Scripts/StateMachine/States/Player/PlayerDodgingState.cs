using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    private int _dodgeHash => stateMachine.DodgeHash;
    private int _airborneDashHash => stateMachine.AirborneDashHash;

    private Vector3 _dashDirection {  get; set; }

    public PlayerDodgingState(PlayerStateMachine stateMachine, Vector3 dashDirection) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        _dashDirection = dashDirection;
    }

    public override void Enter()
    {        
        base.Enter();
        stateMachine.Animator.applyRootMotion = true;
        stateMachine.LookInCameraDirection = false;
        stateMachine.SetLookDirection(_dashDirection);
    }
    public override void Exit()
    {
        stateMachine.Animator.applyRootMotion = false;
        stateMachine.LookInCameraDirection = true;
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        PerformDash();
        stateMachine.Invoke(nameof(stateMachine.SwitchToMovement), stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length);
    }

    private void PerformDash()
    {
        if(!stateMachine.IsDashing) return;
        stateMachine.IsDashing = false;
        stateMachine.Animator.CrossFade(_dodgeHash, 0.1f);
        stateMachine.rb.AddForce(_dashDirection * stateMachine.DashSpeed, ForceMode.Impulse);
        Debug.DrawRay(stateMachine.transform.position, _dashDirection * stateMachine.DashSpeed, Color.red, stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length);

    }
}
