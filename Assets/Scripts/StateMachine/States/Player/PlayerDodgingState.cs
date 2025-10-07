using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    private readonly int DodgeHash = Animator.StringToHash("Dodge");

    public PlayerDodgingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {        
        base.Enter();
        stateMachine.Animator.CrossFadeInFixedTime(DodgeHash, .1f);
    }
    public override void Exit()
    {        
        base .Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if (stateMachine.IsGrounded)
        {
            if (stateMachine.rb.linearVelocity.magnitude <= .1f)
            {
                stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
            }
            else
            {
                stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine));
            }
        }
    }
}
