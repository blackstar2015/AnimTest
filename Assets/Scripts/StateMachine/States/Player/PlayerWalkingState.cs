using UnityEngine;

public class PlayerWalkingState : PlayerBaseState
{
    public PlayerWalkingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        stateMachine.Animator.SetFloat("Speed",stateMachine.rb.linearVelocity.magnitude);
        Debug.Log("Entering Walking State"); 
        stateMachine.Controller.JumpEvent += Jump;
    }

    public override void Exit()
    {
        Debug.Log("Exiting Walking State");
        stateMachine.Controller.JumpEvent -= Jump;
    }


    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        Vector3 acceleration = stateMachine.HasMoveInput ? stateMachine.MoveInput * stateMachine.WalkAccelerationFactor : stateMachine.rb.linearVelocity.normalized * -stateMachine.WalkDeccelerationFactor;

        stateMachine.rb.linearVelocity += acceleration * deltaTime;
        Mathf.Clamp(stateMachine.rb.linearVelocity.magnitude,0,stateMachine.PlayerMaxRunSpeed);
        Debug.Log(stateMachine.rb.linearVelocity.magnitude);


        if (stateMachine.rb.linearVelocity.magnitude <= .1f) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        else if (stateMachine.rb.linearVelocity.magnitude >= stateMachine.PlayerMaxWalkSpeed) stateMachine.SwitchState(new PlayerRunningState(this.stateMachine));

        if (!stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine));
    }
}
