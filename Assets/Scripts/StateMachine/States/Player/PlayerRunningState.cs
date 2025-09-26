using UnityEngine;

public class PlayerRunningState : PlayerBaseState
{
    public PlayerRunningState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        Debug.Log("Entering Running State");
        stateMachine.Controller.JumpEvent += Jump;
    }

    public override void Exit()
    {
        Debug.Log("Exiting Running State");
        stateMachine.Controller.JumpEvent -= Jump;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        Vector3 acceleration = stateMachine.HasMoveInput ? stateMachine.MoveInput * stateMachine.RunAccelerationFactor : stateMachine.rb.linearVelocity.normalized * -stateMachine.RunDeccelerationFactor;
        
        while(stateMachine.rb.linearVelocity.magnitude <= stateMachine.PlayerMaxRunSpeed)
        {
            stateMachine.rb.linearVelocity += acceleration * deltaTime;
            break;
        }

        if(stateMachine.rb.linearVelocity.magnitude >= stateMachine.PlayerMaxRunSpeed)
        {
            Debug.Log("Reached max velocity at " + stateMachine.rb.linearVelocity.magnitude);
        }

        if (stateMachine.rb.linearVelocity.magnitude <= .1f) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        else if (stateMachine.rb.linearVelocity.magnitude < stateMachine.PlayerMaxWalkSpeed) stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine));

        if (!stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine));


    }
}
