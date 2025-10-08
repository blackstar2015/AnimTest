using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    private readonly int DodgeHash = Animator.StringToHash("Dodge");
    private float _dashAnimLength;
    public PlayerDodgingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {        
        base.Enter();
        stateMachine.Animator.CrossFadeInFixedTime(DodgeHash,.1f);
        stateMachine.Animator.applyRootMotion = true;
        stateMachine.LookInCameraDirection = false;

    }
    public override void Exit()
    {  
        stateMachine.Animator.applyRootMotion = false;
        stateMachine.LookInCameraDirection = true;
        Debug.Log("asd");
        base .Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        float nextDashTime = stateMachine.LastDashTime + stateMachine.DashCooldown;
        if (Time.time > nextDashTime)
        {
            //float DashAnimLength = stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length;
            stateMachine.Dodge(stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length);
            stateMachine.LastDashTime = Time.time;
            if(stateMachine.rb.linearVelocity.magnitude <= .1f) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
            else stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine));
        }
       
    }
}
