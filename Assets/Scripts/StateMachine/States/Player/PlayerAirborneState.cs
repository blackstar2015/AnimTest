using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    private Vector3 _momentum;
    public PlayerAirborneState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        stateMachine.Controller.JumpEvent += Jump;
    }

    public override void Exit()
    {
        stateMachine.Controller.JumpEvent -= Jump;
    }

    public override  void Jump()
    {
        if (stateMachine.JumpCounter < stateMachine.MaxJumps)
        {
            stateMachine.rb.linearVelocity = new Vector3(stateMachine.rb.linearVelocity.x, stateMachine.DoubleJumpForce, stateMachine.rb.linearVelocity.z);
            stateMachine.JumpCounter++;
        }       
    }

    public bool CheckWallRun()
    {
        if (!stateMachine.IsGrounded) return false;

        bool hit = Physics.SphereCast(stateMachine.transform.position, stateMachine.WallRunCheckRadius, stateMachine.LookDirection, out RaycastHit hitInfo, stateMachine.WallRunCheckDistance, stateMachine.WallRunLayer);

        return hit;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if(stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        //if (CheckWallRun()) stateMachine.SwitchState(new PlayerWallRunningState(this.stateMachine));
    }
}
