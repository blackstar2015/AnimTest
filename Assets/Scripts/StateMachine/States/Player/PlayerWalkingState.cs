using UnityEngine;

public class PlayerWalkingState : PlayerBaseState
{
    public PlayerWalkingState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();   
    }

    public override void Exit()
    {
        base.Exit();
    }


    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        stateMachine.rb.AddForce(stateMachine.LocalMoveInput * stateMachine.Speed * Time.deltaTime, ForceMode.Impulse);
        if(stateMachine.Velocity.magnitude * stateMachine.LocalMoveInput == Vector3.zero) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
            
       
    }
}
